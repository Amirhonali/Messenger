using Messenger.Application.DTOs;
using Messenger.Application.DTOs.User;
using Messenger.Application.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Messenger.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public UserService(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return new AuthResponse { Success = false, Message = "Username already exists" };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);

        return new AuthResponse
        {
            Success = true,
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || user.PasswordHash != HashPassword(request.Password))
        {
            return new AuthResponse { Success = false, Message = "Invalid credentials" };
        }

        var token = _authService.GenerateJwtToken(user);

        return new AuthResponse
        {
            Success = true,
            Token = token
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}