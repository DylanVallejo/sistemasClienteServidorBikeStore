using System.Text;
using BikeStore.DataAccess.Data;
using BikeStore.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace BikeStore.DataAccess.Repositories;

// Implementacion ADO.NET del acceso a datos de Clientes.
public class ClienteRepository : IClienteRepository
{
    private readonly ISqlConnectionFactory _factory;

    private const string SelectBase =
        "SELECT IdCliente, Cedula, Nombres, Apellidos, Telefono, Correo FROM Cliente";

    public ClienteRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Cliente>> ObtenerTodosAsync()
    {
        var lista = new List<Cliente>();
        string sql = SelectBase + " ORDER BY Apellidos, Nombres";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(Mapear(reader));

        return lista;
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        string sql = SelectBase + " WHERE IdCliente = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<List<Cliente>> BuscarAsync(string? cedula, string? apellido)
    {
        var lista = new List<Cliente>();
        var sql = new StringBuilder(SelectBase + " WHERE 1 = 1");

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand { Connection = cn };

        if (!string.IsNullOrWhiteSpace(cedula))
        {
            sql.Append(" AND Cedula LIKE @cedula");
            cmd.Parameters.AddWithValue("@cedula", "%" + cedula + "%");
        }
        if (!string.IsNullOrWhiteSpace(apellido))
        {
            sql.Append(" AND Apellidos LIKE @apellido");
            cmd.Parameters.AddWithValue("@apellido", "%" + apellido + "%");
        }
        sql.Append(" ORDER BY Apellidos, Nombres");
        cmd.CommandText = sql.ToString();

        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(Mapear(reader));

        return lista;
    }

    public async Task<int> CrearAsync(Cliente c)
    {
        const string sql = @"INSERT INTO Cliente (Cedula, Nombres, Apellidos, Telefono, Correo)
                             OUTPUT INSERTED.IdCliente
                             VALUES (@cedula, @nombres, @apellidos, @telefono, @correo)";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        CargarParametros(cmd, c);
        await cn.OpenAsync();
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<bool> ActualizarAsync(Cliente c)
    {
        const string sql = @"UPDATE Cliente
                             SET Cedula = @cedula, Nombres = @nombres, Apellidos = @apellidos,
                                 Telefono = @telefono, Correo = @correo
                             WHERE IdCliente = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        CargarParametros(cmd, c);
        cmd.Parameters.AddWithValue("@id", c.IdCliente);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM Cliente WHERE IdCliente = @id";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static void CargarParametros(SqlCommand cmd, Cliente c)
    {
        cmd.Parameters.AddWithValue("@cedula", c.Cedula);
        cmd.Parameters.AddWithValue("@nombres", c.Nombres);
        cmd.Parameters.AddWithValue("@apellidos", c.Apellidos);
        cmd.Parameters.AddWithValue("@telefono", (object?)c.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@correo", (object?)c.Correo ?? DBNull.Value);
    }

    private static Cliente Mapear(SqlDataReader r) => new()
    {
        IdCliente = r.GetInt32(0),
        Cedula = r.GetString(1),
        Nombres = r.GetString(2),
        Apellidos = r.GetString(3),
        Telefono = r.IsDBNull(4) ? null : r.GetString(4),
        Correo = r.IsDBNull(5) ? null : r.GetString(5)
    };
}
