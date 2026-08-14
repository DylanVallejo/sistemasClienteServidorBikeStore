using BikeStore.DataAccess.Repositories;
using BikeStore.Domain;
using BikeStore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

// API RESTful para administrar Bicicletas (CRUD + busquedas + stock bajo).
[ApiController]
[Route("api/bicicletas")]
public class BicicletasController : ControllerBase
{
    private readonly IBicicletaRepository _repo;

    public BicicletasController(IBicicletaRepository repo)
    {
        _repo = repo;
    }

    // GET /api/bicicletas
    [HttpGet]
    public async Task<ActionResult<List<Bicicleta>>> ObtenerTodas()
        => Ok(await _repo.ObtenerTodasAsync());

    // GET /api/bicicletas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Bicicleta>> ObtenerPorId(int id)
    {
        var bicicleta = await _repo.ObtenerPorIdAsync(id);
        return bicicleta is null ? NotFound(new { mensaje = "Bicicleta no encontrada." }) : Ok(bicicleta);
    }

    // GET /api/bicicletas/buscar?nombre=&idCategoria=&marca=
    [HttpGet("buscar")]
    public async Task<ActionResult<List<Bicicleta>>> Buscar(
        [FromQuery] string? nombre, [FromQuery] int? idCategoria, [FromQuery] string? marca)
        => Ok(await _repo.BuscarAsync(nombre, idCategoria, marca));

    // GET /api/bicicletas/stock-bajo?umbral=5
    [HttpGet("stock-bajo")]
    public async Task<ActionResult<List<Bicicleta>>> StockBajo([FromQuery] int? umbral)
        => Ok(await _repo.ObtenerStockBajoAsync(umbral ?? ReglasNegocio.UmbralStockBajo));

    // POST /api/bicicletas
    [HttpPost]
    public async Task<ActionResult<Bicicleta>> Crear([FromBody] Bicicleta bicicleta)
    {
        var error = Validar(bicicleta);
        if (error is not null) return BadRequest(new { mensaje = error });

        bicicleta.IdBicicleta = await _repo.CrearAsync(bicicleta);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = bicicleta.IdBicicleta }, bicicleta);
    }

    // PUT /api/bicicletas/5  (permite actualizar precio, stock y demas datos)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Bicicleta bicicleta)
    {
        if (id != bicicleta.IdBicicleta)
            return BadRequest(new { mensaje = "El Id de la URL no coincide con el del cuerpo." });

        var error = Validar(bicicleta);
        if (error is not null) return BadRequest(new { mensaje = error });

        var actualizado = await _repo.ActualizarAsync(bicicleta);
        return actualizado ? NoContent() : NotFound(new { mensaje = "Bicicleta no encontrada." });
    }

    // DELETE /api/bicicletas/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _repo.EliminarAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = "Bicicleta no encontrada." });
    }

    // Validaciones simples reutilizadas por POST y PUT.
    private static string? Validar(Bicicleta b)
    {
        if (b.IdCategoria <= 0) return "Debe seleccionar una categoria valida.";
        if (string.IsNullOrWhiteSpace(b.Marca)) return "La marca es obligatoria.";
        if (string.IsNullOrWhiteSpace(b.Modelo)) return "El modelo es obligatorio.";
        if (b.Precio < 0) return "El precio no puede ser negativo.";
        if (b.Stock < 0) return "El stock no puede ser negativo.";
        return null;
    }
}
