namespace Taskzen.Entities;

public class Leave
{
    public int Id { get; init; }
    public DateOnly Date { get; set; }
    public required string LeaveType { get; set; }
    public required string? FromTime { get; set; }
    public required string? ToTime { get; set; }
    
    public DateTime CreatedAt { get; init; }
    public int CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public bool Active { get; set; } = true;

    public User? CreatedByUser { get; init; }
    public User? ModifiedByUser { get; init; }
}