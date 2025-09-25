using System;
namespace Messenger.Application.DTOs.User;

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
}

