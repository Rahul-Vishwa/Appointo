namespace Taskzen.DTOs;

public record AddUserDto
{
    public required string Email { get; set; }            
    public required string Name { get; set; }        
    public required string Picture { get; set; }         
}