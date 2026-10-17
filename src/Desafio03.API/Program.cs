using System.Reflection;
using Desafio03.AccesoDatos.Contexto;
using Desafio03.AccesoDatos.Repositorios;
using Desafio03.API.Datos;
using Desafio03.API.Middleware;
using Desafio03.Entidades.Interfaces;
using Desafio03.Negocio.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Capa de Acceso a Datos: EF Core + SQL Server
// ---------------------------------------------------------------------------
var cadenaConexion = builder.Configuration.GetConnectionString("ConexionSql")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConexionSql' en appsettings.json.");

builder.Services.AddDbContext<PedidosDbContext>(opciones =>
    opciones.UseSqlServer(cadenaConexion, sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));

builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

// ---------------------------------------------------------------------------
// Capa de Negocio
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IProductoServicio, ProductoServicio>();
builder.Services.AddScoped<IPedidoServicio, PedidoServicio>();

// ---------------------------------------------------------------------------
// Capa de Presentación: API REST + Swagger
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Pedidos - Desafío 3",
        Version = "v1",
        Description = "DES104 - Desarrollo de Software Empresarial. " +
                      "API REST con arquitectura de N capas para gestionar clientes, productos y pedidos. " +
                      "Autor: Nelson Steven Rivas Tobar (RT221663)."
    });

    var archivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(archivoXml))
    {
        opciones.IncludeXmlComments(archivoXml);
    }
});

var app = builder.Build();

// Migraciones + datos de ejemplo
await InicializadorBaseDatos.InicializarAsync(app.Services, app.Configuration, app.Logger);

app.UseMiddleware<ManejoErroresMiddleware>();

// Swagger habilitado siempre para poder probar la API también dentro de Docker.
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Pedidos v1");
    opciones.DocumentTitle = "API de Pedidos - RT221663";
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { estado = "activo", fecha = DateTime.Now }))
   .WithTags("Salud");

app.MapControllers();

app.Run();
