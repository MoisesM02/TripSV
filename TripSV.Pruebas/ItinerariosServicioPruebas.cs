using Microsoft.EntityFrameworkCore;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Pruebas
{
    public class ItinerariosServicioPruebas : IDisposable
    {
        private readonly BaseDatosPrueba bd = new();

        public void Dispose() => bd.Dispose();

        private async Task<(Usuario usuario, Itinerario itinerario, List<Sitio> sitios, ItinerariosServicio servicio)> PrepararAsync(int cantidadDias = 3, int cantidadSitios = 3)
        {
            var usuario = await bd.CrearUsuarioAsync("viajero");
            var categoria = await bd.CrearCategoriaAsync("playa");
            var sitios = new List<Sitio>();

            for (var i = 1; i <= cantidadSitios; i++)
            {
                sitios.Add(await bd.CrearSitioAsync($"Sitio {i}", categoria));
            }

            var servicio = new ItinerariosServicio(bd.Contexto);
            var itinerario = new Itinerario
            {
                UsuarioId = usuario.Id,
                Nombre = "Ruta de prueba",
                FechaInicio = new DateTime(2026, 10, 5),
                CantidadDias = cantidadDias
            };
            await servicio.CrearAsync(itinerario);

            return (usuario, itinerario, sitios, servicio);
        }

        private async Task<List<string>> OrdenDelDiaAsync(int itinerarioId, int dia)
        {
            using var verificacion = bd.NuevoContexto();
            return await verificacion.Visitas
                .Where(v => v.ItinerarioId == itinerarioId && v.Dia == dia)
                .OrderBy(v => v.Orden)
                .Select(v => v.Sitio!.Nombre + ":" + v.Orden)
                .ToListAsync();
        }

        private async Task<int> IdVisitaAsync(int itinerarioId, int sitioId)
        {
            using var verificacion = bd.NuevoContexto();
            return await verificacion.Visitas
                .Where(v => v.ItinerarioId == itinerarioId && v.SitioId == sitioId)
                .Select(v => v.Id)
                .SingleAsync();
        }

        [Fact]
        public async Task Crear_DatosValidos_GuardaElItinerario()
        {
            var (_, itinerario, _, _) = await PrepararAsync();

            Assert.True(itinerario.Id > 0);
            Assert.Equal(new DateTime(2026, 10, 7), itinerario.FechaFin);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Itinerario.MaximoDias + 1)]
        public async Task Crear_CantidadDeDiasFueraDeRango_Rechaza(int cantidadDias)
        {
            var usuario = await bd.CrearUsuarioAsync("viajero");
            var servicio = new ItinerariosServicio(bd.Contexto);

            var resultado = await servicio.CrearAsync(new Itinerario
            {
                UsuarioId = usuario.Id,
                Nombre = "Viaje inválido",
                FechaInicio = new DateTime(2026, 10, 5),
                CantidadDias = cantidadDias
            });

            Assert.False(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Itinerario>());
        }

        [Fact]
        public async Task AgregarVisita_AsignaOrdenConsecutivoDentroDelDia()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();

            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[1].Id, 1, "Almuerzo");

            Assert.Equal(new[] { "Sitio 1:1", "Sitio 2:2" }, await OrdenDelDiaAsync(itinerario.Id, 1));
        }

        [Fact]
        public async Task AgregarVisita_MismoSitioEnElMismoDia_Rechaza()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);

            var resultado = await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);

            Assert.False(resultado.Exito);
            Assert.Equal(1, await bd.ContarAsync<Visita>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public async Task AgregarVisita_DiaFueraDelViaje_Rechaza(int dia)
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync(cantidadDias: 3);

            var resultado = await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, dia, null);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task AgregarVisita_DiaCompleto_Rechaza()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync(cantidadSitios: Itinerario.MaximoVisitasPorDia + 1);

            foreach (var sitio in sitios.Take(Itinerario.MaximoVisitasPorDia))
            {
                await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitio.Id, 1, null);
            }

            var resultado = await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios.Last().Id, 1, null);

            Assert.False(resultado.Exito);
            Assert.Equal(Itinerario.MaximoVisitasPorDia, await bd.ContarAsync<Visita>());
        }

        [Fact]
        public async Task AgregarVisita_ItinerarioDeOtroUsuario_Rechaza()
        {
            var (_, itinerario, sitios, servicio) = await PrepararAsync();
            var intruso = await bd.CrearUsuarioAsync("intruso");

            var resultado = await servicio.AgregarVisitaAsync(itinerario.Id, intruso.Id, sitios[0].Id, 1, null);

            Assert.False(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Visita>());
        }

        [Fact]
        public async Task MoverVisita_HaciaArriba_IntercambiaElOrden()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[1].Id, 1, null);

            var resultado = await servicio.MoverVisitaAsync(await IdVisitaAsync(itinerario.Id, sitios[1].Id), usuario.Id, -1);

            Assert.True(resultado.Exito);
            Assert.Equal(new[] { "Sitio 2:1", "Sitio 1:2" }, await OrdenDelDiaAsync(itinerario.Id, 1));
        }

        [Fact]
        public async Task MoverVisita_PrimeraHaciaArriba_Rechaza()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);

            var resultado = await servicio.MoverVisitaAsync(await IdVisitaAsync(itinerario.Id, sitios[0].Id), usuario.Id, -1);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task CambiarDia_MueveLaVisitaYRenumeraAmbosDias()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[1].Id, 1, null);
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[2].Id, 2, null);

            var resultado = await servicio.CambiarDiaAsync(await IdVisitaAsync(itinerario.Id, sitios[0].Id), usuario.Id, 2);

            Assert.True(resultado.Exito);
            Assert.Equal(new[] { "Sitio 2:1" }, await OrdenDelDiaAsync(itinerario.Id, 1));
            Assert.Equal(new[] { "Sitio 3:1", "Sitio 1:2" }, await OrdenDelDiaAsync(itinerario.Id, 2));
        }

        [Fact]
        public async Task QuitarVisita_RenumeraLasRestantes()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();
            foreach (var sitio in sitios)
            {
                await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitio.Id, 1, null);
            }

            var resultado = await servicio.QuitarVisitaAsync(await IdVisitaAsync(itinerario.Id, sitios[0].Id), usuario.Id);

            Assert.True(resultado.Exito);
            Assert.Equal(new[] { "Sitio 2:1", "Sitio 3:2" }, await OrdenDelDiaAsync(itinerario.Id, 1));
        }

        [Fact]
        public async Task Actualizar_ReducirDiasDejandoVisitasFuera_Rechaza()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync(cantidadDias: 3);
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 3, null);

            var resultado = await servicio.ActualizarAsync(new Itinerario
            {
                Id = itinerario.Id,
                Nombre = itinerario.Nombre,
                FechaInicio = itinerario.FechaInicio,
                CantidadDias = 2
            }, usuario.Id);

            Assert.False(resultado.Exito);
            Assert.Contains("día 3", resultado.Mensaje);
        }

        [Fact]
        public async Task Eliminar_ItinerarioDeOtroUsuario_Rechaza()
        {
            var (_, itinerario, _, servicio) = await PrepararAsync();
            var intruso = await bd.CrearUsuarioAsync("intruso");

            var resultado = await servicio.EliminarAsync(itinerario.Id, intruso.Id);

            Assert.False(resultado.Exito);
            Assert.Equal(1, await bd.ContarAsync<Itinerario>());
        }

        [Fact]
        public async Task Obtener_ItinerarioDeOtroUsuario_DevuelveNulo()
        {
            var (_, itinerario, _, servicio) = await PrepararAsync();
            var intruso = await bd.CrearUsuarioAsync("intruso");

            Assert.Null(await servicio.ObtenerConVisitasAsync(itinerario.Id, intruso.Id));
            Assert.Empty(await servicio.ListarAsync(intruso.Id));
        }

        [Fact]
        public async Task Eliminar_ItinerarioPropio_EliminaTambienSusVisitas()
        {
            var (usuario, itinerario, sitios, servicio) = await PrepararAsync();
            await servicio.AgregarVisitaAsync(itinerario.Id, usuario.Id, sitios[0].Id, 1, null);

            var resultado = await servicio.EliminarAsync(itinerario.Id, usuario.Id);

            Assert.True(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Itinerario>());
            Assert.Equal(0, await bd.ContarAsync<Visita>());
        }
    }
}
