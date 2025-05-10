using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
      .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
      .AddTransforms(t =>
      {
          t.AddResponseTransform(ctx =>
          {
              ctx.HttpContext.Response.Headers.Add("X-Devoted-Gateway", "true");
              return ValueTask.CompletedTask;
          });
      });

var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(opts =>
      {
          opts.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = jwt["Issuer"],
              ValidAudience = jwt["Audience"],
              IssuerSigningKey = signingKey
          };
      });

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 100,
                QueueLimit = 0
            });
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();


app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true &&
        ctx.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        var snippet = token.Length > 15 ? token[..15] + "…" : token;

        app.Logger.LogInformation(
            "JWT {Snippet} accepted for {Method} {Path}",
            snippet,
            ctx.Request.Method,
            ctx.Request.Path);
    }

    await next();
});

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapReverseProxy().RequireAuthorization();

app.Run();