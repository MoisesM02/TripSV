using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class FavoritosServicio : IFavoritosServicio
    {
        private readonly ContextoTripSV contexto;

        public FavoritosServicio(ContextoTripSV contexto)
        {
            this.contexto = contexto;
        }

        public async Task<List<Favorito>> ListarAsync(string usuarioId) =>
            await contexto.Favoritos
                .Include(f => f.Sitio)
                    .ThenInclude(s => s!.Categoria)
                .Where(f => f.UsuarioId == usuarioId)
                .OrderByDescending(f => f.Fecha)
                .ThenByDescending(f => f.Id)
                .ToListAsync();

        public async Task<List<int>> ObtenerIdsSitiosAsync(string usuarioId) =>
            await contexto.Favoritos
                .Where(f => f.UsuarioId == usuarioId)
                .Select(f => f.SitioId)
                .ToListAsync();

        public async Task<bool> EsFavoritoAsync(string usuarioId, int sitioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return false;
            }

            return await contexto.Favoritos.AnyAsync(f => f.UsuarioId == usuarioId && f.SitioId == sitioId);
        }

        public async Task<int> ContarPorSitioAsync(int sitioId) =>
            await contexto.Favoritos.CountAsync(f => f.SitioId == sitioId);

        public async Task<Resultado> AlternarAsync(string usuarioId, int sitioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Resultado.Error("Debe iniciar sesión para guardar destinos en favoritos.");
            }

            var sitio = await contexto.Sitios.FirstOrDefaultAsync(s => s.Id == sitioId);
            if (sitio is null)
            {
                return Resultado.Error("El sitio no existe.");
            }

            var existente = await contexto.Favoritos
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.SitioId == sitioId);

            if (existente is not null)
            {
                contexto.Favoritos.Remove(existente);
                await contexto.SaveChangesAsync();
                return Resultado.Ok($"Se quitó {sitio.Nombre} de sus favoritos.");
            }

            contexto.Favoritos.Add(new Favorito
            {
                UsuarioId = usuarioId,
                SitioId = sitioId,
                Fecha = FechaHora.Ahora
            });

            await contexto.SaveChangesAsync();
            return Resultado.Ok($"Se agregó {sitio.Nombre} a sus favoritos.");
        }
    }
}
