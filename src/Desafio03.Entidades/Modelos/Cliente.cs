using System.ComponentModel.DataAnnotations;

namespace Desafio03.Entidades.Modelos;

/// <summary>
/// Cliente que realiza pedidos.
/// </summary>
public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(Patrones.SoloLetras, ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(Patrones.SoloLetras, ErrorMessage = "El apellido solo puede contener letras y espacios.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [RegularExpression(Patrones.Email, ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Pedidos realizados por el cliente (relación 1 a N).</summary>
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
