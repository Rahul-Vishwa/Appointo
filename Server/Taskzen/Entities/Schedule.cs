using System.Runtime.InteropServices.JavaScript;

namespace Taskzen.Entities;

public class Schedule{
    public int Id { get; init; }
    public required string[] DaysAvailable { get; init; }
    public required string StartTime { get; init; }
    public required string EndTime { get; init; }
    public required string BreakStartTime { get; init; }
    public required string BreakEndTime { get; init; }
    public int SlotDuration { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public bool? IsActive { get; set; }
    public bool Active { get; set; } = true;
    
    public DateTime CreatedAt { get; init; }
    public int CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
    
    public User? CreatedByUser { get; init; }
    public User? ModifiedByUser { get; init; }
}
