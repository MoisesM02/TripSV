using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class ComentariosServicio : IComentariosServicio
    {
        private readonly ContextoTripSV contexto;

        public ComentariosServicio(ContextoTripSV contexto)
        {
            this.contexto = contexto;
        }

        public async Task<List<Comentario>> ListarPorSitioAsync(int sitioId, bool incluirOcultos = false)
        {
            var consulta = contexto.Comentarios
                .Include(c => c.Respuestas)
                .Where(c => c.SitioId == sitioId && c.RespuestaAId == null);

            if (!incluirOcultos)
            {
                consulta = consulta.Where(c => !c.Oculto);
            }

            var comentarios = await consulta.OrderBy(c => c.Id).ToListAsync();

            foreach (var comentario in comentarios)
            {
                comentario.Respuestas = comentario.Respuestas
                    .Where(r => incluirOcultos || !r.Oculto)
                    .OrderBy(r => r.Id)
                    .ToList();
            }

            return comentarios;
        }

        public async Task<List<Comentario>> ListarParaModeracionAsync(string? filtro, bool soloOcultos)
        {
            var consulta = contexto.Comentarios
                .Include(c => c.Sitio)
                .Include(c => c.RespuestaA)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                consulta = consulta.Where(c =>
                    c.Texto.Contains(filtro) ||
                    c.NombreUsuario.Contains(filtro) ||
                    c.Sitio!.Nombre.Contains(filtro));
            }

            if (soloOcultos)
            {
                consulta = consulta.Where(c => c.Oculto);
            }

            return await consulta
                .OrderByDescending(c => c.Fecha)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<Comentario?> ObtenerAsync(int id) =>
            await contexto.Comentarios
                .Include(c => c.Sitio)
                .Include(c => c.Respuestas)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Resultado> AgregarAsync(
            int sitioId,
            string nombreUsuario,
            string? usuarioId,
            string texto,
            int? respuestaAId)
        {
            if (string.IsNullOrWhiteSpace(texto) || string.IsNullOrWhiteSpace(nombreUsuario) || sitioId <= 0)
            {
                return Resultado.Error("Faltan datos para publicar el comentario.");
            }

            if (!await contexto.Sitios.AnyAsync(s => s.Id == sitioId))
            {
                return Resultado.Error("El sitio no existe.");
            }

            if (respuestaAId is not null)
            {
                var padre = await contexto.Comentarios
                    .FirstOrDefaultAsync(c => c.Id == respuestaAId && c.SitioId == sitioId);

                if (padre is null)
                {
                    return Resultado.Error("El comentario que intenta responder no existe.");
                }

                if (padre.RespuestaAId is not null)
                {
                    respuestaAId = padre.RespuestaAId;
                }
            }

            var comentario = new Comentario
            {
                SitioId = sitioId,
                NombreUsuario = nombreUsuario,
                UsuarioId = usuarioId,
                Texto = texto.Trim(),
                Fecha = FechaHora.Ahora,
                RespuestaAId = respuestaAId,
                Oculto = false
            };

            contexto.Comentarios.Add(comentario);
            await contexto.SaveChangesAsync();

            return Resultado.Ok(respuestaAId is null
                ? "Comentario publicado correctamente."
                : "Respuesta publicada correctamente.");
        }

        public async Task<Resultado> EliminarAsync(int id, string nombreUsuario, bool esAdministrador)
        {
            var comentario = await contexto.Comentarios
                .Include(c => c.Respuestas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comentario is null)
            {
                return Resultado.Error("El comentario no existe.");
            }

            var esAutor = string.Equals(comentario.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase);

            if (!esAdministrador && !esAutor)
            {
                return Resultado.Error("Solo puede eliminar sus propios comentarios.");
            }

            if (comentario.Respuestas.Count > 0)
            {
                contexto.Comentarios.RemoveRange(comentario.Respuestas);
                await contexto.SaveChangesAsync();
            }

            contexto.Comentarios.Remove(comentario);
            await contexto.SaveChangesAsync();

            return Resultado.Ok("Comentario eliminado correctamente.");
        }

        public async Task<Resultado> CambiarVisibilidadAsync(int id, bool oculto)
        {
            var comentario = await contexto.Comentarios
                .Include(c => c.Respuestas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comentario is null)
            {
                return Resultado.Error("El comentario no existe.");
            }

            comentario.Oculto = oculto;

            foreach (var respuesta in comentario.Respuestas)
            {
                respuesta.Oculto = oculto;
            }

            await contexto.SaveChangesAsync();

            return Resultado.Ok(oculto
                ? "El comentario quedó oculto para los visitantes."
                : "El comentario volvió a ser visible.");
        }
    }
}
