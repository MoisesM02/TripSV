using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TripSV.Modelos;

namespace TripSV.ViewModels
{
    public class SitioFormularioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre del sitio")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "La ubicación es obligatoria")]
        [StringLength(60, ErrorMessage = "La ubicación no puede exceder {1} caracteres")]
        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder {1} caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Información")]
        public string? Informacion { get; set; }

        [Display(Name = "Imagen")]
        public IFormFile? Imagen { get; set; }

        public bool TieneImagen { get; set; }

        public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
    }

    public class SitioDetalleViewModel
    {
        public Sitio Sitio { get; set; } = new();

        public List<Comentario> Comentarios { get; set; } = new();

        public int? MiPuntuacion { get; set; }

        public bool PuedeParticipar { get; set; }

        public bool EsAdministrador { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El comentario no puede quedar vacío")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "El comentario debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Comentario")]
        public string? NuevoComentario { get; set; }
    }

    public class ModeracionViewModel
    {
        public List<Comentario> Comentarios { get; set; } = new();

        [Display(Name = "Buscar")]
        public string? Filtro { get; set; }

        [Display(Name = "Solo ocultos")]
        public bool SoloOcultos { get; set; }

        public int TotalVisibles { get; set; }

        public int TotalOcultos { get; set; }
    }
}
