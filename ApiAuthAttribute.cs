using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace YourGameServer;

internal class ApiAuthAttribute : AuthorizeAttribute
{
    public ApiAuthAttribute()
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }

    public ApiAuthAttribute(string policy) : base(policy)
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }
}
