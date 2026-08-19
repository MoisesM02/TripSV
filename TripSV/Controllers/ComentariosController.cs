using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Controllers
{
    [Authorize]
    public class ComentariosController : Controller
    {
        private readonly IComentariosServicio comentarios;
        private readonly UserManager<Usuario> gestorUsuarios;

        public ComentariosController(IComentariosServicio comentarios, UserManager<Usuario> gestorUsuarios)
        {
            this.comentarios = comentarios;
            this.gestorUsuarios = gestorUsuarios;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int sitioId, string comentario, int? respuestaAId)
        {
            if (string.IsNullOrWhiteSpace(comentario))
            {
                TempData["Error"] = "El comentario no puede quedar vacío.";
                return RedirectToAction("Detalle", "Sitios", new { id = sitioId });
            }

            var usuario = await gestorUsuarios.GetUserAsync(User);

            var resultado = await comentarios.AgregarAsync(
                sitioId,
                usuario?.UserName ?? User.Identity!.Name!,
                usuario?.Id,
                comentario,
                respuestaAId);

            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;

            return RedirectToAction("Detalle", "Sitios", new { id = sitioId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, int sitioId)
        {
            var resultado = await comentarios.EliminarAsync(
                id,
                User.Identity?.Name ?? string.Empty,
                User.IsInRole(Roles.Administrador));

            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;

            return RedirectToAction("Detalle", "Sitios", new { id = sitioId });
        }
    }
}
