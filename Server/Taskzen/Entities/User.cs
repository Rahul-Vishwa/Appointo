using System.Text.Json.Serialization;

namespace Taskzen.Entities;

public class User
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? Picture { get; init; }
    public required string GivenName { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool Active { get; set; } = true;
    
    [JsonIgnore]
    public ICollection<Appointment> CreatedAppointments { get; set; } = new List<Appointment>();
    [JsonIgnore]
    public ICollection<Appointment> ModifiedAppointments { get; set; } = new List<Appointment>();
    
    [JsonIgnore]
    public ICollection<Leave> CreatedLeaves { get; set; } = new List<Leave>();
    [JsonIgnore]
    public ICollection<Leave> ModifiedLeaves { get; set; } = new List<Leave>();
}