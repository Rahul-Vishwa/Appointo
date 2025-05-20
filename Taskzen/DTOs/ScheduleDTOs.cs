using System.Runtime.InteropServices.JavaScript;

namespace Taskzen.DTOs;

public record AddScheduleDto
{
    public required string[] DaysAvailable { get; init; }
    public required string StartTime { get; init; }
    public required string EndTime { get; init; }
    public required string BreakStartTime { get; init; }
    public required string BreakEndTime { get; init; }
    public int SlotDuration { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    internal int CreatedBy { get; set; }
}
    
public record GetScheduleDto
{
    public int? Id { get; init; }
    public required string[] DaysAvailable { get; init; }
    public required string StartTime { get; init; }
    public required string EndTime { get; init; }
    public required string BreakStartTime { get; init; }
    public required string BreakEndTime { get; init; }
    public int SlotDuration { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
}