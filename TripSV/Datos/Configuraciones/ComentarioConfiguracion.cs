using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class ComentarioConfiguracion : IEntityTypeConfiguration<Comentario>
    {
        public void Configure(EntityTypeBuilder<Comentario> constructor)
        {
            constructor.ToTable("comentarios");

            constructor.HasKey(c => c.Id);

            constructor.Property(c => c.Id).HasColumnName("id");

            constructor.Property(c => c.UsuarioId)
                .HasColumnName("usuario_id")
                .HasMaxLength(450);

            constructor.Property(c => c.NombreUsuario)
                .HasColumnName("nombre_usuario")
                .HasMaxLength(64)
                .IsRequired();

            constructor.Property(c => c.Texto)
                .HasColumnName("comentario")
                .HasMaxLength(1000)
                .IsRequired();

            constructor.Property(c => c.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSDATETIME()");

            constructor.Property(c => c.SitioId).HasColumnName("sitio_id");

            constructor.Property(c => c.RespuestaAId).HasColumnName("respuesta_a_id");

            constructor.Property(c => c.Oculto)
                .HasColumnName("oculto")
                .HasDefaultValue(false);

            constructor.HasIndex(c => c.SitioId);

            constructor.HasIndex(c => c.RespuestaAId);

            constructor.HasOne(c => c.Sitio)
                .WithMany(s => s.Comentarios)
                .HasForeignKey(c => c.SitioId)
                .OnDelete(DeleteBehavior.Cascade);

            constructor.HasOne(c => c.Usuario)
                .WithMany(u => u.Comentarios)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            constructor.HasOne(c => c.RespuestaA)
                .WithMany(c => c.Respuestas)
                .HasForeignKey(c => c.RespuestaAId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
