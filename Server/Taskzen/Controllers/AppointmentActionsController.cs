using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Helpers;
using Taskzen.Interfaces;

namespace Taskzen.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class AppointmentActionsController(IAppointmentActions appointment, AppDbContext dbContext) : ControllerBase
{
    [HttpGet("GetBookedSlotsWithDetails")]
    public async Task<IActionResult> GetBookedSlotsWithDetails([FromQuery] GetBookedSlotsDto slot)
    {
        var bookedSlots = await appointment.GetBookedSlotsWithDetails(slot); 
        return Ok(bookedSlots);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAppointments(int page, int pageSize, bool futureAppointments, bool active)
    {
        var appointments = await appointment.GetAllAppointments(page, pageSize, futureAppointments, active);
        return Ok(appointments);
    }
    
    [HttpPost("ApplyLeave")]
    public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveDto leave)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            leave.CreatedBy = (int)userId;
            var rescheduledStatus = await appointment.ApplyLeave(leave);

            if (rescheduledStatus != null)
            {
                return Ok(new { status = rescheduledStatus });
            }
            return BadRequest("Leave already applied on same date.");
        }
        return BadRequest("User not found.");
    }

    [HttpPatch("EditLeave")]
    public async Task<IActionResult> EditLeave(EditLeaveDto leave)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            leave.ModifiedBy = (int)userId;
            
            var rescheduledStatus = await appointment.EditLeave(leave);
            if (rescheduledStatus != null)
            {
                return Ok(new { status = rescheduledStatus });
            }
            return BadRequest("No leave exists having the Id.");
        }
        return BadRequest("User not found.");
    }

    [HttpDelete("CancelLeave/{id}")]

    public async Task<IActionResult> CancelLeave(int id)
    {
        var canceledLeave = await appointment.CancelLeave(id);
        if (canceledLeave != null)
        {
            return NoContent();
        }
        return BadRequest("Leave not found.");
    }
}