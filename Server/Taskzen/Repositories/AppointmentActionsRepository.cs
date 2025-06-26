using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Interfaces;

namespace Taskzen.Repositories;

public class AppointmentActionsRepository(AppDbContext dbContext): IAppointmentActions
{
    public async Task<List<GetBookedSlotsWithDetailsDto>> GetBookedSlotsWithDetails(GetBookedSlotsDto slot)
    {
        return await dbContext.Appointments
            .Where(a => a.Date == slot.Date && a.Active == true)
            .Include(a => a.CreatedByUser)
            .Select(a=>new GetBookedSlotsWithDetailsDto
            {
                Id = a.Id,
                Time = a.Time,
                Name = a.CreatedByUser!.Name,
                UserId = a.CreatedBy
            })
            .ToListAsync();       
    }

    public async Task<GetAllAppointmentsResultDto> GetAllAppointments(int page, int pageSize, bool futureAppointments, bool active)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var nowTime = TimeOnly.FromDateTime(DateTime.Now);

        var baseQuery = dbContext.Appointments
            .Where(a => a.Active == active &&
                        ((futureAppointments && a.Date >= today) ||
                         (!futureAppointments && a.Date <= today)))
            .Include(a => a.CreatedByUser)
            .Include(a => a.ModifiedByUser)
            .Select(a => new GetAllAppointmentsDto
            {   
                Id = a.Id,
                Time = a.Time,
                Date = a.Date,
                CreatedBy = a.CreatedByUser!.Name,
                CreatedAt = a.CreatedAt,
                ModifiedAt = a.ModifiedAt,
                ModifiedBy = a.ModifiedByUser!.Name,
            });

        var rawAppointments = await baseQuery.ToListAsync();

        var filtered = rawAppointments
            .Where(a =>
            {
                var appointmentTime = TimeOnly.ParseExact(a.Time, "HH:mm");
                return futureAppointments
                    ? (a.Date > today || (a.Date == today && appointmentTime > nowTime))
                    : (a.Date < today || (a.Date == today && appointmentTime <= nowTime));
            });

        var sorted = futureAppointments
            ? filtered.OrderBy(a => a.Date).ThenBy(a => TimeOnly.ParseExact(a.Time, "HH:mm"))
            : filtered.OrderByDescending(a => a.Date).ThenByDescending(a => TimeOnly.ParseExact(a.Time, "HH:mm"));

