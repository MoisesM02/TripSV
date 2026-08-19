using System.ComponentModel.DataAnnotations;


namespace TripSV.Modelos
{
    public class Sitio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder {1} caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Imagen")]
        public byte[]? Imagen { get; set; }

        public string? ImagenTipo { get; set; }

        [Display(Name = "Calificación")]
        public decimal Calificacion { get; set; }

        [Display(Name = "Total de calificaciones")]
        public int TotalPuntuaciones { get; set; }

        [Required(ErrorMessage = "La ubicación es obligatoria")]
        [StringLength(60, ErrorMessage = "La ubicación no puede exceder {1} caracteres")]
        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; } = string.Empty;

        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }

        [Display(Name = "Información")]
        public string? Informacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

        public ICollection<Puntuacion> Puntuaciones { get; set; } = new List<Puntuacion>();

    }
}
