using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Taskzen.DTOs;
using Taskzen.Interfaces;
using Taskzen.Data;
using Taskzen.Entities;

namespace Taskzen.Repositories;

public class UserRepository(AppDbContext dbContext, IConfiguration _configuration): IUser
{
    public async Task<User> AddUser(AddUserDto user)
    {
        User? existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
    
        if (existingUser == null)
        {
            User newUser = new User
            {
                Email = user.Email,
                Name = user.Name,
                Picture = user.Picture,
                GivenName = user.GivenName,
            };
            
            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();
            
            return newUser;
        }

        return existingUser;
    }

    public async Task<bool> AssignRole(string roleId, string userId)
    {
        var client = new HttpClient();
        
        string auth0ClientId = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID")!;
        string auth0ClientSecret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET")!;
        string domain = _configuration["Auth0:Domain"]!;
        
        var tokenRequest = new Dictionary<string, string>
        {
            { "client_id", auth0ClientId },
            { "client_secret", auth0ClientSecret },
            { "audience", $"https://{domain}/api/v2/" },
            { "grant_type", "client_credentials" },
        };

        var tokenResponse = await client.PostAsync($"https://{domain}/oauth/token", new FormUrlEncodedContent(tokenRequest));
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var error = await tokenResponse.Content.ReadAsStringAsync();
            return false;
        }
        
        var content = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(content);
        var mgmtToken = tokenData.GetProperty("access_token").GetString();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mgmtToken);

        var assignRolePayload = new StringContent(
            JsonSerializer.Serialize(new { roles = new[] { roleId } }),
            Encoding.UTF8,
            "application/json"
        );

        var apiResponse = await client.PostAsync(
            $"https://{domain}/api/v2/users/{Uri.EscapeDataString(userId)}/roles",
            assignRolePayload
        );
        
        return apiResponse.IsSuccessStatusCode;
    }
}