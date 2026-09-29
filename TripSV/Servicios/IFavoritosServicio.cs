using TripSV.Modelos;

namespace TripSV.Servicios
{
    public interface IFavoritosServicio
    {
        Task<List<Favorito>> ListarAsync(string usuarioId);

        Task<List<int>> ObtenerIdsSitiosAsync(string usuarioId);

        Task<bool> EsFavoritoAsync(string usuarioId, int sitioId);

        Task<int> ContarPorSitioAsync(int sitioId);

        Task<Resultado> AlternarAsync(string usuarioId, int sitioId);
    }
}
