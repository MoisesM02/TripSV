using Microsoft.AspNetCore.Identity;

namespace TripSV.Modelos
{
    public class Usuario : IdentityUser
    {
        public DateTime FechaRegistro { get; set; }

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

        public ICollection<Puntuacion> Puntuaciones { get; set; } = new List<Puntuacion>();
    }
}
