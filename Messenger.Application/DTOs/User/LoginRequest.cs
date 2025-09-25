using System;
namespace Messenger.Application.DTOs.User;

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

