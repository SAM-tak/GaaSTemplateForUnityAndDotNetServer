using System.Text;
using System.Security;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using YourGameServer.Data;
using Microsoft.AspNetCore.Authentication;

namespace YourGameServer;

public class JwtTokenGenarator
{
    public TokenValidationParameters TokenValidationParameters { get; init; }

    public int ExpireMinutes { get; }

    SigningCredentials SigningCredentials { get; init; }

    public JwtTokenGenarator(WebApplicationBuilder builder)
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        ExpireMinutes = jwtConfig.GetValue<int>("ExpireMinutes");
        if(builder.Environment.IsProduction() && ExpireMinutes <= 0) throw new SecurityException("Jwt.ExpireMinute must be over 0 in production.");
        var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection(jwtConfig["SecretKeyStore"])["SecretKey"]));
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
    /// Create SecurityToken By Symmetry Key
    /// </summary>
    /// <param name="playerId">Player Account Table index id</param>
    /// <param name="deviceId">Device Id</param>
    /// <returns>Token string</returns>
    public string CreateToken(ulong playerId, ulong deviceId)
    {
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: TokenValidationParameters?.ValidIssuer,
            audience: $"{TokenValidationParameters?.ValidAudience}/{playerId}/{deviceId}",
            expires: ExpireMinutes > 0 ? DateTime.Now.AddMinutes(ExpireMinutes) : null,
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

internal static class JwtTokenGenaratorExtentions
{
    public static AuthenticationBuilder AddJwtTokenGenerator(this AuthenticationBuilder builder, WebApplicationBuilder webAppBuilder)
    {
        var jwtTokenGenarator = new JwtTokenGenarator(webAppBuilder);
        builder.Services.AddSingleton(i => jwtTokenGenarator);
        return builder.AddJwtBearer(options => options.TokenValidationParameters = jwtTokenGenarator.TokenValidationParameters);
    }

    public static IApplicationBuilder UseJwtTokenGenerator(this IApplicationBuilder app)
    {
        var jwtTokenGenarator = app.ApplicationServices.GetService<JwtTokenGenarator>();
        if(jwtTokenGenarator is not null) jwtTokenGenarator.TokenValidationParameters.AudienceValidator = (audiences, securityToken, validationParameters) => {
            var candidate = audiences.Select(i => i.Split('/')).FirstOrDefault(i => i.Length == 3 && i[0] == validationParameters.ValidAudience);
            if(candidate is null) return false;
            var playerIdString = candidate[1];
            var deviceIdString = candidate[2];
            if(!string.IsNullOrEmpty(playerIdString) && !string.IsNullOrEmpty(deviceIdString)) {
                var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
                using var scope = serviceScopeFactory.CreateScope();
                var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                var headers = httpContextAccessor?.HttpContext?.Request.Headers;
                if(headers is null) return false;
                if(headers.ContainsKey("PlayerId")) headers.Remove("PlayerId");
                if(headers.ContainsKey("DeviceId")) headers.Remove("DeviceId");
                headers.Add("PlayerId", playerIdString);
                headers.Add("DeviceId", deviceIdString);
                var playerId = ulong.Parse(playerIdString);
                var deviceId = ulong.Parse(deviceIdString);
                var context = scope.ServiceProvider.GetService<GameDbContext>();
                var currentDeviceId = context?.PlayerAccounts.Where(i => i.Id == playerId).Select(i => i.CurrentDeviceId).FirstOrDefault();
                return currentDeviceId != null && currentDeviceId == deviceId;
            }
            return false;
        };
        return app;
    }
}
