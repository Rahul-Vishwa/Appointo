using Microsoft.AspNetCore.Mvc.RazorPages;
using Taskzen.DTOs;
using Taskzen.Entities;

namespace Taskzen.Interfaces;

public interface IAppointment
{
    Task<List<string>> GetSlots(DateOnly date);
    Task<GetUserBookedSlotsDto?> GetUserBookedSlot(GetBookedSlotsDto slot);
    Task<Appointment?> EditAppointment(EditAppointmentDto appointment);
    Task<Appointment?> DeleteAppointment(int id, int modifiedBy, string role);
    Task<GetUserAppointmentResultDto> GetUserAppointments(int createdBy, int page, int pageSize);
    Task<GetLeaveDto?> GetLeaveByDate(DateOnly date);
}