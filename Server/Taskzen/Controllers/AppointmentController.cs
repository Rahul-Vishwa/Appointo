using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Helpers;
using Taskzen.Interfaces;

namespace Taskzen.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Customer")]
[Route("[controller]")]
public class AppointmentController(IAppointment appointment, AppDbContext dbContext): ControllerBase
{
    [HttpGet("GetSlots/{date}")]
    public async Task<IActionResult> GetSlots(DateOnly date)
    {
        var slots = await appointment.GetSlots(date);
        return Ok(slots);
    }

    [HttpGet("GetUserBookedSlots")]
    public async Task<IActionResult> GetUserBookedSlots([FromQuery] GetBookedSlotsDto slot)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            slot.CreatedBy = (int)userId;            
            
            var bookedSlot = await appointment.GetUserBookedSlot(slot);
            return Ok(bookedSlot);
        }
        return BadRequest("User not found.");
    }
    
    [HttpPatch]
    public async Task<IActionResult> EditAppointment([FromBody] EditAppointmentDto editedAppointment)
    {
        var userId = await User.GetUserId(dbContext);
        var role = User.FindFirst("https://taskzen.com/role")?.Value;
        
        if (userId != null && role != null)
        {
            editedAppointment.ModifiedBy = (int)userId;            
            editedAppointment.Role = role;            
            
            var bookedSlot = await appointment.EditAppointment(editedAppointment);
            return Ok(bookedSlot);
        }
        return BadRequest("User not found.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var userId = await User.GetUserId(dbContext);
        var role = User.FindFirst("https://taskzen.com/role")?.Value;
        
        if (userId != null && role != null)
        {
            var deletedAppointment = await appointment.DeleteAppointment(id, (int)userId, role);
            if (deletedAppointment != null)
            {
                return NoContent();
            }
            return NotFound();
        }   
        return BadRequest("User not found.");
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserAppointments([FromQuery] int id, int page, int pageSize)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            var appointments = await appointment.GetUserAppointments(id == 0 ? (int)userId : id, page, pageSize);
            return Ok(appointments);
        }
        return BadRequest("User not found.");
    }
    
    [HttpGet("GetLeaveByDate")]
    public async Task<IActionResult> GetLeaveByDate(DateOnly date)
    {
        var leave = await appointment.GetLeaveByDate(date);
        return Ok(leave);
    }
}