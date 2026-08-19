using System.ComponentModel.DataAnnotations;

namespace TripSV.Modelos
{
    public class Puntuacion
    {
        public int Id { get; set; }

        public string? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Display(Name = "Sitio")]
        public int SitioId { get; set; }

        public Sitio? Sitio { get; set; }

        [Range(1, 5, ErrorMessage = "La calificación debe estar entre {1} y {2}")]
        [Display(Name = "Calificación")]
        public int Valor { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }
    }
}
