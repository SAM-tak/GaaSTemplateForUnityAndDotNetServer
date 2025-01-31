using System.Text;
using System.Security;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace YourGameServer.Game;

public class JwtAuthorizer
{
    public TokenValidationParameters TokenValidationParameters { get; init; }

    public int ExpireMinutes { get; }

    SigningCredentials SigningCredentials { get; init; }

    public JwtAuthorizer(WebApplicationBuilder builder)
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        ExpireMinutes = jwtConfig.GetValue<int>("ExpireMinutes");
        if(builder.Environment.IsProduction() && ExpireMinutes <= 0) {
            throw new SecurityException("Jwt.ExpireMinute must be over 0 in production.");
        }
        var key = builder.Configuration.GetSection(jwtConfig["SecretKeyStore"] ?? string.Empty)["SecretKey"] ?? string.Empty;
        var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = ExpireMinutes > 0,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = issuerSigningKey
        };
        SigningCredentials = new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// return period
    /// </summary>
    /// <param name="baseTime">base time</param>
    /// <returns>expire date</returns>
    public DateTime ExpireDate(DateTime baseTime) => baseTime.AddMinutes(ExpireMinutes) + TokenValidationParameters.ClockSkew;

    /// <summary>
    /// Create SecurityToken By Symmetry Key
    /// </summary>
    /// <param name="playerId">Player Account Table index id</param>
    /// <param name="deviceId">Device Id</param>
    /// <param name="period">expire date</param>
    /// <returns>Token string</returns>
    public string CreateToken(ulong playerId, ulong deviceId, out DateTime period)
    {
        period = ExpireMinutes > 0 ? DateTime.UtcNow.AddMinutes(ExpireMinutes) : DateTime.MaxValue;
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: TokenValidationParameters?.ValidIssuer,
            audience: $"{TokenValidationParameters?.ValidAudience}/{playerId}/{deviceId}",
            expires: ExpireMinutes > 0 ? period : null,
            signingCredentials: SigningCredentials
        ));
    }

    /// <summary>
    /// Validate Token
    /// </summary>
    /// <param name="token">JWT token string (without 'Bearer')</param>
    /// <returns>is valid?</returns>
    public bool ValidateToken(string token)
    {
        return new JwtSecurityTokenHandler().ValidateToken(token, TokenValidationParameters, out var _) is not null;
    }
}

public static class JwtAuthorizerExtentions
{
    public static AuthenticationBuilder AddJwtAuthorizer(this AuthenticationBuilder builder, WebApplicationBuilder webAppBuilder)
    {
        var jwtAuthorizer = new JwtAuthorizer(webAppBuilder);
        builder.Services.AddSingleton(i => jwtAuthorizer);
        return builder.AddJwtBearer(options => options.TokenValidationParameters = jwtAuthorizer.TokenValidationParameters);
    }

    public static IApplicationBuilder UseJwtAuthorizer(this IApplicationBuilder app)
    {
        var jwtAuthorizer = app.ApplicationServices.GetService<JwtAuthorizer>();
        if(jwtAuthorizer is null) {
            return app;
        }

        jwtAuthorizer.TokenValidationParameters.AudienceValidator = (audiences, securityToken, validationParameters) => {
            var candidate = audiences.Select(i => i.Split('/')).FirstOrDefault(i => i.Length == 3 && i[0] == validationParameters.ValidAudience);
            if(candidate is null) {
                return false;
            }

            var playerIdString = candidate[1];
            var deviceIdString = candidate[2];
            if(!string.IsNullOrEmpty(playerIdString) && !string.IsNullOrEmpty(deviceIdString)
                && ulong.TryParse(playerIdString, out _) && ulong.TryParse(deviceIdString, out _)) {
                var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
                using var scope = serviceScopeFactory.CreateScope();
                var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                var headers = httpContextAccessor?.HttpContext?.Request.Headers;
                if(headers is null) {
                    return false;
                }
                headers["PlayerId"] = playerIdString;
                headers["DeviceId"] = deviceIdString;
                return true;
            }
            return false;
        };
        return app;
    }
}
