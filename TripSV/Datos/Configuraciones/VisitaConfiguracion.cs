using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class VisitaConfiguracion : IEntityTypeConfiguration<Visita>
    {
        public void Configure(EntityTypeBuilder<Visita> constructor)
        {
            constructor.ToTable("visitas", t =>
                t.HasCheckConstraint("CK_visitas_dia", $"[dia] BETWEEN 1 AND {Itinerario.MaximoDias}"));

            constructor.HasKey(v => v.Id);

            constructor.Property(v => v.Id).HasColumnName("id");

            constructor.Property(v => v.ItinerarioId).HasColumnName("itinerario_id");

            constructor.Property(v => v.SitioId).HasColumnName("sitio_id");

            constructor.Property(v => v.Dia).HasColumnName("dia");

            constructor.Property(v => v.Orden).HasColumnName("orden");

            constructor.Property(v => v.Notas)
                .HasColumnName("notas")
                .HasMaxLength(200);

            constructor.HasIndex(v => new { v.ItinerarioId, v.Dia, v.SitioId }).IsUnique();

            constructor.HasIndex(v => v.SitioId);

            constructor.HasOne(v => v.Itinerario)
                .WithMany(i => i.Visitas)
                .HasForeignKey(v => v.ItinerarioId)
                .OnDelete(DeleteBehavior.Cascade);

            constructor.HasOne(v => v.Sitio)
                .WithMany(s => s.Visitas)
                .HasForeignKey(v => v.SitioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
