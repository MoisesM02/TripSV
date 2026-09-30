using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Pruebas
{
    public class FavoritosServicioPruebas : IDisposable
    {
        private readonly BaseDatosPrueba bd = new();

        public void Dispose() => bd.Dispose();

        [Fact]
        public async Task Alternar_SitioNoGuardado_LoAgregaAFavoritos()
        {
            var sitio = await bd.CrearSitioAsync("El Pital", await bd.CrearCategoriaAsync("montañas"));
            var usuario = await bd.CrearUsuarioAsync("visitante");
            var servicio = new FavoritosServicio(bd.Contexto);

            var resultado = await servicio.AlternarAsync(usuario.Id, sitio.Id);

            Assert.True(resultado.Exito);
            Assert.Contains("Se agregó", resultado.Mensaje);
            Assert.True(await servicio.EsFavoritoAsync(usuario.Id, sitio.Id));
        }

        [Fact]
        public async Task Alternar_SitioYaGuardado_LoQuitaDeFavoritos()
        {
            var sitio = await bd.CrearSitioAsync("El Pital", await bd.CrearCategoriaAsync("montañas"));
            var usuario = await bd.CrearUsuarioAsync("visitante");
            var servicio = new FavoritosServicio(bd.Contexto);
            await servicio.AlternarAsync(usuario.Id, sitio.Id);

            var resultado = await servicio.AlternarAsync(usuario.Id, sitio.Id);

            Assert.True(resultado.Exito);
            Assert.Contains("Se quitó", resultado.Mensaje);
            Assert.False(await servicio.EsFavoritoAsync(usuario.Id, sitio.Id));
            Assert.Equal(0, await bd.ContarAsync<Favorito>());
        }

        [Fact]
        public async Task Alternar_SitioInexistente_Rechaza()
        {
            var usuario = await bd.CrearUsuarioAsync("visitante");
            var servicio = new FavoritosServicio(bd.Contexto);

            var resultado = await servicio.AlternarAsync(usuario.Id, 999);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task Alternar_SinUsuario_Rechaza()
        {
            var sitio = await bd.CrearSitioAsync("El Pital", await bd.CrearCategoriaAsync("montañas"));
            var servicio = new FavoritosServicio(bd.Contexto);

            var resultado = await servicio.AlternarAsync(string.Empty, sitio.Id);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task ContarPorSitio_CuentaCadaUsuarioUnaSolaVez()
        {
            var sitio = await bd.CrearSitioAsync("El Pital", await bd.CrearCategoriaAsync("montañas"));
            var ana = await bd.CrearUsuarioAsync("ana");
            var beto = await bd.CrearUsuarioAsync("beto");
            var servicio = new FavoritosServicio(bd.Contexto);

            await servicio.AlternarAsync(ana.Id, sitio.Id);
            await servicio.AlternarAsync(beto.Id, sitio.Id);

            Assert.Equal(2, await servicio.ContarPorSitioAsync(sitio.Id));
        }

        [Fact]
        public async Task Listar_DevuelveSoloLosFavoritosDelUsuario()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");
            var tunco = await bd.CrearSitioAsync("El Tunco", categoria);
            var zonte = await bd.CrearSitioAsync("El Zonte", categoria);
            var ana = await bd.CrearUsuarioAsync("ana");
            var beto = await bd.CrearUsuarioAsync("beto");
            var servicio = new FavoritosServicio(bd.Contexto);
            await servicio.AlternarAsync(ana.Id, tunco.Id);
            await servicio.AlternarAsync(beto.Id, zonte.Id);

            var deAna = await new FavoritosServicio(bd.NuevoContexto()).ListarAsync(ana.Id);

            var favorito = Assert.Single(deAna);
            Assert.Equal("El Tunco", favorito.Sitio!.Nombre);
        }
    }
}
