using Microsoft.EntityFrameworkCore;
using Taskzen.DTOs;
using Taskzen.Interfaces;
using Taskzen.Data;
using Taskzen.Entities;

namespace Taskzen.Repositories;

public class UserRepository: IUser
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<User> AddUser(AddUserDto user)
    {
        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
    
        if (existingUser == null)
        {
            User newUser = new User
            {
                Email = user.Email,
                Name = user.Name,
                Picture = user.Picture,
            };
            
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();
            
            return newUser;
        }

        return existingUser;
    }
}