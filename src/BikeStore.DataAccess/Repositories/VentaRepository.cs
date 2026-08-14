using BikeStore.DataAccess.Data;
using BikeStore.Domain;
using BikeStore.Domain.Dtos;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Excepciones;
using Microsoft.Data.SqlClient;

namespace BikeStore.DataAccess.Repositories;

// Implementacion ADO.NET del acceso a datos de Ventas.
// El registro usa una TRANSACCION: si algo falla (ej. stock insuficiente),
// no se guarda nada y no se descuenta inventario.
public class VentaRepository : IVentaRepository
{
    private readonly ISqlConnectionFactory _factory;

    public VentaRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Venta> RegistrarAsync(VentaCreateDto dto)
    {
        // Validaciones basicas antes de tocar la BD.
        if (dto.Detalles is null || dto.Detalles.Count == 0)
            throw new NegocioException("La venta debe tener al menos un producto.");

        using var cn = _factory.CreateConnection();
        await cn.OpenAsync();
        using var tx = (SqlTransaction)await cn.BeginTransactionAsync();

        try
        {
            // 1) Insertar la cabecera con totales en 0 (se actualizan al final).
            var venta = new Venta { IdCliente = dto.IdCliente, Fecha = DateTime.Now };
            const string sqlCabecera = @"INSERT INTO Venta (Fecha, IdCliente, Subtotal, Iva, Total)
                                        OUTPUT INSERTED.IdVenta
                                        VALUES (@fecha, @idCliente, 0, 0, 0)";
            using (var cmd = new SqlCommand(sqlCabecera, cn, tx))
            {
                cmd.Parameters.AddWithValue("@fecha", venta.Fecha);
                cmd.Parameters.AddWithValue("@idCliente", venta.IdCliente);
                venta.IdVenta = (int)(await cmd.ExecuteScalarAsync())!;
            }

            decimal subtotalVenta = 0m;

            // 2) Procesar cada linea: leer precio/stock, validar, insertar detalle y descontar stock.
            foreach (var linea in dto.Detalles)
            {
                if (linea.Cantidad <= 0)
                    throw new NegocioException("La cantidad de cada producto debe ser mayor a cero.");

                // Leer precio y stock ACTUAL de la bicicleta (dentro de la transaccion).
                decimal precio;
                int stock;
                const string sqlBici = "SELECT Precio, Stock FROM Bicicleta WHERE IdBicicleta = @id";
                using (var cmd = new SqlCommand(sqlBici, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@id", linea.IdBicicleta);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                        throw new NegocioException($"La bicicleta con Id {linea.IdBicicleta} no existe.");
                    precio = reader.GetDecimal(0);
                    stock = reader.GetInt32(1);
                }

                if (stock < linea.Cantidad)
                    throw new NegocioException($"Stock insuficiente para la bicicleta Id {linea.IdBicicleta}. Disponible: {stock}, solicitado: {linea.Cantidad}.");

                decimal subtotalLinea = precio * linea.Cantidad;
                subtotalVenta += subtotalLinea;

                // Insertar el detalle.
                const string sqlDetalle = @"INSERT INTO DetalleVenta (IdVenta, IdBicicleta, Cantidad, Precio, Subtotal)
                                           VALUES (@idVenta, @idBicicleta, @cantidad, @precio, @subtotal)";
                using (var cmd = new SqlCommand(sqlDetalle, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@idVenta", venta.IdVenta);
                    cmd.Parameters.AddWithValue("@idBicicleta", linea.IdBicicleta);
                    cmd.Parameters.AddWithValue("@cantidad", linea.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.Parameters.AddWithValue("@subtotal", subtotalLinea);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Descontar stock y marcar como Agotado si llega a 0.
                const string sqlStock = @"UPDATE Bicicleta
                                         SET Stock = Stock - @cantidad,
                                             Estado = CASE WHEN Stock - @cantidad <= 0 THEN 'Agotado' ELSE 'Disponible' END
                                         WHERE IdBicicleta = @id";
                using (var cmd = new SqlCommand(sqlStock, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@cantidad", linea.Cantidad);
                    cmd.Parameters.AddWithValue("@id", linea.IdBicicleta);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // 3) Calcular IVA y total, y actualizar la cabecera.
            decimal iva = Math.Round(subtotalVenta * ReglasNegocio.PorcentajeIva, 2);
            decimal total = subtotalVenta + iva;

            const string sqlActualizar = "UPDATE Venta SET Subtotal = @subtotal, Iva = @iva, Total = @total WHERE IdVenta = @id";
            using (var cmd = new SqlCommand(sqlActualizar, cn, tx))
            {
                cmd.Parameters.AddWithValue("@subtotal", subtotalVenta);
                cmd.Parameters.AddWithValue("@iva", iva);
                cmd.Parameters.AddWithValue("@total", total);
                cmd.Parameters.AddWithValue("@id", venta.IdVenta);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            venta.Subtotal = subtotalVenta;
            venta.Iva = iva;
            venta.Total = total;
            return venta;
        }
        catch
        {
            // Cualquier error revierte TODO (no se guarda venta ni se descuenta stock).
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Venta>> ObtenerHistorialAsync()
    {
        var lista = new List<Venta>();
        const string sql = @"
            SELECT v.IdVenta, v.Fecha, v.IdCliente, v.Subtotal, v.Iva, v.Total,
                   (c.Nombres + ' ' + c.Apellidos) AS NombreCliente
            FROM Venta v
            INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
            ORDER BY v.Fecha DESC";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(MapearCabecera(reader));

        return lista;
    }

    public async Task<List<Venta>> ObtenerPorClienteAsync(int idCliente)
    {
        var lista = new List<Venta>();
        const string sql = @"
            SELECT v.IdVenta, v.Fecha, v.IdCliente, v.Subtotal, v.Iva, v.Total,
                   (c.Nombres + ' ' + c.Apellidos) AS NombreCliente
            FROM Venta v
            INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
            WHERE v.IdCliente = @idCliente
            ORDER BY v.Fecha DESC";

        using var cn = _factory.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@idCliente", idCliente);
        await cn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add(MapearCabecera(reader));

        return lista;
    }

    public async Task<Venta?> ObtenerPorIdAsync(int id)
    {
        Venta? venta = null;

        using var cn = _factory.CreateConnection();
        await cn.OpenAsync();

        // Cabecera.
        const string sqlCab = @"
            SELECT v.IdVenta, v.Fecha, v.IdCliente, v.Subtotal, v.Iva, v.Total,
                   (c.Nombres + ' ' + c.Apellidos) AS NombreCliente
            FROM Venta v
            INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
            WHERE v.IdVenta = @id";
        using (var cmd = new SqlCommand(sqlCab, cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                venta = MapearCabecera(reader);
        }

        if (venta is null) return null;

        // Detalles.
        const string sqlDet = @"
            SELECT d.IdDetalle, d.IdVenta, d.IdBicicleta, d.Cantidad, d.Precio, d.Subtotal,
                   (b.Marca + ' ' + b.Modelo) AS DescripcionBicicleta
            FROM DetalleVenta d
            INNER JOIN Bicicleta b ON d.IdBicicleta = b.IdBicicleta
            WHERE d.IdVenta = @id";
        using (var cmd = new SqlCommand(sqlDet, cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                venta.Detalles.Add(new DetalleVenta
                {
                    IdDetalle = reader.GetInt32(0),
                    IdVenta = reader.GetInt32(1),
                    IdBicicleta = reader.GetInt32(2),
                    Cantidad = reader.GetInt32(3),
                    Precio = reader.GetDecimal(4),
                    Subtotal = reader.GetDecimal(5),
                    DescripcionBicicleta = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
        }

        return venta;
    }

    private static Venta MapearCabecera(SqlDataReader r) => new()
    {
        IdVenta = r.GetInt32(0),
        Fecha = r.GetDateTime(1),
        IdCliente = r.GetInt32(2),
        Subtotal = r.GetDecimal(3),
        Iva = r.GetDecimal(4),
        Total = r.GetDecimal(5),
        NombreCliente = r.IsDBNull(6) ? null : r.GetString(6)
    };
}
