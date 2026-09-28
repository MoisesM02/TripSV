using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class FavoritoConfiguracion : IEntityTypeConfiguration<Favorito>
    {
        public void Configure(EntityTypeBuilder<Favorito> constructor)
        {
            constructor.ToTable("favoritos");

            constructor.HasKey(f => f.Id);

            constructor.Property(f => f.Id).HasColumnName("id");

            constructor.Property(f => f.UsuarioId)
                .HasColumnName("usuario_id")
                .HasMaxLength(450)
                .IsRequired();

            constructor.Property(f => f.SitioId).HasColumnName("sitio_id");

            constructor.Property(f => f.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSDATETIME()");

            constructor.HasIndex(f => new { f.UsuarioId, f.SitioId }).IsUnique();

            constructor.HasIndex(f => f.SitioId);

            constructor.HasOne(f => f.Usuario)
                .WithMany(u => u.Favoritos)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            constructor.HasOne(f => f.Sitio)
                .WithMany(s => s.Favoritos)
                .HasForeignKey(f => f.SitioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
