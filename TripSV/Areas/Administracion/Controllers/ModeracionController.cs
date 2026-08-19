using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;
using TripSV.ViewModels;

namespace TripSV.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Authorize(Roles = Roles.Administrador)]
    public class ModeracionController : Controller
    {
        private readonly IComentariosServicio comentarios;

        public ModeracionController(IComentariosServicio comentarios)
        {
            this.comentarios = comentarios;
        }

        public async Task<IActionResult> Index(string? filtro, bool soloOcultos)
        {
            ViewData["Title"] = "Moderación de comentarios";

            var todos = await comentarios.ListarParaModeracionAsync(filtro, soloOcultos);
            var completos = await comentarios.ListarParaModeracionAsync(null, false);

            return View(new ModeracionViewModel
            {
                Comentarios = todos,
                Filtro = filtro,
                SoloOcultos = soloOcultos,
                TotalVisibles = completos.Count(c => !c.Oculto),
                TotalOcultos = completos.Count(c => c.Oculto)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ocultar(int id, string? filtro, bool soloOcultos)
        {
            var resultado = await comentarios.CambiarVisibilidadAsync(id, true);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index), new { filtro, soloOcultos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mostrar(int id, string? filtro, bool soloOcultos)
        {
            var resultado = await comentarios.CambiarVisibilidadAsync(id, false);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index), new { filtro, soloOcultos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, string? filtro, bool soloOcultos)
        {
            var resultado = await comentarios.EliminarAsync(id, User.Identity?.Name ?? string.Empty, true);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index), new { filtro, soloOcultos });
        }
    }
}
