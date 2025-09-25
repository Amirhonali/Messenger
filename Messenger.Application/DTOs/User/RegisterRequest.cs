using System;
namespace Messenger.Application.DTOs.User;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}

