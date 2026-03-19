// Creadores: Bayron Parra y Andres Alfaro
using CletaEatsBackend.Control;
using CletaEatsBackend.Datos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

// Inicializar base de datos al arranque
_ = DatabaseManager.Instance;

app.Run();
