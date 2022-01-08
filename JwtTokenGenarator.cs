using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Linq;
using YourGameServer.Data;
using Microsoft.AspNetCore.Routing;

namespace YourGameServer;

public class JwtTokenGenarator
{
    public TokenValidationParameters TokenValidationParameters { get; init; }
    public SigningCredentials SigningCredentials { get; init; }

    public int ExpireMinutes { get; }

    internal IServiceProvider _serviceProvider;

    public JwtTokenGenarator(IConfiguration config)
    {
        var jwtConfig = config.GetSection("Jwt");
        ExpireMinutes = jwtConfig.GetValue<int>("ExpireMinutes");
        TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = ExpireMinutes > 0,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config.GetSection(jwtConfig["SecretKeyStore"])["SecretKey"])
            ),
            AudienceValidator = (audiences, securityToken, validationParameters) => {
                if(_serviceProvider != null) {
                    var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                    using var scope = serviceScopeFactory.CreateScope();
                    var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                    var pid = (string)httpContextAccessor.HttpContext.GetRouteValue("pid");
                    var deviceIdString = httpContextAccessor.HttpContext.Request.Headers["DeviceId"];
                    if(!string.IsNullOrEmpty(pid) && !string.IsNullOrEmpty(deviceIdString)
                    && audiences.Any(i => i == $"{validationParameters.ValidAudience}/{pid}/{deviceIdString}")) {
                        var playerId = long.Parse(pid);
                        var deviceId = long.Parse(deviceIdString);
                        var context = scope.ServiceProvider.GetService<GameDbContext>();
                        var playerAccount = context.PlayerAccounts.Find(playerId);
                        if (playerAccount != null)
                        {
                            return playerAccount.CurrentDeviceId == deviceId;
                        }
                    }
                }
                return false;
            }
        };
        SigningCredentials = new SigningCredentials(TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// Create SecurityToken By Symmetry Key
    /// </summary>
    /// <param name="playerId">Player Account Table index id</param>
    /// <param name="deviceId">Device Id</param>
    /// <returns>Token string</returns>
    public string CreateToken(long playerId, long deviceId)
    {
        var token = new JwtSecurityToken(
            issuer: TokenValidationParameters.ValidIssuer,
            audience: $"{TokenValidationParameters.ValidAudience}/{playerId}/{deviceId}",
            expires: ExpireMinutes > 0 ? DateTime.Now.AddMinutes(ExpireMinutes) : null,
            signingCredentials: SigningCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// internal static class WebApplicationBuilderJwtTokenGenaratorExtention
// {
//     public static WebApplicationBuilder AddJwtTokenGenarator(this WebApplicationBuilder builder)
//     {
//         var jwtTokenGenarator = new JwtTokenGenarator(builder.Configuration);
//         if(builder.Environment.IsProduction() && jwtTokenGenarator.ExpireMinutes <= 0) throw new SecurityException("Jwt.ExpireMinute must be over 0 in production.");
//         builder.Services.AddSingleton(i => jwtTokenGenarator);
//         return builder;
//     }
// }

// internal static class ApplicationBuilderJwtTokenGenaratorExtention
// {
//     public static IApplicationBuilder UseJwtTokenGenarator(this IApplicationBuilder app)
//     {
//         app.ApplicationServices.GetService<JwtTokenGenarator>()._serviceProvider = app.ApplicationServices;
//         return app;
//     }
// }
