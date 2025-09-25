using Messenger.Application.DTOs.User;
using Messenger.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        if (!result.Success) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        if (!result.Success) return Unauthorized(result.Message);
        return Ok(result);
    }
}
