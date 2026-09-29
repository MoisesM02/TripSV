using TripSV.Modelos;

namespace TripSV.Servicios
{
    public interface ISitiosServicio
    {
        Task<List<Sitio>> ListarAsync();

        Task<List<Sitio>> ListarResumenAsync();

        Task<List<Sitio>> BuscarAsync(string? texto, int? categoriaId, string? ubicacion, decimal? calificacionMinima, string? orden);

        Task<List<string>> ListarUbicacionesAsync();

        Task<List<Sitio>> ListarPorCategoriaAsync(int categoriaId);

        Task<List<Sitio>> ListarPorCategoriaAsync(string categoria);

        Task<List<Sitio>> ListarDestacadosAsync(int cantidad);

        Task<Sitio?> ObtenerAsync(int id);

        Task<Sitio?> ObtenerPorNombreAsync(string nombre);

        Task<Resultado> CrearAsync(Sitio sitio, IFormFile? imagen);

        Task<Resultado> ActualizarAsync(Sitio sitio, IFormFile? imagen);

        Task<Resultado> EliminarAsync(int id);

        Task<Resultado> EliminarVariosAsync(int[] ids);
    }
}
