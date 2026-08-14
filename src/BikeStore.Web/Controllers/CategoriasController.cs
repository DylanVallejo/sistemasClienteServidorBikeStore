using BikeStore.Domain.Entities;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

// Controlador MVC de Categorias: consume la API /api/categorias.
public class CategoriasController : Controller
{
    private readonly ApiClient _api;
    private const string Ruta = "api/categorias";

    public CategoriasController(ApiClient api)
    {
        _api = api;
    }

    // Lista de categorias.
    public async Task<IActionResult> Index()
    {
        var categorias = await _api.GetListAsync<Categoria>(Ruta);
        return View(categorias);
    }

    // Formulario de creacion.
    public IActionResult Create() => View(new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (!ModelState.IsValid) return View(categoria);

        var (ok, error) = await _api.PostAsync(Ruta, categoria);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(categoria);
        }
        return RedirectToAction(nameof(Index));
    }

    // Formulario de edicion.
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _api.GetAsync<Categoria>($"{Ruta}/{id}");
        return categoria is null ? NotFound() : View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Categoria categoria)
    {
        if (!ModelState.IsValid) return View(categoria);

        var (ok, error) = await _api.PutAsync($"{Ruta}/{categoria.IdCategoria}", categoria);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(categoria);
        }
        return RedirectToAction(nameof(Index));
    }

    // Confirmacion de eliminacion.
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _api.GetAsync<Categoria>($"{Ruta}/{id}");
        return categoria is null ? NotFound() : View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (ok, error) = await _api.DeleteAsync($"{Ruta}/{id}");
        if (!ok) TempData["Error"] = error;
        return RedirectToAction(nameof(Index));
    }
}
