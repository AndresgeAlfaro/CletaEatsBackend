// Creadores: Bayron Parra y Andres Alfaro
using CletaEatsBackend.Datos;
using CletaEatsBackend.Infra;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SupabaseConnectionOptions>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<SupabaseRestService>((_, client) =>
{
    var url = builder.Configuration["Supabase:Url"]?.TrimEnd('/');
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("Falta Supabase:Url en appsettings.json (o variables de entorno).");
    client.BaseAddress = new Uri(url + "/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CletaEats API",
        Version = "v1",
        Description = "REST API del sistema CletaEats (clientes, restaurantes, repartidores, pedidos y reportes)."
    });
});
var corsCsv = builder.Configuration["CORS_ORIGINS"];
var corsOrigins = string.IsNullOrWhiteSpace(corsCsv)
    ? new[] { "http://localhost:5173", "http://127.0.0.1:5173" }
    : corsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CletaEats v1");
    c.DocumentTitle = "CletaEats · API";
    c.InjectStylesheet("/css/swagger-custom.css");
});

app.UseCors();
app.MapControllers();

// Inicializar base de datos al arranque
_ = DatabaseManager.Instance;

app.Run();
