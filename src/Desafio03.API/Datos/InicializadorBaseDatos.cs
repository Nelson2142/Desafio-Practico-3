using Desafio03.AccesoDatos.Contexto;
using Desafio03.Entidades.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Desafio03.API.Datos;

/// <summary>
/// Aplica las migraciones al arrancar y carga datos de ejemplo si la base está vacía.
/// Reintenta varias veces porque, con Docker, SQL Server tarda unos segundos en estar listo.
/// </summary>
public static class InicializadorBaseDatos
{
    public static async Task InicializarAsync(IServiceProvider servicios, IConfiguration configuracion, ILogger logger)
    {
        var intentos = configuracion.GetValue("BaseDatos:IntentosConexion", 10);
        var espera = TimeSpan.FromSeconds(configuracion.GetValue("BaseDatos:SegundosEntreIntentos", 5));

        for (var intento = 1; intento <= intentos; intento++)
        {
            try
            {
                using var alcance = servicios.CreateScope();
                var contexto = alcance.ServiceProvider.GetRequiredService<PedidosDbContext>();

                await contexto.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas correctamente.");

                if (configuracion.GetValue("BaseDatos:CargarDatosDeEjemplo", true))
                {
                    await SembrarAsync(contexto, logger);
                }

                return;
            }
            catch (Exception ex) when (intento < intentos)
            {
                logger.LogWarning("La base de datos aún no responde (intento {Intento}/{Total}): {Mensaje}",
                    intento, intentos, ex.Message);
                await Task.Delay(espera);
            }
        }
    }

    private static async Task SembrarAsync(PedidosDbContext contexto, ILogger logger)
    {
        if (await contexto.Clientes.AnyAsync() || await contexto.Productos.AnyAsync())
        {
            return;
        }

        var clientes = new[]
        {
            new Cliente { Nombre = "Ana", Apellido = "Martínez", Email = "ana.martinez@correo.com" },
            new Cliente { Nombre = "Carlos", Apellido = "Hernández", Email = "carlos.hernandez@correo.com" },
            new Cliente { Nombre = "Sofía", Apellido = "López", Email = "sofia.lopez@correo.com" }
        };

        var productos = new[]
        {
            new Producto { Nombre = "Laptop Lenovo IdeaPad 3", Descripcion = "Laptop 15.6 pulgadas, 16 GB RAM, 512 GB SSD", Precio = 649.99m, Stock = 15 },
            new Producto { Nombre = "Mouse inalámbrico", Descripcion = "Mouse óptico inalámbrico 2.4 GHz", Precio = 18.50m, Stock = 80 },
            new Producto { Nombre = "Teclado mecánico", Descripcion = "Teclado mecánico retroiluminado en español", Precio = 54.75m, Stock = 30 },
            new Producto { Nombre = "Monitor 24 pulgadas", Descripcion = "Monitor Full HD IPS de 24 pulgadas", Precio = 139.00m, Stock = 10 }
        };

        contexto.Clientes.AddRange(clientes);
        contexto.Productos.AddRange(productos);

        // Pedidos de ejemplo (el stock ya refleja las unidades vendidas).
        AgregarPedido(contexto, clientes[0], productos[0], 1, DateTime.Now.AddDays(-3));
        AgregarPedido(contexto, clientes[1], productos[1], 3, DateTime.Now.AddDays(-1));

        await contexto.SaveChangesAsync();
        logger.LogInformation("Datos de ejemplo cargados: {Clientes} clientes y {Productos} productos.",
            clientes.Length, productos.Length);
    }

    private static void AgregarPedido(PedidosDbContext contexto, Cliente cliente, Producto producto, int cantidad, DateTime fecha)
    {
        producto.Stock -= cantidad;
        contexto.Pedidos.Add(new Pedido
        {
            Cliente = cliente,
            Producto = producto,
            Cantidad = cantidad,
            FechaPedido = fecha,
            PrecioUnitario = producto.Precio,
            Total = producto.Precio * cantidad
        });
    }
}
