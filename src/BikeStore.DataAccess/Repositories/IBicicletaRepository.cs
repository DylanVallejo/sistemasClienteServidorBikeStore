using BikeStore.Domain.Entities;

namespace BikeStore.DataAccess.Repositories;

// Operaciones de acceso a datos para Bicicletas.
public interface IBicicletaRepository
{
    Task<List<Bicicleta>> ObtenerTodasAsync();
    Task<Bicicleta?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Bicicleta bicicleta);
    Task<bool> ActualizarAsync(Bicicleta bicicleta);
    Task<bool> EliminarAsync(int id);

    // Busquedas y consultas especiales exigidas por el proyecto.
    Task<List<Bicicleta>> BuscarAsync(string? nombre, int? idCategoria, string? marca);
    Task<List<Bicicleta>> ObtenerStockBajoAsync(int umbral);
}
