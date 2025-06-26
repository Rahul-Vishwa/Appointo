using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Interfaces;

namespace Taskzen.Repositories;

public class DashboardRepository(AppDbContext dbContext, IAppointment _appointment): IDashboard
{
    private async Task<List<Appointment>> GetAppointments(DateOnly date)
    {
        return await dbContext.Appointments
            .Where(a =>
                a.Date == date
            )
            .ToListAsync();
    }

    private async Task<int> GetUpcomingAppointmentsCount(List<Appointment> appointments)
    {
        int count = 0;
        if (appointments.Count > 0)
        {
            foreach (var appointment in appointments)
            {
                var parsedTime = new TimeOnly(int.Parse(appointment.Time.Split(':')[0]), int.Parse(appointment.Time.Split(':')[1]));
                if (parsedTime > TimeOnly.FromDateTime(DateTime.Now) && appointment.Active)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private int GetCancellationsCount(List<Appointment> appointments)
    {
        return appointments.Count(s => !s.Active);
    }

    private async Task<int> GetOpenSlotsLeftCount(int appointmentsCount)
    {
        var slots = await _appointment.GetSlots(DateOnly.FromDateTime(DateTime.Now));
        return slots.Count - appointmentsCount;
    }
    
    public async Task<GetAnalyticsTodayDto> GetAnalyticsToday()
    {
        var appointments = await GetAppointments(DateOnly.FromDateTime(DateTime.Now));
        var appointmentsCount = appointments.Count;

        var upcomingAppointments = 0; 
        var cancellations = 0; 
        if (appointmentsCount > 0)
        {
            upcomingAppointments = await GetUpcomingAppointmentsCount(appointments);
            cancellations = GetCancellationsCount(appointments);
        }
        var openSlotsLeft = await GetOpenSlotsLeftCount(appointmentsCount);

        return new GetAnalyticsTodayDto
        {
            Appointments = appointmentsCount,
            UpcomingAppointments = upcomingAppointments,
            Cancellations = cancellations,
            OpenSlotsLeft = openSlotsLeft
        };
    }

    private async Task<List<AppointmentCountByDateDto>> GetAppointmentsCount(List<Appointment> appointments, DateOnly currentDate, DateOnly previousDate)
    {
        var appointmentsCount = new List<AppointmentCountByDateDto>();
        
        if (appointments.Any())
        {
            for (var date = previousDate; date <= currentDate; date = date.AddDays(1))
            {
                var count = appointments.Count(a => a.Date == date);
                appointmentsCount.Add(new AppointmentCountByDateDto
                {
                    Count = count,
                    Date = date.ToString("yyyy-MM-dd ddd"),
                });
            }
        }

        return appointmentsCount;
    }

    private async Task<List<AppointmentCountByDateDto>> GetAppointmentsCountByDate(DateOnly currentDate, DateOnly previousDate)
    {
        var appointments = await dbContext.Appointments
            .Where(a =>
                a.Date >= previousDate &&
                a.Date <= currentDate &&
                a.Active
            )
            .ToListAsync();
        
        return await GetAppointmentsCount(appointments, currentDate, previousDate);
    }

    public async Task<List<AppointmentCountByDateDto>> GetAppointmentCountPast7Days()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        var previousDate = currentDate.AddDays(-6);

        return await GetAppointmentsCountByDate(currentDate, previousDate);
    }
    
    public async Task<List<AppointmentCountByDateDto>> GetAppointmentCountThisMonth()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        var firstDate = new DateTime(currentDate.Year, currentDate.Month, 1);
        var previousDate = DateOnly.FromDateTime(firstDate);

        return await GetAppointmentsCountByDate(currentDate, previousDate);
    }

    private async Task<List<AppointmentCountByDateDto>> GetCancelledAppointmentsCountByDate(DateOnly currentDate, DateOnly previousDate)
    {
        var appointments = await dbContext.Appointments
            .Where(a =>
                a.Date >= previousDate &&
                a.Date <= currentDate &&
                !a.Active
            )
            .ToListAsync();
        
        return await GetAppointmentsCount(appointments, currentDate, previousDate);
    }
    
    public async Task<List<AppointmentCountByDateDto>> GetCancelledApointmentsPast7Days()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        var previousDate = currentDate.AddDays(-6);

        return await GetCancelledAppointmentsCountByDate(currentDate, previousDate);
    }
    
    public async Task<List<AppointmentCountByDateDto>> GetCancelledApointmentsThisMonth()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        var firstDate = new DateTime(currentDate.Year, currentDate.Month, 1);
        var previousDate = DateOnly.FromDateTime(firstDate);

        return await GetCancelledAppointmentsCountByDate(currentDate, previousDate);
    }

    private string GetChangeString(int newValue, int oldValue)
    {
        if (oldValue == 0)
        {
            return newValue == 0 ? "0% Increase" : "100% Increase";
        }

        decimal change = ((decimal)(newValue - oldValue) / oldValue) * 100;
        string direction = change > 0 ? "Increase" : "Decrease";
        return Math.Abs(change).ToString("0.##") + "% " + direction;
    }
    
    private static (DateOnly Start, DateOnly End) GetMonthDateRange(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return (start, end);
    }
    
    public async Task<GetPercetageDto> GetPercentages()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        var todayCount = (await GetAppointments(today)).Count(a => a.Active);
        var yesterdayCount = (await GetAppointments(yesterday)).Count(a => a.Active);

        var now = DateTime.Now;
        var currentRange = GetMonthDateRange(now.Year, now.Month);
        var currentMonthCount = await dbContext.Appointments
            .Where(a =>
                a.Date >= currentRange.Start &&
                a.Date <= currentRange.End &&
                !a.Active
            )
            .CountAsync();

        var prev = now.AddMonths(-1);
        var previousRange = GetMonthDateRange(prev.Year, prev.Month);
        var previousMonthCount = await dbContext.Appointments
            .Where(a =>
                a.Date >= previousRange.Start &&
                a.Date <= previousRange.End &&
                !a.Active
            )
            .CountAsync();

        return new GetPercetageDto
        {
            Today = GetChangeString(todayCount, yesterdayCount),
            Month = GetChangeString(currentMonthCount, previousMonthCount),
        };
    } 
}