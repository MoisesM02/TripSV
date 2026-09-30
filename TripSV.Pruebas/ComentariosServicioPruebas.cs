using Microsoft.EntityFrameworkCore;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Pruebas
{
    public class ComentariosServicioPruebas : IDisposable
    {
        private readonly BaseDatosPrueba bd = new();

        public void Dispose() => bd.Dispose();

        private async Task<(Sitio sitio, ComentariosServicio servicio)> PrepararAsync()
        {
            var sitio = await bd.CrearSitioAsync("Joya de Cerén", await bd.CrearCategoriaAsync("arqueología"));
            return (sitio, new ComentariosServicio(bd.Contexto));
        }

        private async Task<Comentario> ObtenerUnicoAsync(string texto)
        {
            using var verificacion = bd.NuevoContexto();
            return await verificacion.Comentarios.SingleAsync(c => c.Texto == texto);
        }

        [Fact]
        public async Task Agregar_ComentarioValido_SeGuardaConAutorYFecha()
        {
            var (sitio, servicio) = await PrepararAsync();

            var resultado = await servicio.AgregarAsync(sitio.Id, "visitante", null, "Excelente lugar", null);

            Assert.True(resultado.Exito);
            var guardado = await ObtenerUnicoAsync("Excelente lugar");
            Assert.Equal("visitante", guardado.NombreUsuario);
            Assert.False(guardado.Oculto);
            Assert.Null(guardado.RespuestaAId);
            Assert.NotEqual(default, guardado.Fecha);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Agregar_TextoVacio_Rechaza(string texto)
        {
            var (sitio, servicio) = await PrepararAsync();

            var resultado = await servicio.AgregarAsync(sitio.Id, "visitante", null, texto, null);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task Agregar_RespuestaAUnaRespuesta_SeAgrupaBajoElComentarioOriginal()
        {
            var (sitio, servicio) = await PrepararAsync();
            await servicio.AgregarAsync(sitio.Id, "ana", null, "Comentario original", null);
            var original = await ObtenerUnicoAsync("Comentario original");
            await servicio.AgregarAsync(sitio.Id, "beto", null, "Primera respuesta", original.Id);
            var primeraRespuesta = await ObtenerUnicoAsync("Primera respuesta");

            var resultado = await servicio.AgregarAsync(sitio.Id, "carla", null, "Respuesta a la respuesta", primeraRespuesta.Id);

            Assert.True(resultado.Exito);
            var anidada = await ObtenerUnicoAsync("Respuesta a la respuesta");
            Assert.Equal(original.Id, anidada.RespuestaAId);
        }

        [Fact]
        public async Task Agregar_RespuestaAComentarioDeOtroSitio_Rechaza()
        {
            var (sitio, servicio) = await PrepararAsync();
            var otroSitio = await bd.CrearSitioAsync("El Tazumal", await bd.CrearCategoriaAsync("mayas"));
            await servicio.AgregarAsync(otroSitio.Id, "ana", null, "Comentario en otro sitio", null);
            var ajeno = await ObtenerUnicoAsync("Comentario en otro sitio");

            var resultado = await servicio.AgregarAsync(sitio.Id, "beto", null, "Respuesta cruzada", ajeno.Id);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task Eliminar_ComentarioAjenoSiendoUsuario_Rechaza()
        {
            var (sitio, servicio) = await PrepararAsync();
            await servicio.AgregarAsync(sitio.Id, "ana", null, "Comentario de Ana", null);
            var comentario = await ObtenerUnicoAsync("Comentario de Ana");

            var resultado = await servicio.EliminarAsync(comentario.Id, "beto", esAdministrador: false);

            Assert.False(resultado.Exito);
            Assert.Equal(1, await bd.ContarAsync<Comentario>());
        }

        [Fact]
        public async Task Eliminar_ComentarioPropio_EliminaTambienSusRespuestas()
        {
            var (sitio, servicio) = await PrepararAsync();
            await servicio.AgregarAsync(sitio.Id, "ana", null, "Comentario de Ana", null);
            var comentario = await ObtenerUnicoAsync("Comentario de Ana");
            await servicio.AgregarAsync(sitio.Id, "beto", null, "Respuesta de Beto", comentario.Id);

            var resultado = await servicio.EliminarAsync(comentario.Id, "ANA", esAdministrador: false);

            Assert.True(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Comentario>());
        }

        [Fact]
        public async Task Eliminar_ComentarioAjenoSiendoAdministrador_Permite()
        {
            var (sitio, servicio) = await PrepararAsync();
            await servicio.AgregarAsync(sitio.Id, "ana", null, "Comentario inapropiado", null);
            var comentario = await ObtenerUnicoAsync("Comentario inapropiado");

            var resultado = await servicio.EliminarAsync(comentario.Id, "administrador", esAdministrador: true);

            Assert.True(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Comentario>());
        }

        [Fact]
        public async Task CambiarVisibilidad_Ocultar_OcultaElComentarioYSusRespuestas()
        {
            var (sitio, servicio) = await PrepararAsync();
            await servicio.AgregarAsync(sitio.Id, "ana", null, "Comentario a moderar", null);
            var comentario = await ObtenerUnicoAsync("Comentario a moderar");
            await servicio.AgregarAsync(sitio.Id, "beto", null, "Respuesta a moderar", comentario.Id);

            var resultado = await servicio.CambiarVisibilidadAsync(comentario.Id, oculto: true);

            Assert.True(resultado.Exito);
            using var verificacion = bd.NuevoContexto();
            Assert.All(await verificacion.Comentarios.ToListAsync(), c => Assert.True(c.Oculto));
        }

        [Fact]
        public async Task ListarPorSitio_VisitanteNoVeOcultosYAdministradorSi()
        {
            var (sitio, servicio) = await PrepararAsync();
            await servicio.AgregarAsync(sitio.Id, "ana", null, "Comentario visible", null);
            await servicio.AgregarAsync(sitio.Id, "beto", null, "Comentario oculto", null);
            var oculto = await ObtenerUnicoAsync("Comentario oculto");
            await servicio.CambiarVisibilidadAsync(oculto.Id, oculto: true);

            var paraVisitantes = await new ComentariosServicio(bd.NuevoContexto()).ListarPorSitioAsync(sitio.Id, incluirOcultos: false);
            var paraAdministrador = await new ComentariosServicio(bd.NuevoContexto()).ListarPorSitioAsync(sitio.Id, incluirOcultos: true);

            Assert.Single(paraVisitantes);
            Assert.Equal("Comentario visible", paraVisitantes[0].Texto);
            Assert.Equal(2, paraAdministrador.Count);
        }
    }
}
