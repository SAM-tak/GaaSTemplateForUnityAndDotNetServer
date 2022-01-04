using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace YourGameServer;

public class JwtTokenGenarator
{
    public TokenValidationParameters TokenValidationParameters { get; init; }
    public SigningCredentials SigningCredentials { get; init; }

    public JwtTokenGenarator(IConfiguration config, IWebHostEnvironment env)
    {
        TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "YourGameServer",
            ValidAudience = "YourGameClient",
            IssuerSigningKey = new SymmetricSecurityKey(env.IsDevelopment()
                ? Encoding.UTF8.GetBytes(config.GetSection("Authentication:JWT")["SecretKey"])
                : RandomNumberGenerator.GetBytes(32)
            )
        };
        SigningCredentials = new SigningCredentials(TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// Create SecurityToken By Symmetry Key
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string CreateToken(long id)
    {
        var token = new JwtSecurityToken(
            issuer: TokenValidationParameters.ValidIssuer,
            audience: TokenValidationParameters.ValidAudience,
            claims: new[] { new Claim(ClaimTypes.Sid, id.ToString()) },
            expires: DateTime.Now.AddHours(1),
            signingCredentials: SigningCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
