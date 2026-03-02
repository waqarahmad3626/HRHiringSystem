using HRHiringSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRHiringSystem.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;
    private readonly TimeSpan _tokenLifetime;

    public TokenService(IConnectionMultiplexer redis, IConfiguration config) 
    {
        _redis = redis;
        _config = config;
        int minutes = 60;
        var minutesCfg = _config["Jwt:ExpiresMinutes"];
        if (!string.IsNullOrEmpty(minutesCfg) && int.TryParse(minutesCfg, out var m)) minutes = m;
        _tokenLifetime = TimeSpan.FromMinutes(minutes);
    }

    public Task StoreTokenAsync(string token, TimeSpan expiresIn, System.Guid userId)
    {
        var db = _redis.GetDatabase(); 
        return db.StringSetAsync($"tokens:{token}", userId.ToString(), expiresIn); 
    }

    public async Task<string> GenerateTokenAsync(Guid userId, string? email, string? role)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        };

        if (!string.IsNullOrEmpty(email)) claims.Add(new Claim(ClaimTypes.Email, email));
        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.Add(_tokenLifetime);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        // store in redis
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"tokens:{tokenString}", userId.ToString(), _tokenLifetime);
        return tokenString;
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"tokens:{token}");
    }

    public async Task RevokeTokenAsync(string token)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"tokens:{token}");
    }
}
