using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Pruebas
{
    public class CategoriasYSitiosServicioPruebas : IDisposable
    {
        private readonly BaseDatosPrueba bd = new();

        public void Dispose() => bd.Dispose();

        private SitiosServicio NuevoServicioSitios() => new(bd.Contexto, new SanitizadorHtml());

        [Fact]
        public async Task CrearCategoria_NombreDuplicado_Rechaza()
        {
            var servicio = new CategoriasServicio(bd.Contexto);
            await servicio.CrearAsync(new Categoria { Nombre = "playa" }, null);

            var resultado = await servicio.CrearAsync(new Categoria { Nombre = "  playa  " }, null);

            Assert.False(resultado.Exito);
            Assert.Equal(1, await bd.ContarAsync<Categoria>());
        }

        [Fact]
        public async Task EliminarCategoria_ConSitiosAsociados_Rechaza()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");
            await bd.CrearSitioAsync("El Tunco", categoria);
            var servicio = new CategoriasServicio(bd.Contexto);

            var resultado = await servicio.EliminarAsync(categoria.Id);

            Assert.False(resultado.Exito);
            Assert.Equal(1, await bd.ContarAsync<Categoria>());
        }

        [Fact]
        public async Task EliminarCategoria_SinSitios_LaElimina()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");
            var servicio = new CategoriasServicio(bd.Contexto);

            var resultado = await servicio.EliminarAsync(categoria.Id);

            Assert.True(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Categoria>());
        }

        [Fact]
        public async Task ListarCategorias_IncluyeElConteoDeSitiosSinCargarImagenes()
        {
            var playa = await bd.CrearCategoriaAsync("playa");
            await bd.CrearCategoriaAsync("montañas");
            await bd.CrearSitioAsync("El Tunco", playa);
            await bd.CrearSitioAsync("El Zonte", playa);

            var categorias = await new CategoriasServicio(bd.NuevoContexto()).ListarAsync();

            Assert.Equal(2, categorias.Single(c => c.Nombre == "playa").Sitios.Count);
            Assert.Empty(categorias.Single(c => c.Nombre == "montañas").Sitios);
            Assert.All(categorias, c => Assert.Null(c.Imagen));
        }

        [Fact]
        public async Task CrearSitio_SinImagen_Rechaza()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");

            var resultado = await NuevoServicioSitios().CrearAsync(NuevoSitio(categoria.Id), null);

            Assert.False(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Sitio>());
        }

        [Fact]
        public async Task CrearSitio_CategoriaInexistente_Rechaza()
        {
            var resultado = await NuevoServicioSitios().CrearAsync(NuevoSitio(999), ArchivosPrueba.Imagen("image/png"));

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task CrearSitio_InformacionConScript_SeGuardaSaneada()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");
            var sitio = NuevoSitio(categoria.Id);
            sitio.Informacion = "<p>Horario de 8 a 5</p><script>alert('xss')</script>";

            var resultado = await NuevoServicioSitios().CrearAsync(sitio, ArchivosPrueba.Imagen("image/jpeg"));

            Assert.True(resultado.Exito);
            using var verificacion = bd.NuevoContexto();
            var guardado = await verificacion.Sitios.SingleAsync();
            Assert.Contains("Horario de 8 a 5", guardado.Informacion);
            Assert.DoesNotContain("<script", guardado.Informacion);
        }

        [Fact]
        public async Task CrearSitio_NombreDuplicado_Rechaza()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");
            await bd.CrearSitioAsync("El Tunco", categoria);

            var resultado = await NuevoServicioSitios().CrearAsync(NuevoSitio(categoria.Id), ArchivosPrueba.Imagen("image/png"));

            Assert.False(resultado.Exito);
            Assert.Equal(1, await bd.ContarAsync<Sitio>());
        }

        [Fact]
        public async Task EliminarSitios_SinSeleccion_Rechaza()
        {
            var resultado = await NuevoServicioSitios().EliminarVariosAsync(Array.Empty<int>());

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task EliminarSitios_EliminaEnCascadaComentariosYFavoritos()
        {
            var sitio = await bd.CrearSitioAsync("El Tunco", await bd.CrearCategoriaAsync("playa"));
            var usuario = await bd.CrearUsuarioAsync("visitante");
            await new ComentariosServicio(bd.Contexto).AgregarAsync(sitio.Id, "visitante", usuario.Id, "Muy bonito", null);
            await new FavoritosServicio(bd.Contexto).AlternarAsync(usuario.Id, sitio.Id);

            var resultado = await new SitiosServicio(bd.NuevoContexto(), new SanitizadorHtml()).EliminarVariosAsync(new[] { sitio.Id });

            Assert.True(resultado.Exito);
            Assert.Equal(0, await bd.ContarAsync<Sitio>());
            Assert.Equal(0, await bd.ContarAsync<Comentario>());
            Assert.Equal(0, await bd.ContarAsync<Favorito>());
        }

        [Fact]
        public async Task ListarUbicaciones_UnificaVariantesDeMayusculas()
        {
            var categoria = await bd.CrearCategoriaAsync("playa");
            await bd.CrearSitioAsync("El Tunco", categoria, "La Libertad");
            await bd.CrearSitioAsync("El Zonte", categoria, "La libertad");
            await bd.CrearSitioAsync("Costa del Sol", categoria, "La Paz");

            var ubicaciones = await NuevoServicioSitios().ListarUbicacionesAsync();

            Assert.Equal(new[] { "La Libertad", "La Paz" }, ubicaciones);
        }

        private static Sitio NuevoSitio(int categoriaId) => new()
        {
            Nombre = "El Tunco",
            Descripcion = "Playa para surfear",
            Ubicacion = "La Libertad",
            CategoriaId = categoriaId
        };
    }

    internal static class ArchivosPrueba
    {
        public static IFormFile Imagen(string tipo, long tamano = 1024)
        {
            var contenido = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
            return new FormFile(contenido, 0, tamano, "imagen", "foto")
            {
                Headers = new HeaderDictionary(),
                ContentType = tipo
            };
        }
    }
}
