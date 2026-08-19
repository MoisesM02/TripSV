namespace TripSV.Servicios
{
    public static class ValidadorImagen
    {
        public const long TamanoMaximo = 5 * 1024 * 1024;

        private static readonly string[] TiposPermitidos = ["image/jpg", "image/jpeg", "image/png"];

        public static Resultado Validar(IFormFile? archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                return Resultado.Error("No se recibió ninguna imagen.");
            }

            if (archivo.Length > TamanoMaximo)
            {
                return Resultado.Error("El tamaño de la imagen supera los 5 MB.");
            }

            if (!TiposPermitidos.Contains(archivo.ContentType))
            {
                return Resultado.Error($"Use una imagen .jpg, .jpeg o .png (su formato es {archivo.ContentType}).");
            }

            return Resultado.Ok();
        }

        public static async Task<byte[]> LeerAsync(IFormFile archivo)
        {
            using var memoria = new MemoryStream();
            await archivo.CopyToAsync(memoria);
            return memoria.ToArray();
        }
    }
}
