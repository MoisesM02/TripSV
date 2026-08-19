using System.ComponentModel.DataAnnotations;

namespace TripSV.Modelos
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(64, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder {1} caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Imagen")]
        public byte[]? Imagen { get; set; }

        public string? ImagenTipo { get; set; }

        public ICollection<Sitio> Sitios { get; set; } = new List<Sitio>();
    }
}
