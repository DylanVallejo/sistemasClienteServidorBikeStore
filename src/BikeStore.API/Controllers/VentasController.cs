using BikeStore.DataAccess.Repositories;
using BikeStore.Domain.Dtos;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

// API RESTful para Ventas: registrar (POST) y consultar (GET).
[ApiController]
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly IVentaRepository _repo;

    public VentasController(IVentaRepository repo)
    {
        _repo = repo;
    }

    // GET /api/ventas  -> historial de ventas.
    [HttpGet]
    public async Task<ActionResult<List<Venta>>> ObtenerHistorial()
        => Ok(await _repo.ObtenerHistorialAsync());

    // GET /api/ventas/5  -> venta con su detalle.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Venta>> ObtenerPorId(int id)
    {
        var venta = await _repo.ObtenerPorIdAsync(id);
        return venta is null ? NotFound(new { mensaje = "Venta no encontrada." }) : Ok(venta);
    }

    // GET /api/ventas/cliente/5  -> ventas de un cliente.
    [HttpGet("cliente/{idCliente:int}")]
    public async Task<ActionResult<List<Venta>>> ObtenerPorCliente(int idCliente)
        => Ok(await _repo.ObtenerPorClienteAsync(idCliente));

    // POST /api/ventas  -> registra la venta, calcula IVA/total y descuenta stock.
    [HttpPost]
    public async Task<ActionResult<Venta>> Registrar([FromBody] VentaCreateDto dto)
    {
        try
        {
            var venta = await _repo.RegistrarAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.IdVenta }, venta);
        }
        catch (NegocioException ex)
        {
            // Errores esperados (stock insuficiente, datos invalidos) -> 400 con mensaje claro.
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
