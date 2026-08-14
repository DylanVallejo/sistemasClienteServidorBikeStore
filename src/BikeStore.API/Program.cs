using BikeStore.DataAccess.Data;
using BikeStore.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---- Servicios ----

// Controladores de la API + configuracion de JSON (ignora ciclos por si acaso).
builder.Services.AddControllers();

// Swagger / OpenAPI para documentar y probar la API desde el navegador.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cadena de conexion a SQL Server (definida en appsettings.json).
var connectionString = builder.Configuration.GetConnectionString("BikeStoreDB")
    ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'BikeStoreDB'.");

// La fabrica de conexiones se registra como singleton (solo guarda la cadena).
builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

// Repositorios (capa de acceso a datos) -> se inyectan en los controladores.
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IBicicletaRepository, BicicletaRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();

// CORS: permite que el sitio Web (u otros clientes) consuman la API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ---- Pipeline ----

// Swagger habilitado siempre para facilitar las pruebas del proyecto.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("PermitirTodo");
app.MapControllers();

app.Run();
