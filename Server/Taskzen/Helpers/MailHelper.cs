using System.Net;
using System.Net.Mail;

namespace Taskzen.Helpers;

public static class MailHelper
{
    public static async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            string fromEmail = Environment.GetEnvironmentVariable("MAIL_ID")!;
            string appPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")!;
            
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}