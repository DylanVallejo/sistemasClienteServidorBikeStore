using BikeStore.Domain.Dtos;
using BikeStore.Domain.Entities;

namespace BikeStore.DataAccess.Repositories;

// Operaciones de acceso a datos para Ventas.
public interface IVentaRepository
{
    // Registra una venta completa (cabecera + detalles), calcula totales
    // y descuenta el stock. Devuelve la venta creada con su Id.
    Task<Venta> RegistrarAsync(VentaCreateDto dto);

    // Historial de todas las ventas (solo cabeceras).
    Task<List<Venta>> ObtenerHistorialAsync();

    // Una venta con su detalle completo.
    Task<Venta?> ObtenerPorIdAsync(int id);

    // Ventas realizadas por un cliente.
    Task<List<Venta>> ObtenerPorClienteAsync(int idCliente);
}
