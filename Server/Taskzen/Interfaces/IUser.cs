using Taskzen.DTOs;
using Taskzen.Entities;

namespace Taskzen.Interfaces;

public interface IUser
{
    Task<User> AddUser(AddUserDto user);
    Task<bool> AssignRole(string role, string userId);
}