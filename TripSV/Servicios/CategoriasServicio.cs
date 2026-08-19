using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class CategoriasServicio : ICategoriasServicio
    {
        private readonly ContextoTripSV contexto;

        public CategoriasServicio(ContextoTripSV contexto)
        {
            this.contexto = contexto;
        }

        public async Task<List<Categoria>> ListarAsync() =>
            await contexto.Categorias
                .Include(c => c.Sitios)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

        public async Task<List<Categoria>> BuscarAsync(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return await ListarAsync();
            }

            return await contexto.Categorias
                .Where(c => c.Nombre.Contains(texto))
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Categoria?> ObtenerAsync(int id) =>
            await contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Categoria?> ObtenerPorNombreAsync(string nombre) =>
            await contexto.Categorias.FirstOrDefaultAsync(c => c.Nombre == nombre);

        public async Task<Resultado> CrearAsync(Categoria categoria, IFormFile? imagen)
        {
            categoria.Nombre = categoria.Nombre.Trim();

            if (await contexto.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
            {
                return Resultado.Error("Ya existe una categoría con ese nombre.");
            }

            var asignacion = await AsignarImagenAsync(categoria, imagen);
            if (!asignacion.Exito)
            {
                return asignacion;
            }

            contexto.Categorias.Add(categoria);
            await contexto.SaveChangesAsync();
            return Resultado.Ok("Categoría agregada correctamente.");
        }

        public async Task<Resultado> ActualizarAsync(Categoria categoria, IFormFile? imagen)
        {
            var actual = await contexto.Categorias.FirstOrDefaultAsync(c => c.Id == categoria.Id);
            if (actual is null)
            {
                return Resultado.Error("La categoría no existe.");
            }

            categoria.Nombre = categoria.Nombre.Trim();

            if (await contexto.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre && c.Id != categoria.Id))
            {
                return Resultado.Error("Ya existe otra categoría con ese nombre.");
            }

            actual.Nombre = categoria.Nombre;
            actual.Descripcion = categoria.Descripcion;

            var asignacion = await AsignarImagenAsync(actual, imagen);
            if (!asignacion.Exito)
            {
                return asignacion;
            }

            await contexto.SaveChangesAsync();
            return Resultado.Ok("Categoría actualizada correctamente.");
        }

        public async Task<Resultado> EliminarAsync(int id)
        {
            var categoria = await contexto.Categorias
                .Include(c => c.Sitios)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria is null)
            {
                return Resultado.Error("La categoría no existe.");
            }

            if (categoria.Sitios.Count > 0)
            {
                return Resultado.Error(
                    $"No se puede eliminar: la categoría tiene {categoria.Sitios.Count} sitio(s) asociado(s).");
            }

            contexto.Categorias.Remove(categoria);
            await contexto.SaveChangesAsync();
            return Resultado.Ok("Categoría eliminada correctamente.");
        }

        private static async Task<Resultado> AsignarImagenAsync(Categoria categoria, IFormFile? imagen)
        {
            if (imagen is null || imagen.Length == 0)
            {
                return Resultado.Ok();
            }

            var validacion = ValidadorImagen.Validar(imagen);
            if (!validacion.Exito)
            {
                return validacion;
            }

            categoria.Imagen = await ValidadorImagen.LeerAsync(imagen);
            categoria.ImagenTipo = imagen.ContentType;
            return Resultado.Ok();
        }
    }
}
