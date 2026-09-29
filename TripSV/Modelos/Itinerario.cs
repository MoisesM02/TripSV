using System.ComponentModel.DataAnnotations;

namespace TripSV.Modelos
{
    public class Itinerario
    {
        public const int MaximoDias = 15;

        public const int MaximoVisitasPorDia = 10;

        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "El nombre del viaje es obligatorio")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre del viaje")]
        public string Nombre { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; }

        [Range(1, MaximoDias, ErrorMessage = "La cantidad de días debe estar entre {1} y {2}")]
        [Display(Name = "Cantidad de días")]
        public int CantidadDias { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder {1} caracteres")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();

        public DateTime FechaFin => FechaInicio.AddDays(CantidadDias - 1);
    }
}
