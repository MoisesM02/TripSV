using System.ComponentModel.DataAnnotations;
using TripSV.Modelos;

namespace TripSV.ViewModels
{
    public class ItinerarioFormularioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del viaje es obligatorio")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre del viaje")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; }

        [Range(1, Itinerario.MaximoDias, ErrorMessage = "La cantidad de días debe estar entre {1} y {2}")]
        [Display(Name = "Cantidad de días")]
        public int CantidadDias { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder {1} caracteres")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }

        public int? SitioId { get; set; }

        public string? NombreSitio { get; set; }
    }

    public class DiaItinerarioViewModel
    {
        public int Numero { get; set; }

        public DateTime Fecha { get; set; }

        public List<Visita> Visitas { get; set; } = new();
    }

    public class ItinerarioDetalleViewModel
    {
        public Itinerario Itinerario { get; set; } = new();

        public List<DiaItinerarioViewModel> Dias { get; set; } = new();

        public List<Sitio> SitiosFavoritos { get; set; } = new();

        public List<Sitio> OtrosSitios { get; set; } = new();

        public int TotalVisitas => Dias.Sum(d => d.Visitas.Count);
    }
}
