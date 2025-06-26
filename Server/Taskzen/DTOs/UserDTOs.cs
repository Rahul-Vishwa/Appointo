using System.Text.Json.Serialization;

namespace Taskzen.DTOs;

public record AddUserDto
{
    public required string Email { get; set; }            
    public required string Name { get; set; }        
    public required string Picture { get; set; }         
    public required string GivenName { get; set; }         
}

public record AssignRoleDto
{
    public required string Role { get; set; }
}

public record Auth0TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }
}

public record Auth0Role
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }
}