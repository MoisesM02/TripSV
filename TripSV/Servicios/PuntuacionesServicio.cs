using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class PuntuacionesServicio : IPuntuacionesServicio
    {
        private readonly ContextoTripSV contexto;

        public PuntuacionesServicio(ContextoTripSV contexto)
        {
            this.contexto = contexto;
        }

        public async Task<Resultado> CalificarAsync(int sitioId, string nombreUsuario, string? usuarioId, int valor)
        {
            if (valor < 1 || valor > 5)
            {
                return Resultado.Error("La calificación debe estar entre 1 y 5 estrellas.");
            }

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return Resultado.Error("Debe iniciar sesión para calificar este sitio.");
            }

            var sitio = await contexto.Sitios.FirstOrDefaultAsync(s => s.Id == sitioId);
            if (sitio is null)
            {
                return Resultado.Error("El sitio no existe.");
            }

            var existente = await contexto.Puntuaciones
                .FirstOrDefaultAsync(p => p.SitioId == sitioId && p.NombreUsuario == nombreUsuario);

            var actualizada = existente is not null;

            if (existente is not null)
            {
                existente.Valor = valor;
                existente.Fecha = FechaHora.Ahora;
                existente.UsuarioId ??= usuarioId;
            }
            else
            {
                contexto.Puntuaciones.Add(new Puntuacion
                {
                    SitioId = sitioId,
                    NombreUsuario = nombreUsuario,
                    UsuarioId = usuarioId,
                    Valor = valor,
                    Fecha = FechaHora.Ahora
                });
            }

            await contexto.SaveChangesAsync();

            var promedio = await RecalcularPromedioAsync(sitioId);

            return Resultado.Ok(actualizada
                ? $"Su calificación se actualizó a {valor} estrella(s). Promedio del sitio: {promedio:0.0}/5."
                : $"Calificó este sitio con {valor} estrella(s). Promedio del sitio: {promedio:0.0}/5.");
        }

        public async Task<int?> ObtenerDeUsuarioAsync(int sitioId, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return null;
            }

            return await contexto.Puntuaciones
                .Where(p => p.SitioId == sitioId && p.NombreUsuario == nombreUsuario)
                .Select(p => (int?)p.Valor)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> RecalcularPromedioAsync(int sitioId)
        {
            var sitio = await contexto.Sitios.FirstOrDefaultAsync(s => s.Id == sitioId);
            if (sitio is null)
            {
                return 0;
            }

            var puntuaciones = await contexto.Puntuaciones
                .Where(p => p.SitioId == sitioId)
                .Select(p => p.Valor)
                .ToListAsync();

            sitio.TotalPuntuaciones = puntuaciones.Count;
            sitio.Calificacion = puntuaciones.Count == 0
                ? 0
                : Math.Round((decimal)puntuaciones.Average(), 1, MidpointRounding.AwayFromZero);

            await contexto.SaveChangesAsync();

            return sitio.Calificacion;
        }
    }
}
