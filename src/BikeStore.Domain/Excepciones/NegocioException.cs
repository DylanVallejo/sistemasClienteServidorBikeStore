namespace BikeStore.Domain.Excepciones;

// Excepcion para errores de reglas de negocio (ej: stock insuficiente,
// bicicleta inexistente). La API la traduce a un error 400 con mensaje claro.
public class NegocioException : Exception
{
    public NegocioException(string mensaje) : base(mensaje) { }
}
