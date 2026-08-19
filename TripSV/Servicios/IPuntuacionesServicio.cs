namespace TripSV.Servicios
{
    public interface IPuntuacionesServicio
    {
        Task<Resultado> CalificarAsync(int sitioId, string nombreUsuario, string? usuarioId, int valor);

        Task<int?> ObtenerDeUsuarioAsync(int sitioId, string nombreUsuario);

        Task<decimal> RecalcularPromedioAsync(int sitioId);
    }
}
