using System.ComponentModel.DataAnnotations;

namespace TripSV.Modelos
{
    public class Visita
    {
        public int Id { get; set; }

        public int ItinerarioId { get; set; }

        public Itinerario? Itinerario { get; set; }

        [Display(Name = "Sitio")]
        public int SitioId { get; set; }

        public Sitio? Sitio { get; set; }

        [Range(1, Itinerario.MaximoDias, ErrorMessage = "El día debe estar entre {1} y {2}")]
        [Display(Name = "Día")]
        public int Dia { get; set; }

        public int Orden { get; set; }

        [StringLength(200, ErrorMessage = "Las notas no pueden exceder {1} caracteres")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}
