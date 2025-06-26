using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Interfaces;

namespace Taskzen.Repositories;

public class ScheduleRepository(AppDbContext dbContext): ISchedule
{
    public async Task<AddScheduleResultDto> SaveSchedule(AddScheduleDto schedule)
    {
        var existingSchedule = await dbContext.Schedules.FirstOrDefaultAsync(s =>
            s.EffectiveFrom == schedule.EffectiveFrom &&
            s.Active == true
        );

        if (existingSchedule != null)
        {
            return new AddScheduleResultDto
            {
                Message = "Schedule already exists with same effective from."
            };
        }

        var appointments = await dbContext.Appointments.FirstOrDefaultAsync(a =>
            a.Date >= schedule.EffectiveFrom &&
            a.Active
        );

        if (appointments != null)
        {
            return new AddScheduleResultDto
            {
                Message = "Cannot update schedule because an appointment is already booked for the previous schedule."
            };
        }
        
        var newSchedule = new Schedule{
            DaysAvailable = schedule.DaysAvailable,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            BreakStartTime = schedule.BreakStartTime,
            BreakEndTime = schedule.BreakEndTime,
            SlotDuration = schedule.SlotDuration,
            EffectiveFrom = schedule.EffectiveFrom,
            CreatedBy = schedule.CreatedBy,
        };
        
        await dbContext.Schedules.AddAsync(newSchedule);
        await dbContext.SaveChangesAsync();
        return new AddScheduleResultDto
        {
            Schedule = newSchedule
        };
    }

    private async Task<bool> SetActiveSchedule()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        var schedules = await dbContext.Schedules
            .Where(s => s.EffectiveFrom <= date && s.Active == true)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();
        
        var schedule = schedules.FirstOrDefault();

        if (schedule != null && (schedule.IsActive == false || schedule.IsActive == null))
        {
            schedule.IsActive = true;
            var activeSchedule = await dbContext.Schedules.FirstOrDefaultAsync(
                s => s.IsActive == true && s.Active == true
            );
            if (activeSchedule != null)
            {
                activeSchedule.IsActive = false;
            }
            await dbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }
    
    public async Task<GetScheduleResultDto> GetSchedules(int page, int pageSize)
    {
        var updated = await SetActiveSchedule();
        
        var schedules = await dbContext.Schedules
            .Where(s => s.Active == true)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.CreatedByUser)
            .Select(s => new GetScheduleDto
            {
                Id = s.Id,
                DaysAvailable = s.DaysAvailable,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BreakStartTime = s.BreakStartTime,
                BreakEndTime = s.BreakEndTime,
                SlotDuration = s.SlotDuration,
                EffectiveFrom = s.EffectiveFrom,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.Name : null
            })
            .ToListAsync();

        return new GetScheduleResultDto
        {
            Schedule = schedules,
            TotalCount = schedules.Count()
        };
    }

    public async Task<Schedule?> DeleteSchedule(int id, int userId)
    {
        var schedule = await dbContext.Schedules.FindAsync(id);

        if (schedule != null)
        {
            schedule.Active = false;
            schedule.ModifiedBy = userId;
            schedule.ModifiedAt = DateTime.UtcNow;
            
            await dbContext.SaveChangesAsync();

            return schedule;
        }

        return null;
    }
}