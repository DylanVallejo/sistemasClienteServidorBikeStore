using System.Text;
using BikeStore.DataAccess.Data;
using BikeStore.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace BikeStore.DataAccess.Repositories;

// Implementacion ADO.NET del acceso a datos de Bicicletas.
public class BicicletaRepository : IBicicletaRepository
{
    private readonly ISqlConnectionFactory _factory;

    // Consulta base con JOIN para traer tambien el nombre de la categoria.
    private const string SelectBase = @"
        SELECT b.IdBicicleta, b.IdCategoria, b.Marca, b.Modelo, b.Precio, b.Stock, b.Estado, c.Nombre AS NombreCategoria
        FROM Bicicleta b
        INNER JOIN Categoria c ON b.IdCategoria = c.IdCategoria";

    public BicicletaRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Bicicleta>> ObtenerTodasAsync()
    {
        var lista = new List<Bicicleta>();
        string sql = SelectBase + " ORDER BY b.IdBicicleta";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(Mapear(reader));

        return lista;
    }

    public async Task<Bicicleta?> ObtenerPorIdAsync(int id)
    {
        string sql = SelectBase + " WHERE b.IdBicicleta = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    // Busqueda dinamica: solo agrega filtros por los criterios recibidos.
    public async Task<List<Bicicleta>> BuscarAsync(string? nombre, int? idCategoria, string? marca)
    {
        var lista = new List<Bicicleta>();
        var sql = new StringBuilder(SelectBase + " WHERE 1 = 1");

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand { Connection = cn };

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            sql.Append(" AND b.Modelo LIKE @nombre");
            cmd.Parameters.AddWithValue("@nombre", "%" + nombre + "%");
        }
        if (idCategoria.HasValue)
        {
            sql.Append(" AND b.IdCategoria = @idCategoria");
            cmd.Parameters.AddWithValue("@idCategoria", idCategoria.Value);
        }
        if (!string.IsNullOrWhiteSpace(marca))
        {
            sql.Append(" AND b.Marca LIKE @marca");
            cmd.Parameters.AddWithValue("@marca", "%" + marca + "%");
        }
        sql.Append(" ORDER BY b.IdBicicleta");
        cmd.CommandText = sql.ToString();

        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(Mapear(reader));

        return lista;
    }

    // Devuelve bicicletas cuyo stock es menor o igual al umbral (por defecto sirve para "stock bajo" y "agotado").
    public async Task<List<Bicicleta>> ObtenerStockBajoAsync(int umbral)
    {
        var lista = new List<Bicicleta>();
        string sql = SelectBase + " WHERE b.Stock <= @umbral ORDER BY b.Stock";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@umbral", umbral);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(Mapear(reader));

        return lista;
    }

    public async Task<int> CrearAsync(Bicicleta b)
    {
        const string sql = @"INSERT INTO Bicicleta (IdCategoria, Marca, Modelo, Precio, Stock, Estado)
                             OUTPUT INSERTED.IdBicicleta
                             VALUES (@idCategoria, @marca, @modelo, @precio, @stock, @estado)";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        CargarParametros(cmd, b);
        await cn.OpenAsync();
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<bool> ActualizarAsync(Bicicleta b)
    {
        const string sql = @"UPDATE Bicicleta
                             SET IdCategoria = @idCategoria, Marca = @marca, Modelo = @modelo,
                                 Precio = @precio, Stock = @stock, Estado = @estado
                             WHERE IdBicicleta = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        CargarParametros(cmd, b);
        cmd.Parameters.AddWithValue("@id", b.IdBicicleta);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM Bicicleta WHERE IdBicicleta = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static void CargarParametros(SqlCommand cmd, Bicicleta b)
    {
        cmd.Parameters.AddWithValue("@idCategoria", b.IdCategoria);
        cmd.Parameters.AddWithValue("@marca", b.Marca);
        cmd.Parameters.AddWithValue("@modelo", b.Modelo);
        cmd.Parameters.AddWithValue("@precio", b.Precio);
        cmd.Parameters.AddWithValue("@stock", b.Stock);
        cmd.Parameters.AddWithValue("@estado", b.Estado);
    }

    private static Bicicleta Mapear(SqlDataReader r) => new()
    {
        IdBicicleta = r.GetInt32(0),
        IdCategoria = r.GetInt32(1),
        Marca = r.GetString(2),
        Modelo = r.GetString(3),
        Precio = r.GetDecimal(4),
        Stock = r.GetInt32(5),
        Estado = r.GetString(6),
        NombreCategoria = r.IsDBNull(7) ? null : r.GetString(7)
    };
}
