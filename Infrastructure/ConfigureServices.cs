using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Duende.IdentityServer.Services;
using System.IdentityModel.Tokens.Jwt;
using Infrastructure.Interceptors;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<ApplicationDbContext>(options =>

                 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),

                 builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                  .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information)
                 );
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddDefaultIdentity<ApplicationUser>()
            .AddRoles<IdentityRole>().AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();



        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit =
               options.Password.RequireLowercase =
               options.Password.RequireNonAlphanumeric =
               options.Password.RequireUppercase = false;
            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.RequireUniqueEmail = true;

            //sign in settings
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.User.AllowedUserNameCharacters = null;
            options.User.RequireUniqueEmail = true;
        });
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            // enables immediate logout, after updating the user's stat.
            options.ValidationInterval = TimeSpan.Zero;
        });
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })

            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = false;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = configuration["JWT:Audience"],
                    ValidIssuer = configuration["JWT:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var securityStampClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == "uss")?.Value;
                        var userId = context.Principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                        var userService = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                        var user = await userService.FindByIdAsync(userId);
                        var userSecurityStamp = string.Empty;
                        if (user == null)
                        {
                            context.Fail("user not found");
                        }
                        else
                        {
                            userSecurityStamp = user.SecurityStamp;

                        }


                        // Compare the security stamp from the JWT with the one from the database
                        if (securityStampClaim != userSecurityStamp)
                        {
                            // Token is considered invalid
                            context.Fail("Security stamp mismatch");
                        }

                    },
                    OnMessageReceived = context =>
                    {
                        // Look for the access token in the query string for SignalR connections

                        var accessToken = context.Request.Query["token"];

                        // If the request is for the SignalR hub, set the token
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chathub")))
                        {
                            context.Token = accessToken;
                        }
                        //context.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjZkMDg1OWJkLWJmMzUtNGI3OC05ZDcwLWU4YWZmOWNmNmFhMiIsImVtYWlsIjoiYWRtaW5AbG9jYWxob3N0IiwidXNzIjoiWjNFV0I1QlRZWU4zNzIzQllJTTQ1Q0RQS09VTEc1UkQiLCJyb2xlcyI6WyJDb21wYW55IiwiQWRtaW4iLCJVc2VyUHJvIiwiVXNlciJdLCJleHAiOjE3NDUwODA2MTIsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0IiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3QifQ.WDHEMEJGkl-REGW3O7wqidTiMDnbI701YXuckNOVtDw";
                        return Task.CompletedTask;
                    }
                };
            });

        services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(15));

        services.AddAuthentication().AddIdentityServerJwt();
        services.AddAuthorization();


        return services;
    }
}
