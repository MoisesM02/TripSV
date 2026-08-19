using System.ComponentModel.DataAnnotations;

namespace TripSV.Modelos
{
    public class Comentario
    {
        public int Id { get; set; }

        public string? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El comentario no puede quedar vacío")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "El comentario debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Comentario")]
        public string Texto { get; set; } = string.Empty;

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }

        [Display(Name = "Sitio")]
        public int SitioId { get; set; }

        public Sitio? Sitio { get; set; }

        [Display(Name = "Respuesta a")]
        public int? RespuestaAId { get; set; }

        public Comentario? RespuestaA { get; set; }

        public ICollection<Comentario> Respuestas { get; set; } = new List<Comentario>();

        [Display(Name = "Oculto")]
        public bool Oculto { get; set; }
    }
}
