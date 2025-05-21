using Microsoft.AspNetCore.Mvc.RazorPages;
using Taskzen.DTOs;
using Taskzen.Entities;

namespace Taskzen.Interfaces;

public interface IAppointment
{
    Task<List<string>> GetSlots(DateOnly date);
    Task<GetUserBookedSlotsDto?> GetUserBookedSlot(GetBookedSlotsDto slot);
    Task<List<string>>  GetBookedSlots(GetBookedSlotsDto slot);
    Task<List<GetBookedSlotsWithDetailsDto>> GetBookedSlotsWithDetails(GetBookedSlotsDto slot);
    Task<SaveAppointmentResultDto> SaveAppointment(SaveAppointmentDto appointment);
    Task<Appointment?> EditAppointment(EditAppointmentDto appointment);
    Task<Appointment?> DeleteAppointment(int id);
    Task<GetUserAppointmentResultDto> GetUserAppointments(int createdBy, int page, int pageSize);
    Task<string?> ApplyLeave(ApplyLeaveDto leave);
    Task<GetLeaveDto?> GetLeaveByDate(DateOnly date);
    Task<string?> EditLeave(EditLeaveDto leave);
    Task<Leave?> CancelLeave(int id);
}