using TripSV.Modelos;

namespace TripSV.Servicios
{
    public interface ICategoriasServicio
    {
        Task<List<Categoria>> ListarAsync();

        Task<List<Categoria>> BuscarAsync(string texto);

        Task<Categoria?> ObtenerAsync(int id);

        Task<Categoria?> ObtenerPorNombreAsync(string nombre);

        Task<Resultado> CrearAsync(Categoria categoria, IFormFile? imagen);

        Task<Resultado> ActualizarAsync(Categoria categoria, IFormFile? imagen);

        Task<Resultado> EliminarAsync(int id);
    }
}
