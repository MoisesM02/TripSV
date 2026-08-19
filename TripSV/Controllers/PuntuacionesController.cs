using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Controllers
{
    [Authorize]
    public class PuntuacionesController : Controller
    {
        private readonly IPuntuacionesServicio puntuaciones;
        private readonly UserManager<Usuario> gestorUsuarios;

        public PuntuacionesController(IPuntuacionesServicio puntuaciones, UserManager<Usuario> gestorUsuarios)
        {
            this.puntuaciones = puntuaciones;
            this.gestorUsuarios = gestorUsuarios;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calificar(int sitioId, int valor)
        {
            var usuario = await gestorUsuarios.GetUserAsync(User);

            var resultado = await puntuaciones.CalificarAsync(
                sitioId,
                usuario?.UserName ?? User.Identity!.Name!,
                usuario?.Id,
                valor);

            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;

            return RedirectToAction("Detalle", "Sitios", new { id = sitioId });
        }
    }
}
