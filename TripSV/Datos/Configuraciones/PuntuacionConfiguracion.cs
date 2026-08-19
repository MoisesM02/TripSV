using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class PuntuacionConfiguracion : IEntityTypeConfiguration<Puntuacion>
    {
        public void Configure(EntityTypeBuilder<Puntuacion> constructor)
        {
            constructor.ToTable("puntuaciones", t =>
                t.HasCheckConstraint("CK_puntuaciones_valor", "[puntuacion] BETWEEN 1 AND 5"));

            constructor.HasKey(p => p.Id);

            constructor.Property(p => p.Id).HasColumnName("id");

            constructor.Property(p => p.UsuarioId)
                .HasColumnName("usuario_id")
                .HasMaxLength(450);

            constructor.Property(p => p.NombreUsuario)
                .HasColumnName("nombre_usuario")
                .HasMaxLength(64)
                .IsRequired();

            constructor.Property(p => p.SitioId).HasColumnName("sitio_id");

            constructor.Property(p => p.Valor).HasColumnName("puntuacion");

            constructor.Property(p => p.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSDATETIME()");

            constructor.HasIndex(p => new { p.SitioId, p.NombreUsuario }).IsUnique();

            constructor.HasOne(p => p.Sitio)
                .WithMany(s => s.Puntuaciones)
                .HasForeignKey(p => p.SitioId)
                .OnDelete(DeleteBehavior.Cascade);

            constructor.HasOne(p => p.Usuario)
                .WithMany(u => u.Puntuaciones)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
