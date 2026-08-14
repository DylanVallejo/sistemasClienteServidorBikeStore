using BikeStore.DataAccess.Repositories;
using BikeStore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

// API RESTful para administrar Clientes (CRUD + busqueda por cedula/apellido).
[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _repo;

    public ClientesController(IClienteRepository repo)
    {
        _repo = repo;
    }

    // GET /api/clientes
    [HttpGet]
    public async Task<ActionResult<List<Cliente>>> ObtenerTodos()
        => Ok(await _repo.ObtenerTodosAsync());

    // GET /api/clientes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> ObtenerPorId(int id)
    {
        var cliente = await _repo.ObtenerPorIdAsync(id);
        return cliente is null ? NotFound(new { mensaje = "Cliente no encontrado." }) : Ok(cliente);
    }

    // GET /api/clientes/buscar?cedula=&apellido=
    [HttpGet("buscar")]
    public async Task<ActionResult<List<Cliente>>> Buscar([FromQuery] string? cedula, [FromQuery] string? apellido)
        => Ok(await _repo.BuscarAsync(cedula, apellido));

    // POST /api/clientes
    [HttpPost]
    public async Task<ActionResult<Cliente>> Crear([FromBody] Cliente cliente)
    {
        var error = Validar(cliente);
        if (error is not null) return BadRequest(new { mensaje = error });

        cliente.IdCliente = await _repo.CrearAsync(cliente);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.IdCliente }, cliente);
    }

    // PUT /api/clientes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Cliente cliente)
    {
        if (id != cliente.IdCliente)
            return BadRequest(new { mensaje = "El Id de la URL no coincide con el del cuerpo." });

        var error = Validar(cliente);
        if (error is not null) return BadRequest(new { mensaje = error });

        var actualizado = await _repo.ActualizarAsync(cliente);
        return actualizado ? NoContent() : NotFound(new { mensaje = "Cliente no encontrado." });
    }

    // DELETE /api/clientes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _repo.EliminarAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = "Cliente no encontrado." });
    }

    private static string? Validar(Cliente c)
    {
        if (string.IsNullOrWhiteSpace(c.Cedula)) return "La cedula es obligatoria.";
        if (string.IsNullOrWhiteSpace(c.Nombres)) return "Los nombres son obligatorios.";
        if (string.IsNullOrWhiteSpace(c.Apellidos)) return "Los apellidos son obligatorios.";
        return null;
    }
}
