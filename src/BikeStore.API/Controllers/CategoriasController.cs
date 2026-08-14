using BikeStore.DataAccess.Repositories;
using BikeStore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.API.Controllers;

// API RESTful para administrar Categorias (CRUD completo).
[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepository _repo;

    public CategoriasController(ICategoriaRepository repo)
    {
        _repo = repo;
    }

    // GET /api/categorias
    [HttpGet]
    public async Task<ActionResult<List<Categoria>>> ObtenerTodas()
        => Ok(await _repo.ObtenerTodasAsync());

    // GET /api/categorias/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> ObtenerPorId(int id)
    {
        var categoria = await _repo.ObtenerPorIdAsync(id);
        return categoria is null ? NotFound(new { mensaje = "Categoria no encontrada." }) : Ok(categoria);
    }

    // POST /api/categorias
    [HttpPost]
    public async Task<ActionResult<Categoria>> Crear([FromBody] Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
            return BadRequest(new { mensaje = "El nombre de la categoria es obligatorio." });

        categoria.IdCategoria = await _repo.CrearAsync(categoria);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = categoria.IdCategoria }, categoria);
    }

    // PUT /api/categorias/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Categoria categoria)
    {
        if (id != categoria.IdCategoria)
            return BadRequest(new { mensaje = "El Id de la URL no coincide con el del cuerpo." });

        var actualizado = await _repo.ActualizarAsync(categoria);
        return actualizado ? NoContent() : NotFound(new { mensaje = "Categoria no encontrada." });
    }

    // DELETE /api/categorias/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _repo.EliminarAsync(id);
        return eliminado ? NoContent() : NotFound(new { mensaje = "Categoria no encontrada." });
    }
}
