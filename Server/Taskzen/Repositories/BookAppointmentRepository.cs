using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Entities;
using Taskzen.Helpers;
using Taskzen.Interfaces;

namespace Taskzen.Repositories;

public class BookAppointmentRepository(AppDbContext dbContext): IBookAppointment
{
    public async Task<List<string>> GetBookedSlots(GetBookedSlotsDto slot)
    {
        return await dbContext.Appointments
            .Where(a => a.Date == slot.Date && a.Active == true)
            .Select(a=>a.Time)
            .ToListAsync();       
    }
    
    public async Task<SaveAppointmentResultDto> SaveAppointment(SaveAppointmentDto appointment)
    {
        
        var appointmentsFromNow = dbContext.Appointments.Where(a =>
            a.Date >= appointment.Date &&
            a.CreatedBy == appointment.CreatedBy &&
            a.Active
        ).ToList();
        
        var time = TimeOnly.ParseExact(appointment.Time, "HH:mm");
        var appointmentCount = appointmentsFromNow
            .Count(a =>
                TimeOnly.ParseExact(a.Time, "HH:mm") >= time
            );

        if (appointmentCount >= 3)
        {
            return new SaveAppointmentResultDto
            {
                Success = false,
                Message = "Cannot have more than 3 active appointments."
            };
        }

        var existingAppointment = await dbContext.Appointments.FirstOrDefaultAsync(a =>
            a.Date == appointment.Date &&
            a.CreatedBy == appointment.CreatedBy &&
            a.Active == true
        );
        if (existingAppointment == null)
        {
            var newAppointment = new Appointment()
            {
                Date = appointment.Date,
                Time = appointment.Time,
                CreatedBy = appointment.CreatedBy
            };
            await dbContext.Appointments.AddAsync(newAppointment);
            await dbContext.SaveChangesAsync();

            await SendMail(appointment.CreatedBy, appointment.Date, appointment.Time);

            return new SaveAppointmentResultDto
            {
                Success = true,
                Appointment = newAppointment
            };;
        }

        return new SaveAppointmentResultDto
        {
            Success = false,
            Message = "Appointment already exists on the date."
        };
    }

    private async Task SendMail(int userId, DateOnly appointmentDate, string appointmentTime)
    {
        var user = await dbContext.Users.FindAsync(userId);
        
        var parsedAppointmentTime = TimeOnly.ParseExact(appointmentTime, "HH:mm");
        var formattedTime = parsedAppointmentTime.ToString("hh:mm tt");

        if (user != null)
        {
            var givenName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.GivenName);
            var body = $@"
                <html>
                    <body>
                        <p>Hi {givenName},</p>
                        <p>Your appointment has been booked successfully.</p>
                        <p><strong>Details:</strong><br/>
                        Date: {appointmentDate}<br/>
                        Time: {formattedTime}</p>
                    </body>
                </html>
            ";
            
            await MailHelper.SendEmailAsync(user.Email, "Appointment Booked", body);
        }
    }
}