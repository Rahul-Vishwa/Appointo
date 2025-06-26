using Taskzen.DTOs;
using Taskzen.Entities;

namespace Taskzen.Interfaces;

public interface ISchedule
{
    Task<AddScheduleResultDto> SaveSchedule(AddScheduleDto schedule);
    Task<GetScheduleResultDto> GetSchedules(int page, int pageSize);
    Task<Schedule?> DeleteSchedule(int id, int userId);
}