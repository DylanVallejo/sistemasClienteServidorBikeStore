namespace BikeStore.Domain.Entities;

// Representa una linea (producto) dentro de una venta.
public class DetalleVenta
{
    public int IdDetalle { get; set; }
    public int IdVenta { get; set; }
    public int IdBicicleta { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Subtotal { get; set; } // Cantidad * Precio

    // Campo de apoyo (viene de JOIN con Bicicleta).
    public string? DescripcionBicicleta { get; set; }
}
