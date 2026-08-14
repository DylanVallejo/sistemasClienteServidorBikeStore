namespace BikeStore.Domain;

// Constantes de reglas de negocio en un solo lugar (faciles de cambiar).
public static class ReglasNegocio
{
    // Porcentaje de IVA aplicado a las ventas (Ecuador = 15%).
    // Si el docente pide 12%, cambiar unicamente este valor.
    public const decimal PorcentajeIva = 0.15m;

    // Umbral por defecto para considerar "stock bajo".
    public const int UmbralStockBajo = 5;
}
