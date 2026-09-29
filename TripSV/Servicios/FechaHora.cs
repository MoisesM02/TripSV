namespace TripSV.Servicios
{
    public static class FechaHora
    {
        private static readonly TimeZoneInfo ZonaSalvador = ObtenerZona();

        private static readonly System.Globalization.CultureInfo Cultura = new("es-SV");

        public static DateTime Ahora => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ZonaSalvador).DateTime;

        public static DateTime Hoy => Ahora.Date;

        public static string Formatear(DateTime fecha) =>
            fecha.ToString("dd 'de' MMMM 'de' yyyy hh:mm:ss tt", Cultura);

        public static string FormatearDia(DateTime fecha) =>
            fecha.ToString("dddd d 'de' MMMM", Cultura);

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
