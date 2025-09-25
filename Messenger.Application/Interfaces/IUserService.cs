using System;
using Messenger.Application.DTOs.User;

namespace Messenger.Application.Interfaces;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

