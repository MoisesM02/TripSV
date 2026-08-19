namespace TripSV.Servicios
{
    public static class FechaHora
    {
        private static readonly TimeZoneInfo ZonaSalvador = ObtenerZona();

        public static DateTime Ahora => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ZonaSalvador).DateTime;

        public static string Formatear(DateTime fecha) =>
            fecha.ToString("dd 'de' MMMM 'de' yyyy hh:mm:ss tt", new System.Globalization.CultureInfo("es-SV"));

        private static TimeZoneInfo ObtenerZona()
        {
            foreach (var identificador in new[] { "America/El_Salvador", "Central America Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(identificador);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }
    }
}
