using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripSV.Modelos;

namespace TripSV.Datos.Configuraciones
{
    public class CategoriaConfiguracion : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> constructor)
        {
            constructor.ToTable("categorias");

            constructor.HasKey(c => c.Id);

            constructor.Property(c => c.Id).HasColumnName("id");

            constructor.Property(c => c.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(64)
                .IsRequired();

            constructor.Property(c => c.Descripcion)
                .HasColumnName("descripcion")
                .HasMaxLength(200);

            constructor.Property(c => c.Imagen)
                .HasColumnName("imagen")
                .HasColumnType("varbinary(max)");

            constructor.Property(c => c.ImagenTipo)
                .HasColumnName("imagen_tipo")
                .HasMaxLength(50);

            constructor.HasIndex(c => c.Nombre).IsUnique();
        }
    }
}
