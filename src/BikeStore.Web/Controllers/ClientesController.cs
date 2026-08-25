using BikeStore.Domain.Entities;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers;

// Controlador MVC de Clientes: consume la API /api/clientes.
public class ClientesController : Controller
{
    private readonly ApiClient _api;
    private const string Ruta = "api/clientes";

    public ClientesController(ApiClient api)
    {
        _api = api;
    }

    // Lista con busqueda opcional por cedula o apellido.
    public async Task<IActionResult> Index(string? cedula, string? apellido)
    {
        List<Cliente> clientes;
        if (!string.IsNullOrWhiteSpace(cedula) || !string.IsNullOrWhiteSpace(apellido))
            clientes = await _api.GetListAsync<Cliente>($"{Ruta}/buscar?cedula={cedula}&apellido={apellido}");
        else
            clientes = await _api.GetListAsync<Cliente>(Ruta);

        ViewBag.Cedula = cedula;
        ViewBag.Apellido = apellido;
        return View(clientes);
    }

    public IActionResult Create() => View(new Cliente());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);

        var (ok, error) = await _api.PostAsync(Ruta, cliente);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(cliente);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _api.GetAsync<Cliente>($"{Ruta}/{id}");
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);

        var (ok, error) = await _api.PutAsync($"{Ruta}/{cliente.IdCliente}", cliente);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(cliente);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _api.GetAsync<Cliente>($"{Ruta}/{id}");
        return cliente is null ? NotFound() : View(cliente);
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
