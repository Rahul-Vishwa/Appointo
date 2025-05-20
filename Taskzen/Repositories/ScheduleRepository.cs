using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Interfaces;

namespace Taskzen.Repositories;

public class ScheduleRepository(AppDbContext dbContext): ISchedule
{
    public async Task<Entities.Schedule> SaveSchedule(AddScheduleDto schedule)
    {
        var newSchedule = new Schedule{
            DaysAvailable = schedule.DaysAvailable,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            BreakStartTime = schedule.BreakStartTime,
            BreakEndTime = schedule.BreakEndTime,
            SlotDuration = schedule.SlotDuration,
            EffectiveFrom = schedule.EffectiveFrom,
            CreatedBy = schedule.CreatedBy
        };
        
        await dbContext.Schedules.AddAsync(newSchedule);
        await dbContext.SaveChangesAsync();
        return newSchedule;
    }

    public async Task<List<GetScheduleDto>> GetSchedules()
    {
        return await dbContext.Schedules
            .Include(s => s.CreatedByUser)
            .OrderByDescending(s => s.CreatedAt)
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
    }
}