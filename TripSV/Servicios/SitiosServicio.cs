using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class SitiosServicio : ISitiosServicio
    {
        private readonly ContextoTripSV contexto;

        public SitiosServicio(ContextoTripSV contexto)
        {
            this.contexto = contexto;
        }

        public async Task<List<Sitio>> ListarAsync() =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .OrderBy(s => s.Nombre)
                .ToListAsync();

        public async Task<List<Sitio>> ListarPorCategoriaAsync(int categoriaId) =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .Where(s => s.CategoriaId == categoriaId)
                .OrderByDescending(s => s.Calificacion)
                .ToListAsync();

        public async Task<List<Sitio>> ListarPorCategoriaAsync(string categoria) =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .Where(s => s.Categoria!.Nombre == categoria)
                .OrderByDescending(s => s.Calificacion)
                .ToListAsync();

        public async Task<List<Sitio>> ListarDestacadosAsync(int cantidad) =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .OrderByDescending(s => s.Calificacion)
                .ThenByDescending(s => s.TotalPuntuaciones)
                .Take(cantidad)
                .ToListAsync();

        public async Task<Sitio?> ObtenerAsync(int id) =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<Sitio?> ObtenerPorNombreAsync(string nombre) =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .FirstOrDefaultAsync(s => s.Nombre == nombre);

        public async Task<Resultado> CrearAsync(Sitio sitio, IFormFile? imagen)
        {
            sitio.Nombre = sitio.Nombre.Trim();

            if (await contexto.Sitios.AnyAsync(s => s.Nombre == sitio.Nombre))
            {
                return Resultado.Error("Ya existe un sitio con ese nombre.");
            }

            if (!await contexto.Categorias.AnyAsync(c => c.Id == sitio.CategoriaId))
            {
                return Resultado.Error("La categoría seleccionada no existe.");
            }

            var validacion = ValidadorImagen.Validar(imagen);
            if (!validacion.Exito)
            {
                return validacion;
            }

            sitio.Imagen = await ValidadorImagen.LeerAsync(imagen!);
            sitio.ImagenTipo = imagen!.ContentType;
            sitio.FechaCreacion = FechaHora.Ahora;
            sitio.Calificacion = 0;
            sitio.TotalPuntuaciones = 0;

            contexto.Sitios.Add(sitio);
            await contexto.SaveChangesAsync();
            return Resultado.Ok("Sitio agregado correctamente.");
        }

        public async Task<Resultado> ActualizarAsync(Sitio sitio, IFormFile? imagen)
        {
            var actual = await contexto.Sitios.FirstOrDefaultAsync(s => s.Id == sitio.Id);
            if (actual is null)
            {
                return Resultado.Error("El sitio no existe.");
            }

            sitio.Nombre = sitio.Nombre.Trim();

            if (await contexto.Sitios.AnyAsync(s => s.Nombre == sitio.Nombre && s.Id != sitio.Id))
            {
                return Resultado.Error("Ya existe otro sitio con ese nombre.");
            }

            if (!await contexto.Categorias.AnyAsync(c => c.Id == sitio.CategoriaId))
            {
                return Resultado.Error("La categoría seleccionada no existe.");
            }

            actual.Nombre = sitio.Nombre;
            actual.Descripcion = sitio.Descripcion;
            actual.Ubicacion = sitio.Ubicacion;
            actual.CategoriaId = sitio.CategoriaId;
            actual.Informacion = sitio.Informacion;

            if (imagen is not null && imagen.Length > 0)
            {
                var validacion = ValidadorImagen.Validar(imagen);
                if (!validacion.Exito)
                {
                    return validacion;
                }

                actual.Imagen = await ValidadorImagen.LeerAsync(imagen);
                actual.ImagenTipo = imagen.ContentType;
            }

            await contexto.SaveChangesAsync();
            return Resultado.Ok("Sitio actualizado correctamente.");
        }

        public async Task<Resultado> EliminarAsync(int id) => await EliminarVariosAsync([id]);

        public async Task<Resultado> EliminarVariosAsync(int[] ids)
        {
            if (ids is null || ids.Length == 0)
            {
                return Resultado.Error("Debe seleccionar al menos un sitio para eliminar.");
            }

            var sitios = await contexto.Sitios
                .Include(s => s.Comentarios)
                .Where(s => ids.Contains(s.Id))
                .ToListAsync();

            if (sitios.Count == 0)
            {
                return Resultado.Error("No se encontraron los sitios seleccionados.");
            }

            var respuestas = sitios
                .SelectMany(s => s.Comentarios)
                .Where(c => c.RespuestaAId is not null)
                .ToList();

            contexto.Comentarios.RemoveRange(respuestas);
            await contexto.SaveChangesAsync();

            contexto.Sitios.RemoveRange(sitios);
            await contexto.SaveChangesAsync();

            return Resultado.Ok($"Se eliminaron {sitios.Count} sitio(s) correctamente.");
        }
    }
}
