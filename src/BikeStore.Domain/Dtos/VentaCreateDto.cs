namespace BikeStore.Domain.Dtos;

// Datos que envia el cliente (Web / Swagger) para registrar una venta.
// El precio y los totales NO se envian: el servidor los calcula desde la BD
// para evitar manipulacion de precios.
public class VentaCreateDto
{
    public int IdCliente { get; set; }
    public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
}

public class DetalleVentaCreateDto
{
    public int IdBicicleta { get; set; }
    public int Cantidad { get; set; }
}
