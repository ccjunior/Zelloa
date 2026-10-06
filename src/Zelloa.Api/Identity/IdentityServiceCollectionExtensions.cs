using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Zelloa.Application.Identity;
using Zelloa.Infrastructure.Identity;

namespace Zelloa.Api.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddZelloaIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ITenantContext, HttpTenantContext>();

        services.AddIdentity<ZelloaUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<Infrastructure.Persistence.ZelloaDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<ZelloaUser>, ZelloaUserClaimsPrincipalFactory>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Zelloa.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = "Zelloa.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("CanManageSchool", policy => policy.RequireRole(ZelloaRoles.SchoolAdmin))
            .AddPolicy("CanManageCatalog", policy => policy.RequireRole(ZelloaRoles.SchoolAdmin))
            .AddPolicy("CanOperateCafeteria", policy => policy.RequireRole(ZelloaRoles.CafeteriaOperator))
            .AddPolicy("CanViewStudent", policy => policy.RequireRole(
                ZelloaRoles.SchoolAdmin,
                ZelloaRoles.CafeteriaOperator,
                ZelloaRoles.Guardian))
            .AddPolicy("CanCreateOrder", policy => policy.RequireRole(ZelloaRoles.Guardian));

        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, appConfiguration) =>
        {
            var allowedOrigins = appConfiguration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            options.AddPolicy("ZelloaWeb", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    policy.SetIsOriginAllowed(_ => false);
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
