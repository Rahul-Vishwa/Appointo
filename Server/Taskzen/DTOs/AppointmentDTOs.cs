using System.Security.Cryptography.X509Certificates;
using Taskzen.Entities;

namespace Taskzen.DTOs;

public record SaveAppointmentResultDto 
{
    public Appointment? Appointment { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public record SaveAppointmentDto
{
    public DateOnly Date { get; init; }
    public required string Time { get; init; }
    internal int CreatedBy { get; set; }
}

public record GetBookedSlotsDto
{
    public DateOnly Date { get; init; }
    internal int CreatedBy { get; set; }
}

public record GetBookedSlotsWithDetailsDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string Time { get; init; }
    public required string Name {get; init; }
}

public record GetUserBookedSlotsDto
{
    public int Id { get; init; }
    public required string Time { get; init; }
}

public record EditAppointmentDto
{
    public int Id { get; init; }
    public DateOnly Date { get; init; }
    public required string Time { get; init; }
    internal int ModifiedBy { get; set; }
    internal string Role { get; set; }
}

public record GetUserAppointmentResultDto
{
    public required List<GetUserAppointmentsDto> Appointments { get; init; }
    public int TotalCount { get; init; }
}

public record GetAllAppointmentsResultDto
{   
    public required List<GetAllAppointmentsDto> Appointments { get; init; }
    public int TotalCount { get; init; }
}

public record GetAllAppointmentsDto
{
    public int Id { get; init; }
    public DateOnly Date { get; init; }
    public required string Time { get; init; }
    public required string CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record GetUserAppointmentsDto
{
    public int Id { get; init; }
    public DateOnly Date { get; init; }
    public required string Time { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record ApplyLeaveDto
{
    public DateOnly Date { get; init; }
    public required string LeaveType { get; init; }
    public required string? FromTime { get; init; }
    public required string? ToTime { get; init; }

    internal int CreatedBy { get; set; }
}

public record GetLeaveDto
{
    public int Id { get; init; }
    public required string LeaveType { get; init; }
    public string? FromTime { get; init; }
    public string? ToTime { get; init; }
}

public record EditLeaveDto
{
    public int Id { get; init; }
    public required string LeaveType { get; init; }
    public required string? FromTime { get; init; }
    public required string? ToTime { get; init; }

    internal int ModifiedBy { get; set; }
}

