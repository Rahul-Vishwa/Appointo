using Taskzen.DTOs;
using Taskzen.Entities;

namespace Taskzen.Interfaces;

public interface IAppointmentActions
{
    Task<List<GetBookedSlotsWithDetailsDto>> GetBookedSlotsWithDetails(GetBookedSlotsDto slot);
    Task<GetAllAppointmentsResultDto> GetAllAppointments(int page, int pageSize, bool futureAppointments, bool active);
    Task<string?> ApplyLeave(ApplyLeaveDto leave);
    Task<string?> EditLeave(EditLeaveDto leave);
    Task<Leave?> CancelLeave(int id);
}