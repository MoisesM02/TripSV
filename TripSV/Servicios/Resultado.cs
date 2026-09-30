namespace TripSV.Servicios
{
    public class Resultado
    {
        public bool Exito { get; init; }

        public string Mensaje { get; init; } = string.Empty;

        public static Resultado Ok(string mensaje = "") => new() { Exito = true, Mensaje = mensaje };

        public static Resultado Error(string mensaje) => new() { Exito = false, Mensaje = mensaje };

        public static Resultado Conflicto() =>
            Error("No se pudo guardar el cambio porque los datos se modificaron al mismo tiempo. Actualice la página e intente de nuevo.");
    }
}