        var paginated = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return new GetAllAppointmentsResultDto()
        {
            Appointments = paginated,
            TotalCount = filtered.Count()
        };
    }
    
    private TimeOnly convertToTimeOnly(string time)
    {
        return TimeOnly.ParseExact(time, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
    }
    
    string GetDayOfWeekInStr(DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday => "Sun",
        DayOfWeek.Monday => "Mon",
        DayOfWeek.Tuesday => "Tue",
        DayOfWeek.Wednesday => "Wed",
        DayOfWeek.Thursday => "Thu",
        DayOfWeek.Friday => "Fri",
        DayOfWeek.Saturday => "Sat",
        _ => throw new ArgumentException("Invalid Day"),
    };
    
    private async Task<Schedule?> GetScheduleByDate(DateOnly date)
    {
        var schedules = await dbContext.Schedules
            .Where(s => s.EffectiveFrom <= date && s.Active == true)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();
        
        var schedule = schedules.FirstOrDefault();
        if (schedule != null)
        {
            var day = date.DayOfWeek;
            if (schedule.DaysAvailable.Contains(GetDayOfWeekInStr(day)))
            {
                return schedule;
            }

            return null;
        }

        return null;
    }
    
    private async Task<List<string>> GetSlots(DateOnly date)
    {
        Schedule? schedule = await GetScheduleByDate(date);
        if (schedule == null)
        {
            return new List<string>();
        }
        List<string> slots = new List<string>();
        
        TimeOnly startTime = new TimeOnly(int.Parse(schedule.StartTime.Split(':')[0]), int.Parse(schedule.StartTime.Split(':')[1])); ;
        TimeOnly endTime = new TimeOnly(int.Parse(schedule.EndTime.Split(':')[0]), int.Parse(schedule.EndTime.Split(':')[1])); ;
        TimeOnly breakStartTime = new TimeOnly(int.Parse(schedule.BreakStartTime.Split(':')[0]), int.Parse(schedule.BreakStartTime.Split(':')[1])); ;
        TimeOnly breakEndTime = new TimeOnly(int.Parse(schedule.BreakEndTime.Split(':')[0]), int.Parse(schedule.BreakEndTime.Split(':')[1])); ;

        while (startTime < breakStartTime)
        {
            slots.Add(startTime.ToString("HH:mm"));
            startTime = startTime.AddMinutes(schedule.SlotDuration);
        }

        while (breakEndTime < endTime)
        {
            slots.Add(breakEndTime.ToString("HH:mm"));
            breakEndTime = breakEndTime.AddMinutes(schedule.SlotDuration);
        }


        return slots;
    }
    
    // Reschedule appointment to latest available slot starting from next day upto 10 days
    // Returns true is rescheduled and false is cancelled
    private async Task<bool> RescheduleAppointment(int id, int modifiedBy)
    {
        var appointment = await dbContext.Appointments.FindAsync(id);
        if (appointment is null)
            throw new InvalidOperationException("Appointment not found.");

        var startDate = appointment.Date.AddDays(1);
        var endDate = startDate.AddDays(10);

        var appointments = await dbContext.Appointments
            .Where(a =>
                a.Date >= startDate &&
                a.Date <= endDate &&
                a.Active == true
            )
            .ToListAsync();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var availableSlots = await GetSlots(date);
    
            var bookedSlots = appointments
                .Where(a => a.Date == date)
                .Select(a => a.Time)
                .ToHashSet();

            var freeSlot = availableSlots.FirstOrDefault(slot => !bookedSlots.Contains(slot));
            if (freeSlot != null)
            {
                appointment.Date = date;
                appointment.Time = freeSlot;
                appointment.ModifiedBy = modifiedBy;
                appointment.ModifiedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();
                return true;
            }
        }
            
        dbContext.Appointments.Remove(appointment);
        await dbContext.SaveChangesAsync();
        return false;
    }
    
    private async Task<string> RescheduleAllApointments(Leave leave)
    {
        var schedule = await GetScheduleByDate(leave.Date);
        if (schedule is null)
            throw new InvalidOperationException("Schedule not found.");
            
        var appointments = await dbContext.Appointments
            .Where(a => a.Date == leave.Date && a.Active == true)
            .ToListAsync(); 
            
        var filteredAppointments = appointments.Where(a =>
        {
            var appointmentTime = convertToTimeOnly(a.Time);

            if (leave.LeaveType == "full")
            {
                return true;
            }
            else if (leave.LeaveType == "firstHalf")
            {
                return convertToTimeOnly(schedule.StartTime) <= appointmentTime &&
                       appointmentTime <= convertToTimeOnly(schedule.BreakStartTime);
            }
            else if (leave.LeaveType == "secondHalf")
            {
                return convertToTimeOnly(schedule.BreakEndTime) <= appointmentTime &&
                       appointmentTime <= convertToTimeOnly(schedule.EndTime);
            }
            else
            {
                return convertToTimeOnly(leave.FromTime!) <= appointmentTime &&
                       appointmentTime <= convertToTimeOnly(leave.ToTime!);
            }
        }).ToList();

        var rescheduledCount = 0;
        var canceledCount = 0;
        foreach (var appointment in filteredAppointments)
        {
            var rescheduled = await RescheduleAppointment(appointment.Id, leave.CreatedBy);
            if (rescheduled)
            {
                rescheduledCount++;
            }
            else
            {
                canceledCount++;
            }
        }

        return (rescheduledCount > 0 ? $"{rescheduledCount} Appointments Rescheduled." : "") + 
               " " +
               (canceledCount > 0 ? $"{canceledCount} Appointments Canceled." : "");
    }
    
    public async Task<string?> ApplyLeave(ApplyLeaveDto leave)
    {
        var existingLeave = await dbContext.Leaves.FirstOrDefaultAsync(l => l.Date == leave.Date);

        if (existingLeave == null)
        {
            var newLeave = new Leave()
            {
                Date = leave.Date,
                LeaveType = leave.LeaveType,
                FromTime = leave.FromTime,
                ToTime = leave.ToTime,
                CreatedBy = leave.CreatedBy,
            };
            
            await dbContext.Leaves.AddAsync(newLeave);

            var rescheduled = await RescheduleAllApointments(newLeave);

            await dbContext.SaveChangesAsync();
            return rescheduled;
        }
        return null;
    }
    
    public async Task<string?> EditLeave(EditLeaveDto leave)
    {
        var existingLeave = await dbContext.Leaves.FindAsync(leave.Id);

        if (existingLeave != null)
        {    
            existingLeave.LeaveType = leave.LeaveType;
            existingLeave.FromTime = leave.FromTime;
            existingLeave.ToTime = leave.ToTime;
            existingLeave.ModifiedBy = leave.ModifiedBy;
            existingLeave.ModifiedAt = DateTime.UtcNow;
            
            
            var rescheduled = await RescheduleAllApointments(existingLeave);

            await dbContext.SaveChangesAsync();
            return rescheduled;
        }
        return null;
    }

    public async Task<Leave?> CancelLeave(int id)
    {
        var existingLeave = await dbContext.Leaves.FindAsync(id);

        if (existingLeave is not null)
        {
            dbContext.Leaves.Remove(existingLeave);
            await dbContext.SaveChangesAsync();
            
            return existingLeave;
        }
        return null;
    } 
}