using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class SitiosServicio : ISitiosServicio
    {
        public const string OrdenCalificacion = "calificacion";
        public const string OrdenPopulares = "populares";
        public const string OrdenNombre = "nombre";
        public const string OrdenRecientes = "recientes";

        private const string Intercalacion = "Latin1_General_CI_AI";
        private const int MaximoTerminos = 5;

        private readonly ContextoTripSV contexto;
        private readonly ISanitizadorHtml sanitizador;

        public SitiosServicio(ContextoTripSV contexto, ISanitizadorHtml sanitizador)
        {
            this.contexto = contexto;
            this.sanitizador = sanitizador;
        }

        public async Task<List<Sitio>> ListarAsync() =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .OrderBy(s => s.Nombre)
                .ToListAsync();

        public async Task<List<Sitio>> ListarResumenAsync() =>
            await contexto.Sitios
                .OrderBy(s => s.Nombre)
                .Select(s => new Sitio
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Ubicacion = s.Ubicacion,
                    CategoriaId = s.CategoriaId,
                    Categoria = new Categoria { Id = s.Categoria!.Id, Nombre = s.Categoria.Nombre }
                })
                .ToListAsync();

        public async Task<List<Sitio>> BuscarAsync(
            string? texto,
            int? categoriaId,
            string? ubicacion,
            decimal? calificacionMinima,
            string? orden)
        {
            var consulta = contexto.Sitios.AsQueryable();

            var terminos = (texto ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(t => t.Length >= 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaximoTerminos);

            foreach (var termino in terminos)
            {
                consulta = consulta.Where(s =>
                    EF.Functions.Collate(s.Nombre, Intercalacion).Contains(termino) ||
                    EF.Functions.Collate(s.Ubicacion, Intercalacion).Contains(termino) ||
                    EF.Functions.Collate(s.Descripcion, Intercalacion).Contains(termino) ||
                    EF.Functions.Collate(s.Categoria!.Nombre, Intercalacion).Contains(termino));
            }

            if (categoriaId is > 0)
            {
                consulta = consulta.Where(s => s.CategoriaId == categoriaId);
            }

            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                var departamento = ubicacion.Trim();
                consulta = consulta.Where(s => EF.Functions.Collate(s.Ubicacion, Intercalacion) == departamento);
            }

            if (calificacionMinima is > 0)
            {
                var minimo = Math.Min(calificacionMinima.Value, 5m);
                consulta = consulta.Where(s => s.Calificacion >= minimo);
            }

            consulta = orden switch
            {
                OrdenPopulares => consulta
                    .OrderByDescending(s => s.TotalPuntuaciones)
                    .ThenByDescending(s => s.Calificacion)
                    .ThenBy(s => s.Nombre),
                OrdenNombre => consulta.OrderBy(s => s.Nombre),
                OrdenRecientes => consulta
                    .OrderByDescending(s => s.FechaCreacion)
                    .ThenByDescending(s => s.Id),
                _ => consulta
                    .OrderByDescending(s => s.Calificacion)
                    .ThenByDescending(s => s.TotalPuntuaciones)
                    .ThenBy(s => s.Nombre)
            };

            return await SinImagen(consulta).ToListAsync();
        }

        public async Task<List<string>> ListarUbicacionesAsync()
        {
            var ubicaciones = await contexto.Sitios
                .Select(s => s.Ubicacion)
                .ToListAsync();

            return ubicaciones
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .GroupBy(u => u, StringComparer.CurrentCultureIgnoreCase)
                .Select(g => g.OrderBy(u => u, StringComparer.Ordinal).First())
                .OrderBy(u => u, StringComparer.CurrentCulture)
                .ToList();
        }

        public async Task<List<Sitio>> ListarPorCategoriaAsync(int categoriaId) =>
            await SinImagen(contexto.Sitios
                    .Where(s => s.CategoriaId == categoriaId)
                    .OrderByDescending(s => s.Calificacion)
                    .ThenByDescending(s => s.TotalPuntuaciones))
                .ToListAsync();

        public async Task<List<Sitio>> ListarPorCategoriaAsync(string categoria) =>
            await contexto.Sitios
                .Include(s => s.Categoria)
                .Where(s => s.Categoria!.Nombre == categoria)
                .OrderByDescending(s => s.Calificacion)
                .ToListAsync();

        public async Task<List<Sitio>> ListarDestacadosAsync(int cantidad) =>
            await SinImagen(contexto.Sitios
                    .OrderByDescending(s => s.Calificacion)
                    .ThenByDescending(s => s.TotalPuntuaciones)
                    .Take(cantidad))
                .ToListAsync();

        private static IQueryable<Sitio> SinImagen(IQueryable<Sitio> consulta) =>
            consulta.Select(s => new Sitio
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Descripcion = s.Descripcion,
                Ubicacion = s.Ubicacion,
                Calificacion = s.Calificacion,
                TotalPuntuaciones = s.TotalPuntuaciones,
                CategoriaId = s.CategoriaId,
                FechaCreacion = s.FechaCreacion,
                Categoria = new Categoria { Id = s.Categoria!.Id, Nombre = s.Categoria.Nombre }
            });

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
            sitio.Informacion = sanitizador.Limpiar(sitio.Informacion);
            sitio.Calificacion = 0;
            sitio.TotalPuntuaciones = 0;

            contexto.Sitios.Add(sitio);
            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

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
            actual.Informacion = sanitizador.Limpiar(sitio.Informacion);

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

            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

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
            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            contexto.Sitios.RemoveRange(sitios);
            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok($"Se eliminaron {sitios.Count} sitio(s) correctamente.");
        }
    }
}
