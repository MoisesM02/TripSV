using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Pruebas
{
    public sealed class BaseDatosPrueba : IDisposable
    {
        private readonly SqliteConnection conexion;
        private readonly DbContextOptions<ContextoTripSV> opciones;

        public BaseDatosPrueba()
        {
            conexion = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
            conexion.Open();

            opciones = new DbContextOptionsBuilder<ContextoTripSV>()
                .UseSqlite(conexion)
                .Options;

            Contexto = new ContextoPrueba(opciones);
            Contexto.Database.EnsureCreated();
        }

        public ContextoTripSV Contexto { get; }

        public ContextoTripSV NuevoContexto() => new ContextoPrueba(opciones);

        public async Task<int> ContarAsync<T>() where T : class
        {
            using var contexto = NuevoContexto();
            return await contexto.Set<T>().CountAsync();
        }

        public async Task<Usuario> CrearUsuarioAsync(string nombre)
        {
            var usuario = new Usuario
            {
                UserName = nombre,
                NormalizedUserName = nombre.ToUpperInvariant(),
                Email = $"{nombre}@tripssv.com",
                FechaRegistro = DateTime.Now
            };

            Contexto.Users.Add(usuario);
            await Contexto.SaveChangesAsync();
            return usuario;
        }

        public async Task<Categoria> CrearCategoriaAsync(string nombre)
        {
            var categoria = new Categoria { Nombre = nombre, Descripcion = $"Categoría {nombre}" };
            Contexto.Categorias.Add(categoria);
            await Contexto.SaveChangesAsync();
            return categoria;
        }

        public async Task<Sitio> CrearSitioAsync(string nombre, Categoria categoria, string ubicacion = "San Salvador")
        {
            var sitio = new Sitio
            {
                Nombre = nombre,
                Descripcion = $"Descripción de {nombre}",
                Ubicacion = ubicacion,
                CategoriaId = categoria.Id,
                FechaCreacion = DateTime.Now
            };

            Contexto.Sitios.Add(sitio);
            await Contexto.SaveChangesAsync();
            return sitio;
        }

        public void Dispose()
        {
            Contexto.Dispose();
            conexion.Dispose();
        }

        private sealed class ContextoPrueba : ContextoTripSV
        {
            public ContextoPrueba(DbContextOptions<ContextoTripSV> opciones) : base(opciones)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelo)
            {
                base.OnModelCreating(modelo);

                foreach (var propiedad in modelo.Model.GetEntityTypes().SelectMany(e => e.GetProperties()))
                {
                    if (propiedad.GetDefaultValueSql() == "SYSDATETIME()")
                    {
                        propiedad.SetDefaultValueSql("CURRENT_TIMESTAMP");
                    }

                    if (propiedad.GetColumnType()?.Contains("(max)", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        propiedad.SetColumnType(null);
                    }
                }
            }
        }
    }
}
