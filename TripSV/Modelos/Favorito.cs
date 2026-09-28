using System.ComponentModel.DataAnnotations;

namespace TripSV.Modelos
{
    public class Favorito
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public Usuario? Usuario { get; set; }

        [Display(Name = "Sitio")]
        public int SitioId { get; set; }

        public Sitio? Sitio { get; set; }

        [Display(Name = "Guardado el")]
        public DateTime Fecha { get; set; }
    }
}
