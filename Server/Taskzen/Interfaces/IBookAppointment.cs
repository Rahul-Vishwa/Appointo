using Taskzen.DTOs;

namespace Taskzen.Interfaces;

public interface IBookAppointment
{
    Task<List<string>> GetBookedSlots(GetBookedSlotsDto slot);
    Task<SaveAppointmentResultDto> SaveAppointment(SaveAppointmentDto appointment);
}