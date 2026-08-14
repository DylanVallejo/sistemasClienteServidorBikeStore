namespace BikeStore.Domain.Entities;

// Representa la cabecera de una venta.
public class Venta
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public int IdCliente { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    // Campos de apoyo (vienen de JOIN con Cliente).
    public string? NombreCliente { get; set; }

    // Lineas de la venta.
    public List<DetalleVenta> Detalles { get; set; } = new();
}
