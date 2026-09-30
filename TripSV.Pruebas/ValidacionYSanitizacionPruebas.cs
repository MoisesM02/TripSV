using TripSV.Servicios;

namespace TripSV.Pruebas
{
    public class ValidacionYSanitizacionPruebas
    {
        private readonly SanitizadorHtml sanitizador = new();

        [Fact]
        public void ValidarImagen_SinArchivo_Rechaza()
        {
            Assert.False(ValidadorImagen.Validar(null).Exito);
        }

        [Theory]
        [InlineData("image/png")]
        [InlineData("image/jpeg")]
        [InlineData("image/jpg")]
        public void ValidarImagen_FormatoPermitido_Acepta(string tipo)
        {
            Assert.True(ValidadorImagen.Validar(ArchivosPrueba.Imagen(tipo)).Exito);
        }

        [Theory]
        [InlineData("image/gif")]
        [InlineData("text/plain")]
        [InlineData("application/x-msdownload")]
        public void ValidarImagen_FormatoNoPermitido_Rechaza(string tipo)
        {
            Assert.False(ValidadorImagen.Validar(ArchivosPrueba.Imagen(tipo)).Exito);
        }

        [Fact]
        public void ValidarImagen_MayorACincoMegas_Rechaza()
        {
            var archivo = ArchivosPrueba.Imagen("image/png", ValidadorImagen.TamanoMaximo + 1);

            Assert.False(ValidadorImagen.Validar(archivo).Exito);
        }

        [Fact]
        public void Sanitizar_EliminaEtiquetasScript()
        {
            var limpio = sanitizador.Limpiar("<p>Hola</p><script>document.cookie</script>");

            Assert.Equal("<p>Hola</p>", limpio);
        }

        [Fact]
        public void Sanitizar_EliminaManejadoresDeEventos()
        {
            var limpio = sanitizador.Limpiar("<img src=\"https://ejemplo.com/a.png\" onerror=\"alert(1)\">");

            Assert.DoesNotContain("onerror", limpio);
        }

        [Fact]
        public void Sanitizar_EliminaEnlacesJavascript()
        {
            var limpio = sanitizador.Limpiar("<a href=\"javascript:alert(1)\">clic</a>");

            Assert.DoesNotContain("javascript:", limpio);
        }

        [Fact]
        public void Sanitizar_ConservaElFormatoDelEditor()
        {
            var html = "<p><strong>Horario:</strong> <em>8 a 5</em></p><ul><li>Entrada</li></ul><a href=\"https://www.mitur.gob.sv\">MITUR</a>";

            var limpio = sanitizador.Limpiar(html);

            Assert.Contains("<strong>Horario:</strong>", limpio);
            Assert.Contains("<li>Entrada</li>", limpio);
            Assert.Contains("href=\"https://www.mitur.gob.sv\"", limpio);
        }

        [Fact]
        public void Sanitizar_ConservaMapasDeGoogle()
        {
            var html = "<iframe src=\"https://www.google.com/maps/embed?pb=123\" width=\"600\" height=\"450\" allowfullscreen=\"\"></iframe>";

            var limpio = sanitizador.Limpiar(html);

            Assert.Contains("<iframe", limpio);
            Assert.Contains("https://www.google.com/maps/embed?pb=123", limpio);
        }

        [Fact]
        public void Sanitizar_EliminaIframesDeOrigenesNoPermitidos()
        {
            var limpio = sanitizador.Limpiar("<p>Texto</p><iframe src=\"https://sitio-malicioso.com/robar\"></iframe>");

            Assert.DoesNotContain("<iframe", limpio);
            Assert.Contains("<p>Texto</p>", limpio);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Sanitizar_ContenidoVacio_LoDevuelveSinCambios(string? html)
        {
            Assert.Equal(html, sanitizador.Limpiar(html));
        }
    }
}
