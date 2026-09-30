using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;

namespace TripSV.Servicios
{
    public class ItinerariosServicio : IItinerariosServicio
    {
        private readonly ContextoTripSV contexto;

        public ItinerariosServicio(ContextoTripSV contexto)
        {
            this.contexto = contexto;
        }

        public async Task<List<Itinerario>> ListarAsync(string usuarioId)
        {
            var hoy = FechaHora.Hoy;

            var itinerarios = await contexto.Itinerarios
                .Include(i => i.Visitas)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            return itinerarios
                .OrderBy(i => i.FechaFin < hoy)
                .ThenBy(i => i.FechaFin < hoy ? -i.FechaInicio.Ticks : i.FechaInicio.Ticks)
                .ToList();
        }

        public async Task<Itinerario?> ObtenerAsync(int id, string usuarioId) =>
            await contexto.Itinerarios
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId);

        public async Task<Itinerario?> ObtenerConVisitasAsync(int id, string usuarioId) =>
            await contexto.Itinerarios
                .Where(i => i.Id == id && i.UsuarioId == usuarioId)
                .Select(i => new Itinerario
                {
                    Id = i.Id,
                    UsuarioId = i.UsuarioId,
                    Nombre = i.Nombre,
                    FechaInicio = i.FechaInicio,
                    CantidadDias = i.CantidadDias,
                    Notas = i.Notas,
                    FechaCreacion = i.FechaCreacion,
                    Visitas = i.Visitas
                        .OrderBy(v => v.Dia)
                        .ThenBy(v => v.Orden)
                        .Select(v => new Visita
                        {
                            Id = v.Id,
                            ItinerarioId = v.ItinerarioId,
                            SitioId = v.SitioId,
                            Dia = v.Dia,
                            Orden = v.Orden,
                            Notas = v.Notas,
                            Sitio = new Sitio
                            {
                                Id = v.Sitio!.Id,
                                Nombre = v.Sitio.Nombre,
                                Ubicacion = v.Sitio.Ubicacion,
                                Calificacion = v.Sitio.Calificacion,
                                CategoriaId = v.Sitio.CategoriaId,
                                Categoria = new Categoria { Id = v.Sitio.Categoria!.Id, Nombre = v.Sitio.Categoria.Nombre }
                            }
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

        public async Task<Resultado> CrearAsync(Itinerario itinerario)
        {
            var validacion = Validar(itinerario);
            if (!validacion.Exito)
            {
                return validacion;
            }

            itinerario.Nombre = itinerario.Nombre.Trim();
            itinerario.Notas = Limpiar(itinerario.Notas);
            itinerario.FechaInicio = itinerario.FechaInicio.Date;
            itinerario.FechaCreacion = FechaHora.Ahora;

            contexto.Itinerarios.Add(itinerario);
            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok($"Se creó el itinerario «{itinerario.Nombre}».");
        }

        public async Task<Resultado> ActualizarAsync(Itinerario itinerario, string usuarioId)
        {
            var actual = await CargarAsync(itinerario.Id, usuarioId);
            if (actual is null)
            {
                return Resultado.Error("El itinerario no existe.");
            }

            var validacion = Validar(itinerario);
            if (!validacion.Exito)
            {
                return validacion;
            }

            var ultimoDiaOcupado = actual.Visitas.Count == 0 ? 0 : actual.Visitas.Max(v => v.Dia);
            if (itinerario.CantidadDias < ultimoDiaOcupado)
            {
                return Resultado.Error(
                    $"No se puede reducir a {itinerario.CantidadDias} día(s): hay destinos planificados en el día {ultimoDiaOcupado}. Quítelos o muévalos a otro día primero.");
            }

            actual.Nombre = itinerario.Nombre.Trim();
            actual.FechaInicio = itinerario.FechaInicio.Date;
            actual.CantidadDias = itinerario.CantidadDias;
            actual.Notas = Limpiar(itinerario.Notas);

            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok($"Se actualizó el itinerario «{actual.Nombre}».");
        }

        public async Task<Resultado> EliminarAsync(int id, string usuarioId)
        {
            var itinerario = await CargarAsync(id, usuarioId);
            if (itinerario is null)
            {
                return Resultado.Error("El itinerario no existe.");
            }

            contexto.Itinerarios.Remove(itinerario);
            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok($"Se eliminó el itinerario «{itinerario.Nombre}».");
        }

        public async Task<Resultado> AgregarVisitaAsync(int itinerarioId, string usuarioId, int sitioId, int dia, string? notas)
        {
            var itinerario = await CargarAsync(itinerarioId, usuarioId);
            if (itinerario is null)
            {
                return Resultado.Error("El itinerario no existe.");
            }

            if (dia < 1 || dia > itinerario.CantidadDias)
            {
                return Resultado.Error($"El día debe estar entre 1 y {itinerario.CantidadDias}.");
            }

            var nombreSitio = await ObtenerNombreSitioAsync(sitioId);
            if (nombreSitio is null)
            {
                return Resultado.Error("El sitio no existe.");
            }

            var delDia = itinerario.Visitas.Where(v => v.Dia == dia).ToList();

            if (delDia.Any(v => v.SitioId == sitioId))
            {
                return Resultado.Error($"{nombreSitio} ya está en el día {dia} de este itinerario.");
            }

            if (delDia.Count >= Itinerario.MaximoVisitasPorDia)
            {
                return Resultado.Error($"El día {dia} ya tiene el máximo de {Itinerario.MaximoVisitasPorDia} destinos.");
            }

            var notasLimpias = Limpiar(notas);
            if (notasLimpias?.Length > 200)
            {
                return Resultado.Error("Las notas no pueden exceder 200 caracteres.");
            }

            itinerario.Visitas.Add(new Visita
            {
                SitioId = sitioId,
                Dia = dia,
                Orden = delDia.Count == 0 ? 1 : delDia.Max(v => v.Orden) + 1,
                Notas = notasLimpias
            });

            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok($"Se agregó {nombreSitio} al día {dia} de «{itinerario.Nombre}».");
        }

        public async Task<Resultado> QuitarVisitaAsync(int visitaId, string usuarioId)
        {
            var visita = await CargarVisitaAsync(visitaId, usuarioId);
            if (visita is null)
            {
                return Resultado.Error("El destino no existe en sus itinerarios.");
            }

            var restantes = visita.Itinerario!.Visitas
                .Where(v => v.Dia == visita.Dia && v.Id != visita.Id)
                .OrderBy(v => v.Orden)
                .ToList();

            contexto.Visitas.Remove(visita);
            Renumerar(restantes);

            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            var nombreSitio = await ObtenerNombreSitioAsync(visita.SitioId);
            return Resultado.Ok($"Se quitó {nombreSitio} del día {visita.Dia}.");
        }

        public async Task<Resultado> MoverVisitaAsync(int visitaId, string usuarioId, int desplazamiento)
        {
            if (desplazamiento != -1 && desplazamiento != 1)
            {
                return Resultado.Error("Movimiento no válido.");
            }

            var visita = await CargarVisitaAsync(visitaId, usuarioId);
            if (visita is null)
            {
                return Resultado.Error("El destino no existe en sus itinerarios.");
            }

            var delDia = visita.Itinerario!.Visitas
                .Where(v => v.Dia == visita.Dia)
                .OrderBy(v => v.Orden)
                .ThenBy(v => v.Id)
                .ToList();

            var posicion = delDia.IndexOf(visita);
            var destino = posicion + desplazamiento;

            if (destino < 0 || destino >= delDia.Count)
            {
                return Resultado.Error("No se puede mover el destino en esa dirección.");
            }

            (delDia[posicion], delDia[destino]) = (delDia[destino], delDia[posicion]);
            Renumerar(delDia);

            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok();
        }

        public async Task<Resultado> CambiarDiaAsync(int visitaId, string usuarioId, int dia)
        {
            var visita = await CargarVisitaAsync(visitaId, usuarioId);
            if (visita is null)
            {
                return Resultado.Error("El destino no existe en sus itinerarios.");
            }

            if (visita.Dia == dia)
            {
                return Resultado.Ok();
            }

            var itinerario = visita.Itinerario!;

            if (dia < 1 || dia > itinerario.CantidadDias)
            {
                return Resultado.Error($"El día debe estar entre 1 y {itinerario.CantidadDias}.");
            }

            var nombreSitio = await ObtenerNombreSitioAsync(visita.SitioId);
            var nuevoDia = itinerario.Visitas.Where(v => v.Dia == dia).ToList();

            if (nuevoDia.Any(v => v.SitioId == visita.SitioId))
            {
                return Resultado.Error($"{nombreSitio} ya está en el día {dia}.");
            }

            if (nuevoDia.Count >= Itinerario.MaximoVisitasPorDia)
            {
                return Resultado.Error($"El día {dia} ya tiene el máximo de {Itinerario.MaximoVisitasPorDia} destinos.");
            }

            var diaAnterior = visita.Dia;

            visita.Dia = dia;
            visita.Orden = nuevoDia.Count == 0 ? 1 : nuevoDia.Max(v => v.Orden) + 1;

            Renumerar(itinerario.Visitas
                .Where(v => v.Dia == diaAnterior)
                .OrderBy(v => v.Orden)
                .ToList());

            if (!await contexto.GuardarSinConflictoAsync())
            {
                return Resultado.Conflicto();
            }

            return Resultado.Ok($"Se movió {nombreSitio} al día {dia}.");
        }

        private async Task<Itinerario?> CargarAsync(int id, string usuarioId) =>
            await contexto.Itinerarios
                .Include(i => i.Visitas)
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId);

        private async Task<Visita?> CargarVisitaAsync(int visitaId, string usuarioId) =>
            await contexto.Visitas
                .Include(v => v.Itinerario)
                    .ThenInclude(i => i!.Visitas)
                .FirstOrDefaultAsync(v => v.Id == visitaId && v.Itinerario!.UsuarioId == usuarioId);

        private async Task<string?> ObtenerNombreSitioAsync(int sitioId) =>
            await contexto.Sitios
                .Where(s => s.Id == sitioId)
                .Select(s => s.Nombre)
                .FirstOrDefaultAsync();

        private static Resultado Validar(Itinerario itinerario)
        {
            if (string.IsNullOrWhiteSpace(itinerario.Nombre) || itinerario.Nombre.Trim().Length < 3)
            {
                return Resultado.Error("El nombre del viaje debe tener al menos 3 caracteres.");
            }

            if (itinerario.CantidadDias < 1 || itinerario.CantidadDias > Itinerario.MaximoDias)
            {
                return Resultado.Error($"La cantidad de días debe estar entre 1 y {Itinerario.MaximoDias}.");
            }

            if (itinerario.FechaInicio == default)
            {
                return Resultado.Error("La fecha de inicio es obligatoria.");
            }

            return Resultado.Ok();
        }

        private static void Renumerar(List<Visita> visitas)
        {
            for (var i = 0; i < visitas.Count; i++)
            {
                visitas[i].Orden = i + 1;
            }
        }

        private static string? Limpiar(string? texto) =>
            string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }
}
