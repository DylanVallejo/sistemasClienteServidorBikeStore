using BikeStore.Domain.Dtos;
using BikeStore.Domain.Entities;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeStore.Web.Controllers;

// Controlador MVC de Ventas: consume la API /api/ventas.
public class VentasController : Controller
{
    private readonly ApiClient _api;
    private const string Ruta = "api/ventas";

    public VentasController(ApiClient api)
    {
        _api = api;
    }

    // Historial de ventas.
    public async Task<IActionResult> Index()
    {
        var ventas = await _api.GetListAsync<Venta>(Ruta);
        return View(ventas);
    }

    // Detalle de una venta.
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _api.GetAsync<Venta>($"{Ruta}/{id}");
        return venta is null ? NotFound() : View(venta);
    }

    // Ventas de un cliente.
    public async Task<IActionResult> PorCliente(int idCliente)
    {
        var ventas = await _api.GetListAsync<Venta>($"{Ruta}/cliente/{idCliente}");
        var cliente = await _api.GetAsync<Cliente>($"api/clientes/{idCliente}");
        ViewBag.NombreCliente = cliente is null ? "" : $"{cliente.Nombres} {cliente.Apellidos}";
        return View(ventas);
    }

    // Formulario para registrar una venta.
    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();
        return View(new VentaCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VentaCreateDto venta)
    {
        // Quitar lineas vacias (por si el formulario envia filas sin bicicleta).
        venta.Detalles = venta.Detalles
            .Where(d => d.IdBicicleta > 0 && d.Cantidad > 0)
            .ToList();

        if (venta.IdCliente <= 0)
            ModelState.AddModelError(string.Empty, "Debe seleccionar un cliente.");
        if (venta.Detalles.Count == 0)
            ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");

        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(venta);
        }

        var (ok, error) = await _api.PostAsync(Ruta, venta);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await CargarListasAsync();
            return View(venta);
        }
        return RedirectToAction(nameof(Index));
    }

    // Carga clientes y bicicletas para los desplegables del formulario.
    private async Task CargarListasAsync()
    {
        var clientes = await _api.GetListAsync<Cliente>("api/clientes");
        var bicicletas = await _api.GetListAsync<Bicicleta>("api/bicicletas");

        ViewBag.Clientes = new SelectList(
            clientes.Select(c => new { c.IdCliente, Texto = $"{c.Cedula} - {c.Nombres} {c.Apellidos}" }),
            "IdCliente", "Texto");

        // Se envia la lista de bicicletas (con precio y stock) para el desplegable dinamico.
        ViewBag.Bicicletas = bicicletas;
    }
}
