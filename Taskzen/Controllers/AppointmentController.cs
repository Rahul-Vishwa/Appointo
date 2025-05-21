using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Helpers;
using Taskzen.Interfaces;

namespace Taskzen.Controllers;

[ApiController]
[Authorize]
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
    
    [HttpGet("GetBookedSlots")]
    public async Task<IActionResult> GetBookedSlots([FromQuery] GetBookedSlotsDto slot)
    {
        var bookedSlots = await appointment.GetBookedSlots(slot); 
        return Ok(bookedSlots);
    }
    
    [HttpGet("GetBookedSlotsWithDetails")]
    public async Task<IActionResult> GetBookedSlotsWithDetails([FromQuery] GetBookedSlotsDto slot)
    {
        var bookedSlots = await appointment.GetBookedSlotsWithDetails(slot); 
        return Ok(bookedSlots);
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAppointment([FromBody] SaveAppointmentDto saveAppointment)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            saveAppointment.CreatedBy = (int)userId;
            
            var newAppointment = await appointment.SaveAppointment(saveAppointment);

            if (newAppointment.Success)
            {
                return CreatedAtAction(nameof(SaveAppointment), new { id = newAppointment.Appointment?.Id }, newAppointment);
            }

            return BadRequest(newAppointment.Message);
        }
        return BadRequest("User not found.");
    }
    
    [HttpPatch]
    public async Task<IActionResult> EditAppointment([FromBody] EditAppointmentDto editedAppointment)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            editedAppointment.ModifiedBy = (int)userId;            
            
            var bookedSlot = await appointment.EditAppointment(editedAppointment);
            return Ok(bookedSlot);
        }
        return BadRequest("User not found.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var deletedAppointment = await appointment.DeleteAppointment(id);
        if (deletedAppointment != null)
        {
            return NoContent();
        }
        
        return NotFound();
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

    [HttpGet("GetLeaveByDate")]
    public async Task<IActionResult> GetLeaveByDate(DateOnly date)
    {
        var leave = await appointment.GetLeaveByDate(date);
        return Ok(leave);
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