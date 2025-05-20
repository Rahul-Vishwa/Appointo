using Taskzen.DTOs;
using Taskzen.Entities;

namespace Taskzen.Interfaces;

public interface ISchedule
{
    Task<Schedule> SaveSchedule(AddScheduleDto schedule);
    Task<List<GetScheduleDto>> GetSchedules();
}