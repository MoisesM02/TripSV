using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Controllers
{
    [Authorize]
    public class FavoritosController : Controller
    {
        private readonly IFavoritosServicio favoritos;
        private readonly UserManager<Usuario> gestorUsuarios;

        public FavoritosController(IFavoritosServicio favoritos, UserManager<Usuario> gestorUsuarios)
        {
            this.favoritos = favoritos;
            this.gestorUsuarios = gestorUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Mis favoritos";
            return View(await favoritos.ListarAsync(gestorUsuarios.GetUserId(User) ?? string.Empty));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Alternar(int sitioId, string? urlRetorno)
        {
            var resultado = await favoritos.AlternarAsync(gestorUsuarios.GetUserId(User) ?? string.Empty, sitioId);

            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;

            if (!string.IsNullOrWhiteSpace(urlRetorno) && Url.IsLocalUrl(urlRetorno))
            {
                return LocalRedirect(urlRetorno);
            }

            return RedirectToAction("Detalle", "Sitios", new { id = sitioId });
        }
    }
}
