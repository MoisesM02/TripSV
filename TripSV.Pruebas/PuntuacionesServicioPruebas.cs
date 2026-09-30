using Microsoft.EntityFrameworkCore;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Pruebas
{
    public class PuntuacionesServicioPruebas : IDisposable
    {
        private readonly BaseDatosPrueba bd = new();

        public void Dispose() => bd.Dispose();

        [Fact]
        public async Task Calificar_PrimeraVez_CreaPuntuacionYActualizaPromedio()
        {
            var sitio = await bd.CrearSitioAsync("El Tunco", await bd.CrearCategoriaAsync("playa"));
            var usuario = await bd.CrearUsuarioAsync("visitante");
            var servicio = new PuntuacionesServicio(bd.Contexto);

            var resultado = await servicio.CalificarAsync(sitio.Id, usuario.UserName!, usuario.Id, 4);

            Assert.True(resultado.Exito);
            using var verificacion = bd.NuevoContexto();
            var guardado = await verificacion.Sitios.SingleAsync(s => s.Id == sitio.Id);
            Assert.Equal(4m, guardado.Calificacion);
            Assert.Equal(1, guardado.TotalPuntuaciones);
        }

        [Fact]
        public async Task Calificar_SegundaVezMismoUsuario_ActualizaSinDuplicar()
        {
            var sitio = await bd.CrearSitioAsync("El Tunco", await bd.CrearCategoriaAsync("playa"));
            var servicio = new PuntuacionesServicio(bd.Contexto);

            await servicio.CalificarAsync(sitio.Id, "visitante", null, 2);
            var resultado = await servicio.CalificarAsync(sitio.Id, "visitante", null, 5);

            Assert.True(resultado.Exito);
            Assert.Contains("actualizó", resultado.Mensaje);
            using var verificacion = bd.NuevoContexto();
            Assert.Equal(1, await verificacion.Puntuaciones.CountAsync());
            Assert.Equal(5, await servicio.ObtenerDeUsuarioAsync(sitio.Id, "visitante"));
        }

        [Fact]
        public async Task Calificar_VariosUsuarios_CalculaPromedioRedondeadoAUnDecimal()
        {
            var sitio = await bd.CrearSitioAsync("Lago de Coatepeque", await bd.CrearCategoriaAsync("lagos"));
            var servicio = new PuntuacionesServicio(bd.Contexto);

            await servicio.CalificarAsync(sitio.Id, "ana", null, 5);
            await servicio.CalificarAsync(sitio.Id, "beto", null, 4);
            await servicio.CalificarAsync(sitio.Id, "carla", null, 4);

            using var verificacion = bd.NuevoContexto();
            var guardado = await verificacion.Sitios.SingleAsync(s => s.Id == sitio.Id);
            Assert.Equal(4.3m, guardado.Calificacion);
            Assert.Equal(3, guardado.TotalPuntuaciones);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public async Task Calificar_ValorFueraDeRango_Rechaza(int valor)
        {
            var sitio = await bd.CrearSitioAsync("El Tunco", await bd.CrearCategoriaAsync("playa"));
            var servicio = new PuntuacionesServicio(bd.Contexto);

            var resultado = await servicio.CalificarAsync(sitio.Id, "visitante", null, valor);

            Assert.False(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Puntuacion>());
        }

        [Fact]
        public async Task Calificar_SinUsuario_Rechaza()
        {
            var sitio = await bd.CrearSitioAsync("El Tunco", await bd.CrearCategoriaAsync("playa"));
            var servicio = new PuntuacionesServicio(bd.Contexto);

            var resultado = await servicio.CalificarAsync(sitio.Id, " ", null, 3);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task Calificar_SitioInexistente_Rechaza()
        {
            var servicio = new PuntuacionesServicio(bd.Contexto);

            var resultado = await servicio.CalificarAsync(999, "visitante", null, 3);

            Assert.False(resultado.Exito);
            Assert.Equal("El sitio no existe.", resultado.Mensaje);
        }
    }
}
