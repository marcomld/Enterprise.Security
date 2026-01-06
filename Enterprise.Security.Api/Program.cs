using Enterprise.Security.Api.Extensions;
using Enterprise.Security.Api.Middleware;
using Enterprise.Security.Application;     // Para AddApplication
using Enterprise.Security.Infrastructure;
using Enterprise.Security.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.RateLimiting;  // Para AddInfrastructure

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Capas Core (Modularidad)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. API Services
builder.Services.AddControllers();
builder.Services.AddSwaggerWithJwt(); // Tu extensión

// HARDENING 4: Rate Limiter (Anti-DDoS / Brute Force)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política para el Login: Máximo 5 intentos por minuto por IP
    options.AddFixedWindowLimiter("auth-policy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0; // No encolar, rechazar directo
    });
});

// HARDENING 5: CORS Restrictivo (Prepara para Blazor/React)
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("ProductionCors", policy =>
//    {
//        policy.WithOrigins("https://misitio.com", "https://localhost:7000") // Pon aquí la URL de tu futuro Blazor
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


// 3. Auth Configuration
builder.Services.AddJwtAuthentication(builder.Configuration); // Tu extensión
builder.Services.AddAuthorization(); // Básico por ahora

var app = builder.Build();


// --- ZONA DE SEEDING ---
// Ejecutamos esto antes de abrir la API al tráfico
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var initializer = services.GetRequiredService<DbInitializer>();
        await initializer.RunAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la migración o el seeding de datos.");
    }
}

// HARDENING 6: Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

// 4. Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Manejo global de errores

// HARDENING 7: Swagger solo en Dev
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// Activar Rate Limiter y CORS antes de Auth
app.UseCors("AllowBlazorClient");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
