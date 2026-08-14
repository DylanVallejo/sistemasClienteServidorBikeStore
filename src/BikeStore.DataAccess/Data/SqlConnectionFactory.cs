using Microsoft.Data.SqlClient;

namespace BikeStore.DataAccess.Data;

// Contrato para obtener conexiones a SQL Server.
public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}

// Crea conexiones ADO.NET usando la cadena de conexion configurada en appsettings.json.
// La cadena se inyecta en el constructor desde la capa API.
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Devuelve una conexion nueva (cada repositorio la abre y la cierra con "using").
    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}
