using System.Net.Http.Json;

namespace BikeStore.Web.Services;

// Envoltorio simple sobre HttpClient para consumir la API REST.
// Centraliza las llamadas GET/POST/PUT/DELETE y la (de)serializacion JSON.
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // GET que devuelve una lista (si falla, devuelve lista vacia).
    public async Task<List<T>> GetListAsync<T>(string url)
        => await _http.GetFromJsonAsync<List<T>>(url) ?? new List<T>();

    // GET que devuelve un objeto (null si no existe / 404).
    public async Task<T?> GetAsync<T>(string url)
    {
        var respuesta = await _http.GetAsync(url);
        if (!respuesta.IsSuccessStatusCode) return default;
        return await respuesta.Content.ReadFromJsonAsync<T>();
    }

    // POST. Devuelve (exito, mensajeError). Si exito, mensajeError es null.
    public async Task<(bool ok, string? error)> PostAsync<T>(string url, T datos)
    {
        var respuesta = await _http.PostAsJsonAsync(url, datos);
        return await LeerResultadoAsync(respuesta);
    }

    // PUT. Devuelve (exito, mensajeError).
    public async Task<(bool ok, string? error)> PutAsync<T>(string url, T datos)
    {
        var respuesta = await _http.PutAsJsonAsync(url, datos);
        return await LeerResultadoAsync(respuesta);
    }

    // DELETE. Devuelve (exito, mensajeError).
    public async Task<(bool ok, string? error)> DeleteAsync(string url)
    {
        var respuesta = await _http.DeleteAsync(url);
        return await LeerResultadoAsync(respuesta);
    }

    // Lee el resultado de una respuesta y extrae el mensaje de error de la API si lo hay.
    private static async Task<(bool ok, string? error)> LeerResultadoAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return (true, null);

        try
        {
            var error = await respuesta.Content.ReadFromJsonAsync<ErrorApi>();
            return (false, error?.Mensaje ?? "Ocurrio un error al procesar la solicitud.");
        }
        catch
        {
            return (false, "Ocurrio un error al procesar la solicitud.");
        }
    }

    // Estructura del mensaje de error que devuelve la API ({ "mensaje": "..." }).
    private class ErrorApi
    {
        public string? Mensaje { get; set; }
    }
}
