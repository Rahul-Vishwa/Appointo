using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.Data.Migrations;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Helpers;
using Taskzen.Interfaces;
using Schedule = Taskzen.Entities.Schedule;

namespace Taskzen.Repositories;

public class AppointmentRepository(AppDbContext dbContext): IAppointment
{
    private string GetDayOfWeekInStr(DayOfWeek day) => day switch
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

    public async Task<Appointment?> EditAppointment(EditAppointmentDto appointment)
    {
        var existingAppointment = await dbContext.Appointments.FindAsync(appointment.Id);

        if (existingAppointment != null)
        {
            var currentDate = existingAppointment.Date;
            var currentTime = existingAppointment.Time;
            
            existingAppointment.Date = appointment.Date;
            existingAppointment.Time = appointment.Time;
            existingAppointment.ModifiedAt = DateTime.UtcNow;
            existingAppointment.ModifiedBy = appointment.ModifiedBy;
            
            await dbContext.SaveChangesAsync();
            await SendRescheduleMail(appointment.Role, (int)existingAppointment.CreatedBy, currentDate, currentTime, appointment.Date, appointment.Time);
                
            return existingAppointment;
        }
        
        return null;
    }
    
    private async Task SendRescheduleMail(string role, int userId, DateOnly currentDate, string currentTime, DateOnly appointmentDate, string appointmentTime)
    {
        var user = await dbContext.Users.FindAsync(userId);
        
        var parsedAppointmentTime = TimeOnly.ParseExact(appointmentTime, "HH:mm");
        var formattedTime = parsedAppointmentTime.ToString("hh:mm tt");
        
        var parsedCurrentTime = TimeOnly.ParseExact(currentTime, "HH:mm");
        var currentFormattedTime = parsedCurrentTime.ToString("hh:mm tt");

        if (user != null)
        {
            var givenName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.GivenName);
            var body = $@"
                <html>
                    <body>
                        <p>Hi {givenName},</p>
                        <p> {(role == "Admin" ? "Your appointment has been rescheduled due to some issue." : "Your appointment has been rescheduled successfully.")}</p>
                        <p><strong>Previous Appointment:</strong><br/>
                        Date: {currentDate}<br/>
                        Time: {currentFormattedTime}</p>
                        <p><strong>Rescheduled Appointment:</strong><br/>
                        Date: {appointmentDate}<br/>
                        Time: {formattedTime}</p>
                    </body>
                </html>
            ";
            
            await MailHelper.SendEmailAsync(user.Email, "Appointment Rescheduled", body);
        }
    }

    private async Task SendCancelMail(string role, int userId, DateOnly appointmentDate, string appointmentTime)
    {
        var user = await dbContext.Users.FindAsync(userId);
        
        var parsedAppointmentTime = TimeOnly.ParseExact(appointmentTime, "HH:mm");
        var formattedTime = parsedAppointmentTime.ToString("hh:mm tt");
        
        if (user != null)
        {
            var givenName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.GivenName);
            var body = $@"
                <html>
                    <body>
                        <p>Hi {givenName},</p>
                        <p> {(role == "Admin" ? "Your appointment has been cancelled due to some issue." : "Your appointment has been cancelled successfully.")}</p>
                        <p><strong>Cancelled Appointment:</strong><br/>
                        Date: {appointmentDate}<br/>
                        Time: {formattedTime}</p>
                    </body>
                </html>
            ";
            
            await MailHelper.SendEmailAsync(user.Email, "Appointment Cancelled", body);
        }
    }
    
    public async Task<Appointment?> DeleteAppointment(int id, int modifiedBy, string role)
    {
        var appointment = await dbContext.Appointments.FindAsync(id);

        if (appointment != null)
        {
            appointment.Active = false;
            appointment.ModifiedBy = modifiedBy;
            appointment.ModifiedAt = DateTime.UtcNow;
            
            await dbContext.SaveChangesAsync();
            await SendCancelMail(role, appointment.CreatedBy, appointment.Date, appointment.Time);
            
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
        };
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
}