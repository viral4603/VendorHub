using VendorHub.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerServices();
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Database connection
builder.Services.AddDatabaseServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VendorHub API v1");
        c.RoutePrefix = "swagger"; // makes Swagger UI load at localhost/swagger/ URL "/"
    });
}

// Middleware
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();