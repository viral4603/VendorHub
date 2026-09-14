using VendorHub.AppService.Interfaces;
using VendorHub.AppService.Services;
using VendorHub.Infrastructure.Repositories;
using VendorHub.Infrastructure.Security;

namespace VendorHub.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}