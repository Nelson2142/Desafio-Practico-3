using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Desafio03.API.Controllers;

/// <summary>CRUD de productos.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductoServicio _servicio;

    public ProductosController(IProductoServicio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Lista todos los productos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductoDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    /// <summary>Obtiene un producto por su Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id)
    {
        return Ok(await _servicio.ObtenerPorIdAsync(id));
    }

    /// <summary>Crea un producto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoDto>> Crear([FromBody] ProductoGuardarDto datos)
    {
        var creado = await _servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza un producto.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, [FromBody] ProductoGuardarDto datos)
    {
        return Ok(await _servicio.ActualizarAsync(id, datos));
    }

    /// <summary>Elimina un producto (solo si no aparece en pedidos).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
