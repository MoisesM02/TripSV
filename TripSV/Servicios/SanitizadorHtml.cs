using Ganss.Xss;

namespace TripSV.Servicios
{
    public class SanitizadorHtml : ISanitizadorHtml
    {
        private static readonly string[] OrigenesPermitidos =
        [
            "https://www.google.com/maps/embed",
            "https://maps.google.com/maps",
            "https://www.youtube.com/embed/",
            "https://www.youtube-nocookie.com/embed/"
        ];

        private static readonly string[] AtributosAdicionales =
        [
            "allowfullscreen", "frameborder", "loading", "referrerpolicy", "target", "aria-hidden", "tabindex"
        ];

        private readonly HtmlSanitizer sanitizador;

        public SanitizadorHtml()
        {
            sanitizador = new HtmlSanitizer();
            sanitizador.AllowedTags.Add("iframe");

            foreach (var atributo in AtributosAdicionales)
            {
                sanitizador.AllowedAttributes.Add(atributo);
            }

            sanitizador.FilterUrl += (_, evento) =>
            {
                if (EsIframe(evento.Tag.TagName) && !EsOrigenPermitido(evento.OriginalUrl))
                {
                    evento.SanitizedUrl = null;
                }
            };

            sanitizador.PostProcessDom += (_, evento) =>
            {
                foreach (var iframe in evento.Document.QuerySelectorAll("iframe").ToList())
                {
                    if (!EsOrigenPermitido(iframe.GetAttribute("src")))
                    {
                        iframe.Remove();
                    }
                }
            };
        }

        public string? Limpiar(string? html) =>
            string.IsNullOrWhiteSpace(html) ? html : sanitizador.Sanitize(html);

        private static bool EsIframe(string nombreEtiqueta) =>
            string.Equals(nombreEtiqueta, "iframe", StringComparison.OrdinalIgnoreCase);

        private static bool EsOrigenPermitido(string? url) =>
            !string.IsNullOrWhiteSpace(url) &&
            OrigenesPermitidos.Any(origen => url.StartsWith(origen, StringComparison.OrdinalIgnoreCase));
    }
}
