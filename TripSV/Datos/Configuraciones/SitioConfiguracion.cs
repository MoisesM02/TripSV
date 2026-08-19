using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class SitioConfiguracion : IEntityTypeConfiguration<Sitio>
    {
        public void Configure(EntityTypeBuilder<Sitio> constructor)
        {
            constructor.ToTable("sitios");

            constructor.HasKey(s => s.Id);

            constructor.Property(s => s.Id).HasColumnName("id");

            constructor.Property(s => s.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            constructor.Property(s => s.Descripcion)
                .HasColumnName("descripcion")
                .HasMaxLength(500)
                .IsRequired();

            constructor.Property(s => s.Imagen)
                .HasColumnName("imagen")
                .HasColumnType("varbinary(max)");

            constructor.Property(s => s.ImagenTipo)
                .HasColumnName("imagen_tipo")
                .HasMaxLength(50);

            constructor.Property(s => s.Calificacion)
                .HasColumnName("calificacion")
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0m);

            constructor.Property(s => s.TotalPuntuaciones)
                .HasColumnName("total_puntuaciones")
                .HasDefaultValue(0);

            constructor.Property(s => s.Ubicacion)
                .HasColumnName("ubicacion")
                .HasMaxLength(60)
                .IsRequired();

            constructor.Property(s => s.CategoriaId).HasColumnName("categoria_id");

            constructor.Property(s => s.Informacion)
                .HasColumnName("informacion")
                .HasColumnType("nvarchar(max)");

            constructor.Property(s => s.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("SYSDATETIME()");

            constructor.HasIndex(s => s.Nombre).IsUnique();

            constructor.HasOne(s => s.Categoria)
                .WithMany(c => c.Sitios)
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
