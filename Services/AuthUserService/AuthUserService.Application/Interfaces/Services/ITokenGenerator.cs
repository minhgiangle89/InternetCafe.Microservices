﻿using AuthUserService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthUserService.Application.Interfaces.Services
{
    public interface ITokenGenerator
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromToken(string token);
        int TokenExpirationInMinutes { get; }
    }
}
