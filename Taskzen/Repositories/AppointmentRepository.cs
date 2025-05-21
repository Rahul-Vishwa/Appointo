using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.Data.Migrations;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Interfaces;
using Schedule = Taskzen.Entities.Schedule;

namespace Taskzen.Repositories;

public class AppointmentRepository(AppDbContext dbContext): IAppointment
{
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
    
    public async Task<List<string>> GetSlots(DateOnly date)
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
    
    public async Task<GetUserBookedSlotsDto?> GetUserBookedSlot(GetBookedSlotsDto slot)
    {
        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a =>
                a.Date == slot.Date && 
                a.CreatedBy == slot.CreatedBy &&
                a.Active == true
            );

        if (appointment != null)
        {
            return new GetUserBookedSlotsDto
            {
                Id = appointment.Id,
                Time = appointment.Time
            };
        }

        return null;
    }

    public async Task<List<string>> GetBookedSlots(GetBookedSlotsDto slot)
    {
        return await dbContext.Appointments
            .Where(a => a.Date == slot.Date && a.Active == true)
            .Select(a=>a.Time)
            .ToListAsync();       
    }

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
    
    public async Task<SaveAppointmentResultDto> SaveAppointment(SaveAppointmentDto appointment)
    {
        var appointmentCount = dbContext.Appointments.Count(a =>
            a.CreatedAt > new DateTime() &&
            a.CreatedBy == appointment.CreatedBy &&
            a.Active == true
        );

        if (appointmentCount >= 3)
        {
            return new SaveAppointmentResultDto
            {
                Success = false,
                Message = "Cannot have more than 3 active appointments."
            };
        }

        var existingAppointment = await dbContext.Appointments.FirstOrDefaultAsync(a =>
            a.Date == appointment.Date &&
            a.CreatedBy == appointment.CreatedBy &&
            a.Active == true
        );
        if (existingAppointment == null)
        {
            var newAppointment = new Appointment
            {
                Date = appointment.Date,
                Time = appointment.Time,
                CreatedBy = appointment.CreatedBy
            };
            await dbContext.Appointments.AddAsync(newAppointment);
            await dbContext.SaveChangesAsync();

            return new SaveAppointmentResultDto
            {
                Success = true,
                Appointment = newAppointment
            };;
        }

        return new SaveAppointmentResultDto
        {
            Success = false,
            Message = "Appointment already exists on the date."
        };
    }

    public async Task<Appointment?> EditAppointment(EditAppointmentDto appointment)
    {
        var existingAppointment = await dbContext.Appointments.FindAsync(appointment.Id);

        if (existingAppointment != null)
        {
            existingAppointment.Date = appointment.Date;
            existingAppointment.Time = appointment.Time;
            existingAppointment.ModifiedAt = DateTime.UtcNow;
            existingAppointment.ModifiedBy = appointment.ModifiedBy;
            
            await dbContext.SaveChangesAsync();
            return existingAppointment;
        }
        
        return null;
    }

    public async Task<Appointment?> DeleteAppointment(int id)
    {
        var appointment = await dbContext.Appointments.FindAsync(id);

        if (appointment != null)
        {
            appointment.Active = false;
            await dbContext.SaveChangesAsync();
            
            return appointment;
        }
        return null;
    }

    public async Task<GetUserAppointmentResultDto> GetUserAppointments(int createdBy, int page, int pageSize)
    {
        var user = await dbContext.Users
            .Include(u => u.CreatedAppointments)
            .ThenInclude(a => a.ModifiedByUser)
            .FirstOrDefaultAsync(u => u.Id == createdBy);
        
        if (user == null)
            return new GetUserAppointmentResultDto{
                Appointments = new List<GetUserAppointmentsDto>(),
                TotalCount = 0
            };
        
        var appointments = user.CreatedAppointments
            .Where(a => a.Active == true)
            .OrderByDescending(a => a.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new GetUserAppointmentsDto
            {
                Id = a.Id,
                Date = a.Date,
                Time = a.Time,
                CreatedAt = a.CreatedAt,
                ModifiedAt = a.ModifiedAt,
                ModifiedBy = a.ModifiedByUser?.Name
            })
            .ToList();
        
        return new GetUserAppointmentResultDto{
            Appointments = appointments,
            TotalCount = appointments.Count
        };;
    }
    
    public async Task<string?> ApplyLeave(ApplyLeaveDto leave)
    {
        var existingLeave = await dbContext.Leaves.FirstOrDefaultAsync(l => l.Date == leave.Date);

        if (existingLeave == null)
        {
            var newLeave = new Leave
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
    
    private TimeOnly convertToTimeOnly(string time)
    {
        return TimeOnly.ParseExact(time, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
    }
    
    public async Task<string> RescheduleAllApointments(Leave leave)
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

    public async Task<GetLeaveDto?> GetLeaveByDate(DateOnly date)
    {
        var leave = await dbContext.Leaves.FirstOrDefaultAsync(l => l.Date == date);
        if (leave is { LeaveType: "full" })
        {
            return new GetLeaveDto
            {
                Id = leave.Id,
                LeaveType = "full"
            };
        }
        
        var schedule = await GetScheduleByDate(date);
        if (schedule != null && leave != null)
        {
            if (leave.LeaveType == "firstHalf")
            {
                return new GetLeaveDto
                {
                    Id = leave.Id,
                    LeaveType = leave.LeaveType,
                    FromTime = schedule.StartTime,
                    ToTime = schedule.BreakStartTime,
                };
            }
            else if (leave.LeaveType == "secondHalf")
            {
                return new GetLeaveDto
                {
                    Id = leave.Id,
                    LeaveType = leave.LeaveType,
                    FromTime = schedule.BreakEndTime,
                    ToTime = schedule.EndTime,
                };
            }
            else
            {
                return new GetLeaveDto
                {
                    Id = leave.Id,
                    LeaveType = leave.LeaveType,
                    FromTime = leave.FromTime,
                    ToTime = leave.ToTime,
                };
            }
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