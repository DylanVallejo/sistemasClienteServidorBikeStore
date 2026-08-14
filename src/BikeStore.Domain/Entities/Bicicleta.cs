namespace BikeStore.Domain.Entities;

// Representa una bicicleta del inventario.
public class Bicicleta
{
    public int IdBicicleta { get; set; }
    public int IdCategoria { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Estado { get; set; } = "Disponible"; // Disponible / Agotado

    // Campo de apoyo para mostrar el nombre de la categoria (viene de un JOIN, no es columna propia).
    public string? NombreCategoria { get; set; }
}
