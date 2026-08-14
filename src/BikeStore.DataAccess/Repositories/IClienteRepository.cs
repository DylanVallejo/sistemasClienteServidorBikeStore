using BikeStore.Domain.Entities;

namespace BikeStore.DataAccess.Repositories;

// Operaciones de acceso a datos para Clientes.
public interface IClienteRepository
{
    Task<List<Cliente>> ObtenerTodosAsync();
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Cliente cliente);
    Task<bool> ActualizarAsync(Cliente cliente);
    Task<bool> EliminarAsync(int id);

    // Busqueda por cedula o apellido.
    Task<List<Cliente>> BuscarAsync(string? cedula, string? apellido);
}
