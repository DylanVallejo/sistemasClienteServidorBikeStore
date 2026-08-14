namespace BikeStore.Domain.Entities;

// Representa a un cliente de la tienda.
public class Cliente
{
    public int IdCliente { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}
