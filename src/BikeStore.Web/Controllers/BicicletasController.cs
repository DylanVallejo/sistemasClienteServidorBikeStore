using BikeStore.Domain.Entities;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeStore.Web.Controllers;

// Controlador MVC de Bicicletas: consume la API /api/bicicletas.
public class BicicletasController : Controller
{
    private readonly ApiClient _api;
    private const string Ruta = "api/bicicletas";

    public BicicletasController(ApiClient api)
    {
        _api = api;
    }

    // Lista con busqueda opcional por nombre/modelo, categoria y marca.
    public async Task<IActionResult> Index(string? nombre, int? idCategoria, string? marca)
    {
        List<Bicicleta> bicicletas;
        if (!string.IsNullOrWhiteSpace(nombre) || idCategoria.HasValue || !string.IsNullOrWhiteSpace(marca))
            bicicletas = await _api.GetListAsync<Bicicleta>($"{Ruta}/buscar?nombre={nombre}&idCategoria={idCategoria}&marca={marca}");
        else
            bicicletas = await _api.GetListAsync<Bicicleta>(Ruta);

        await CargarCategoriasAsync(idCategoria);
        ViewBag.Nombre = nombre;
        ViewBag.Marca = marca;
        return View(bicicletas);
    }

    // Consulta de bicicletas con stock bajo.
    public async Task<IActionResult> StockBajo(int umbral = 5)
    {
        var bicicletas = await _api.GetListAsync<Bicicleta>($"{Ruta}/stock-bajo?umbral={umbral}");
        ViewBag.Umbral = umbral;
        return View(bicicletas);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Bicicleta());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bicicleta bicicleta)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(bicicleta.IdCategoria);
            return View(bicicleta);
        }

        var (ok, error) = await _api.PostAsync(Ruta, bicicleta);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await CargarCategoriasAsync(bicicleta.IdCategoria);
            return View(bicicleta);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var bicicleta = await _api.GetAsync<Bicicleta>($"{Ruta}/{id}");
        if (bicicleta is null) return NotFound();

        await CargarCategoriasAsync(bicicleta.IdCategoria);
        return View(bicicleta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Bicicleta bicicleta)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(bicicleta.IdCategoria);
            return View(bicicleta);
        }

        var (ok, error) = await _api.PutAsync($"{Ruta}/{bicicleta.IdBicicleta}", bicicleta);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await CargarCategoriasAsync(bicicleta.IdCategoria);
            return View(bicicleta);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var bicicleta = await _api.GetAsync<Bicicleta>($"{Ruta}/{id}");
        return bicicleta is null ? NotFound() : View(bicicleta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (ok, error) = await _api.DeleteAsync($"{Ruta}/{id}");
        if (!ok) TempData["Error"] = error;
        return RedirectToAction(nameof(Index));
    }

    // Carga las categorias en un desplegable (ViewBag) para los formularios y filtros.
    private async Task CargarCategoriasAsync(int? seleccionada = null)
    {
        var categorias = await _api.GetListAsync<Categoria>("api/categorias");
        ViewBag.Categorias = new SelectList(categorias, "IdCategoria", "Nombre", seleccionada);
    }
}
