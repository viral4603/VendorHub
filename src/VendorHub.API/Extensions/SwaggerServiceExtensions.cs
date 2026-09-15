using Microsoft.OpenApi;

namespace VendorHub.API.Extensions;

public static class SwaggerServiceExtensions
{
    private const string SecuritySchemeId = "Bearer";

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "VendorHub API",
                Version = "v1",
                Description = "Multi-vendor e-commerce API built with ASP.NET Core, EF Core (Code-First)"
            });

            // Http/bearer rather than an ApiKey header, so the Authorize dialog takes
            // the raw token and Swagger adds the "Bearer " prefix itself.
            options.AddSecurityDefinition(SecuritySchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the token returned by /api/auth/login — without the \"Bearer \" prefix."
            });

            // Applied to every operation: Swagger cannot tell which endpoints carry
            // [Authorize], so the token is sent on all of them and anonymous ones
            // simply ignore it.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SecuritySchemeId, document)] = new List<string>()
            });
        });

        return services;
    }
}
