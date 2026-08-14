namespace BikeStore.Domain.Entities;

// Representa una categoria de bicicletas (Montana, Ruta, BMX, Electricas, Infantiles).
public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
