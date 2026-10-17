using Desafio03.Entidades.Interfaces;
using Desafio03.Entidades.Modelos;
using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Excepciones;
using Desafio03.Negocio.Validacion;

namespace Desafio03.Negocio.Servicios;

public interface IProductoServicio
{
    Task<List<ProductoDto>> ObtenerTodosAsync();
    Task<ProductoDto> ObtenerPorIdAsync(int id);
    Task<ProductoDto> CrearAsync(ProductoGuardarDto datos);
    Task<ProductoDto> ActualizarAsync(int id, ProductoGuardarDto datos);
    Task EliminarAsync(int id);
}

/// <summary>
/// Lógica de negocio de productos.
/// Reglas: precio mayor que cero, stock no negativo
/// y no se elimina un producto que tenga pedidos.
/// </summary>
public class ProductoServicio : IProductoServicio
{
    private readonly IUnidadDeTrabajo _uow;

    public ProductoServicio(IUnidadDeTrabajo unidadDeTrabajo)
    {
        _uow = unidadDeTrabajo;
    }

    public async Task<List<ProductoDto>> ObtenerTodosAsync()
    {
        var productos = await _uow.Productos.ObtenerTodosAsync();
        return productos.Select(ProductoDto.DesdeEntidad).ToList();
    }

    public async Task<ProductoDto> ObtenerPorIdAsync(int id)
    {
        var producto = await BuscarAsync(id);
        return ProductoDto.DesdeEntidad(producto);
    }

    public async Task<ProductoDto> CrearAsync(ProductoGuardarDto datos)
    {
        Normalizar(datos);
        Validador.Validar(datos);

        var producto = new Producto
        {
            Nombre = datos.Nombre,
            Descripcion = datos.Descripcion,
            Precio = decimal.Round(datos.Precio, 2),
            Stock = datos.Stock
        };

        Validador.Validar(producto);
        _uow.Productos.Agregar(producto);
        await _uow.GuardarCambiosAsync();

        return ProductoDto.DesdeEntidad(producto);
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ProductoGuardarDto datos)
    {
        Normalizar(datos);
        Validador.Validar(datos);

        var producto = await BuscarAsync(id);
        producto.Nombre = datos.Nombre;
        producto.Descripcion = datos.Descripcion;
        producto.Precio = decimal.Round(datos.Precio, 2);
        producto.Stock = datos.Stock;

        Validador.Validar(producto);
        _uow.Productos.Actualizar(producto);
        await _uow.GuardarCambiosAsync();

        return ProductoDto.DesdeEntidad(producto);
    }

    public async Task EliminarAsync(int id)
    {
        var producto = await BuscarAsync(id);

        if (await _uow.Productos.TienePedidosAsync(id))
        {
            throw new ReglaNegocioException(
                "No se puede eliminar el producto porque aparece en pedidos registrados.");
        }

        _uow.Productos.Eliminar(producto);
        await _uow.GuardarCambiosAsync();
    }

    private async Task<Producto> BuscarAsync(int id)
    {
        return await _uow.Productos.ObtenerPorIdAsync(id)
               ?? throw new RecursoNoEncontradoException("producto", id);
    }

    private static void Normalizar(ProductoGuardarDto datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        datos.Nombre = datos.Nombre?.Trim() ?? string.Empty;
        datos.Descripcion = datos.Descripcion?.Trim() ?? string.Empty;
    }
}
