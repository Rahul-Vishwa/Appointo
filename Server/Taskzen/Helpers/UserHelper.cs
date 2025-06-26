using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Taskzen.Data;

namespace Taskzen.Helpers;

public static class UserHelper
{
    public static async Task<int?> GetUserId(this ClaimsPrincipal principal, AppDbContext dbContext)
    {
        var email = principal.FindFirst("https://taskzen.com/email")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }
        
        var userId = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return userId?.Id;
    }
}