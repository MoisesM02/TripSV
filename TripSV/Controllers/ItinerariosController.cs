using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;
using TripSV.ViewModels;

namespace TripSV.Controllers
{
    [Authorize]
    public class ItinerariosController : Controller
    {
        private readonly IItinerariosServicio itinerarios;
        private readonly ISitiosServicio sitios;
        private readonly IFavoritosServicio favoritos;
        private readonly UserManager<Usuario> gestorUsuarios;

        public ItinerariosController(
            IItinerariosServicio itinerarios,
            ISitiosServicio sitios,
            IFavoritosServicio favoritos,
            UserManager<Usuario> gestorUsuarios)
        {
            this.itinerarios = itinerarios;
            this.sitios = sitios;
            this.favoritos = favoritos;
            this.gestorUsuarios = gestorUsuarios;
        }

        private string UsuarioId => gestorUsuarios.GetUserId(User) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Mis itinerarios";
            return View(await itinerarios.ListarAsync(UsuarioId));
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? sitioId)
        {
            ViewData["Title"] = "Nuevo itinerario";

            var modelo = new ItinerarioFormularioViewModel
            {
                FechaInicio = FechaHora.Hoy.AddDays(7),
                CantidadDias = 2
            };

            if (sitioId is not null)
            {
                var sitio = await sitios.ObtenerAsync(sitioId.Value);
                if (sitio is not null)
                {
                    modelo.SitioId = sitio.Id;
                    modelo.NombreSitio = sitio.Nombre;
                }
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ItinerarioFormularioViewModel modelo)
        {
            ViewData["Title"] = "Nuevo itinerario";

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var itinerario = new Itinerario
            {
                UsuarioId = UsuarioId,
                Nombre = modelo.Nombre,
                FechaInicio = modelo.FechaInicio,
                CantidadDias = modelo.CantidadDias,
                Notas = modelo.Notas
            };

            var resultado = await itinerarios.CrearAsync(itinerario);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(modelo);
            }

            var mensaje = resultado.Mensaje;

            if (modelo.SitioId is not null)
            {
                var agregado = await itinerarios.AgregarVisitaAsync(itinerario.Id, UsuarioId, modelo.SitioId.Value, 1, null);
                if (agregado.Exito)
                {
                    mensaje = $"{mensaje} {agregado.Mensaje}";
                }
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction(nameof(Detalle), new { id = itinerario.Id });
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var itinerario = await itinerarios.ObtenerConVisitasAsync(id, UsuarioId);
            if (itinerario is null)
            {
                TempData["Error"] = "El itinerario no existe.";
                return RedirectToAction(nameof(Index));
            }

            var idsFavoritos = (await favoritos.ObtenerIdsSitiosAsync(UsuarioId)).ToHashSet();
            var todos = await sitios.ListarResumenAsync();

            ViewData["Title"] = itinerario.Nombre;

            return View(new ItinerarioDetalleViewModel
            {
                Itinerario = itinerario,
                Dias = Enumerable.Range(1, itinerario.CantidadDias)
                    .Select(numero => new DiaItinerarioViewModel
                    {
                        Numero = numero,
                        Fecha = itinerario.FechaInicio.AddDays(numero - 1),
                        Visitas = itinerario.Visitas.Where(v => v.Dia == numero).OrderBy(v => v.Orden).ToList()
                    })
                    .ToList(),
                SitiosFavoritos = todos.Where(s => idsFavoritos.Contains(s.Id)).ToList(),
                OtrosSitios = todos.Where(s => !idsFavoritos.Contains(s.Id)).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var itinerario = await itinerarios.ObtenerAsync(id, UsuarioId);
            if (itinerario is null)
            {
                TempData["Error"] = "El itinerario no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Editar itinerario";

            return View(new ItinerarioFormularioViewModel
            {
                Id = itinerario.Id,
                Nombre = itinerario.Nombre,
                FechaInicio = itinerario.FechaInicio,
                CantidadDias = itinerario.CantidadDias,
                Notas = itinerario.Notas
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ItinerarioFormularioViewModel modelo)
        {
            ViewData["Title"] = "Editar itinerario";

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = await itinerarios.ActualizarAsync(new Itinerario
            {
                Id = modelo.Id,
                Nombre = modelo.Nombre,
                FechaInicio = modelo.FechaInicio,
                CantidadDias = modelo.CantidadDias,
                Notas = modelo.Notas
            }, UsuarioId);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(modelo);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalle), new { id = modelo.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await itinerarios.EliminarAsync(id, UsuarioId);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarVisita(int itinerarioId, int sitioId, int dia, string? notas, string? urlRetorno)
        {
            var resultado = await itinerarios.AgregarVisitaAsync(itinerarioId, UsuarioId, sitioId, dia, notas);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;

            if (!string.IsNullOrWhiteSpace(urlRetorno) && Url.IsLocalUrl(urlRetorno))
            {
                return LocalRedirect(urlRetorno);
            }

            return RedirectToAction(nameof(Detalle), null, new { id = itinerarioId }, $"dia-{dia}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarVisita(int visitaId, int itinerarioId, int dia)
        {
            var resultado = await itinerarios.QuitarVisitaAsync(visitaId, UsuarioId);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalle), null, new { id = itinerarioId }, $"dia-{dia}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoverVisita(int visitaId, int itinerarioId, int dia, int desplazamiento)
        {
            var resultado = await itinerarios.MoverVisitaAsync(visitaId, UsuarioId, desplazamiento);
            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Detalle), null, new { id = itinerarioId }, $"dia-{dia}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarDia(int visitaId, int itinerarioId, int dia)
        {
            var resultado = await itinerarios.CambiarDiaAsync(visitaId, UsuarioId, dia);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalle), null, new { id = itinerarioId }, $"dia-{dia}");
        }
    }
}
