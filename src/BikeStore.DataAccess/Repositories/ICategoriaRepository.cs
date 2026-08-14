using BikeStore.Domain.Entities;

namespace BikeStore.DataAccess.Repositories;

// Operaciones de acceso a datos para Categorias.
public interface ICategoriaRepository
{
    Task<List<Categoria>> ObtenerTodasAsync();
    Task<Categoria?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Categoria categoria);
    Task<bool> ActualizarAsync(Categoria categoria);
    Task<bool> EliminarAsync(int id);
}
