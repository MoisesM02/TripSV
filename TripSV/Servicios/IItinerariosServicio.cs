using TripSV.Modelos;

namespace TripSV.Servicios
{
    public interface IItinerariosServicio
    {
        Task<List<Itinerario>> ListarAsync(string usuarioId);

        Task<Itinerario?> ObtenerAsync(int id, string usuarioId);

        Task<Itinerario?> ObtenerConVisitasAsync(int id, string usuarioId);

        Task<Resultado> CrearAsync(Itinerario itinerario);

        Task<Resultado> ActualizarAsync(Itinerario itinerario, string usuarioId);

        Task<Resultado> EliminarAsync(int id, string usuarioId);

        Task<Resultado> AgregarVisitaAsync(int itinerarioId, string usuarioId, int sitioId, int dia, string? notas);

        Task<Resultado> QuitarVisitaAsync(int visitaId, string usuarioId);

        Task<Resultado> MoverVisitaAsync(int visitaId, string usuarioId, int desplazamiento);

        Task<Resultado> CambiarDiaAsync(int visitaId, string usuarioId, int dia);
    }
}
