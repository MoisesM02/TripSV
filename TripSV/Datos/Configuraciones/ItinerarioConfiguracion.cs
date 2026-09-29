using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class ItinerarioConfiguracion : IEntityTypeConfiguration<Itinerario>
    {
        public void Configure(EntityTypeBuilder<Itinerario> constructor)
        {
            constructor.ToTable("itinerarios", t =>
                t.HasCheckConstraint("CK_itinerarios_cantidad_dias", $"[cantidad_dias] BETWEEN 1 AND {Itinerario.MaximoDias}"));

            constructor.HasKey(i => i.Id);

            constructor.Property(i => i.Id).HasColumnName("id");

            constructor.Property(i => i.UsuarioId)
                .HasColumnName("usuario_id")
                .HasMaxLength(450)
                .IsRequired();

            constructor.Property(i => i.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(80)
                .IsRequired();

            constructor.Property(i => i.FechaInicio)
                .HasColumnName("fecha_inicio")
                .HasColumnType("date");

            constructor.Property(i => i.CantidadDias).HasColumnName("cantidad_dias");

            constructor.Property(i => i.Notas)
                .HasColumnName("notas")
                .HasMaxLength(500);

            constructor.Property(i => i.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSDATETIME()");

            constructor.Ignore(i => i.FechaFin);

            constructor.HasIndex(i => i.UsuarioId);

            constructor.HasOne(i => i.Usuario)
                .WithMany(u => u.Itinerarios)
                .HasForeignKey(i => i.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
