using Enterprise.Security.Api.Extensions;
using Enterprise.Security.Api.Middleware;
using Enterprise.Security.Application;     // Para AddApplication
using Enterprise.Security.Infrastructure;
using Enterprise.Security.Infrastructure.Persistence.Seed;  // Para AddInfrastructure

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Capas Core (Modularidad)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. API Services
builder.Services.AddControllers();
builder.Services.AddSwaggerWithJwt(); // Tu extensión



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();



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


// 4. Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Manejo global de errores

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
