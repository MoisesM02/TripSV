using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TripSV.Modelos;

namespace TripSV.Datos
{
    public class ContextoTripSV : IdentityDbContext<Usuario>
    {
        public ContextoTripSV(DbContextOptions<ContextoTripSV> opciones) : base(opciones)
        {
        }

        public DbSet<Categoria> Categorias => Set<Categoria>();

        public DbSet<Sitio> Sitios => Set<Sitio>();

        public DbSet<Comentario> Comentarios => Set<Comentario>();

        public DbSet<Puntuacion> Puntuaciones => Set<Puntuacion>();

        protected override void OnModelCreating(ModelBuilder modelo)
        {
            base.OnModelCreating(modelo);

            modelo.ApplyConfigurationsFromAssembly(typeof(ContextoTripSV).Assembly);

            modelo.Entity<Usuario>(constructor =>
            {
                constructor.ToTable("usuarios");
                constructor.Property(u => u.FechaRegistro)
                    .HasColumnName("fecha_registro")
                    .HasDefaultValueSql("SYSDATETIME()");
            });

            modelo.Entity<IdentityRole>().ToTable("roles");
            modelo.Entity<IdentityUserRole<string>>().ToTable("usuarios_roles");
            modelo.Entity<IdentityUserClaim<string>>().ToTable("usuarios_claims");
            modelo.Entity<IdentityUserLogin<string>>().ToTable("usuarios_logins");
            modelo.Entity<IdentityUserToken<string>>().ToTable("usuarios_tokens");
            modelo.Entity<IdentityRoleClaim<string>>().ToTable("roles_claims");
        }
    }
}
