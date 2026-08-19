using TripSV.Modelos;

namespace TripSV.Servicios
{
    public interface IComentariosServicio
    {
        Task<List<Comentario>> ListarPorSitioAsync(int sitioId, bool incluirOcultos = false);

        Task<List<Comentario>> ListarParaModeracionAsync(string? filtro, bool soloOcultos);

        Task<Comentario?> ObtenerAsync(int id);

        Task<Resultado> AgregarAsync(int sitioId, string nombreUsuario, string? usuarioId, string texto, int? respuestaAId);

        Task<Resultado> EliminarAsync(int id, string nombreUsuario, bool esAdministrador);

        Task<Resultado> CambiarVisibilidadAsync(int id, bool oculto);
    }
}
