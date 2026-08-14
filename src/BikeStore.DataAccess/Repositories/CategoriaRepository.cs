using BikeStore.DataAccess.Data;
using BikeStore.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace BikeStore.DataAccess.Repositories;

// Implementacion ADO.NET del acceso a datos de Categorias.
// Se usan comandos parametrizados (@parametro) para evitar inyeccion SQL.
public class CategoriaRepository : ICategoriaRepository
{
    private readonly ISqlConnectionFactory _factory;

    public CategoriaRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Categoria>> ObtenerTodasAsync()
    {
        var lista = new List<Categoria>();
        const string sql = "SELECT IdCategoria, Nombre, Descripcion, Activo FROM Categoria ORDER BY Nombre";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(Mapear(reader));

        return lista;
    }

    public async Task<Categoria?> ObtenerPorIdAsync(int id)
    {
        const string sql = "SELECT IdCategoria, Nombre, Descripcion, Activo FROM Categoria WHERE IdCategoria = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<int> CrearAsync(Categoria c)
    {
        // OUTPUT INSERTED devuelve el Id generado por la BD.
        const string sql = @"INSERT INTO Categoria (Nombre, Descripcion, Activo)
                             OUTPUT INSERTED.IdCategoria
                             VALUES (@nombre, @descripcion, @activo)";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@nombre", c.Nombre);
        cmd.Parameters.AddWithValue("@descripcion", (object?)c.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@activo", c.Activo);
        await cn.OpenAsync();
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<bool> ActualizarAsync(Categoria c)
    {
        const string sql = @"UPDATE Categoria
                             SET Nombre = @nombre, Descripcion = @descripcion, Activo = @activo
                             WHERE IdCategoria = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@nombre", c.Nombre);
        cmd.Parameters.AddWithValue("@descripcion", (object?)c.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@activo", c.Activo);
        cmd.Parameters.AddWithValue("@id", c.IdCategoria);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM Categoria WHERE IdCategoria = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // Convierte una fila del lector en un objeto Categoria.
    private static Categoria Mapear(SqlDataReader r) => new()
    {
        IdCategoria = r.GetInt32(0),
        Nombre = r.GetString(1),
        Descripcion = r.IsDBNull(2) ? null : r.GetString(2),
        Activo = r.GetBoolean(3)
    };
}
