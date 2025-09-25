using System;
using Messenger.Domain.Entities;

namespace Messenger.Application.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(User user);
}

