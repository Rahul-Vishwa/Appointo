namespace Taskzen.DTOs;

public record GetAnalyticsTodayDto
{
    public int Appointments { get; init; }
    public int UpcomingAppointments { get; init; }
    public int Cancellations { get; init; }
    public int OpenSlotsLeft { get; init; }
}

public record AppointmentCountByDateDto
{
    public int Count { get; init; }
    public required string Date { get; init; }
}


public record GetPercetageDto
{
    public required string Today { get; init; }
    public required string Month { get; init; }
}
