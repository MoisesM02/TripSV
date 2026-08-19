using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;
using TripSV.ViewModels;

namespace TripSV.Controllers
{
    public class SitiosController : Controller
    {
        private readonly ISitiosServicio sitios;
        private readonly ICategoriasServicio categorias;
        private readonly IComentariosServicio comentarios;
        private readonly IPuntuacionesServicio puntuaciones;

        public SitiosController(
            ISitiosServicio sitios,
            ICategoriasServicio categorias,
            IComentariosServicio comentarios,
            IPuntuacionesServicio puntuaciones)
        {
            this.sitios = sitios;
            this.categorias = categorias;
            this.comentarios = comentarios;
            this.puntuaciones = puntuaciones;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Todos los sitios";
            return View(await sitios.ListarAsync());
        }

        public async Task<IActionResult> MostrarSitios(int? id, string? cat)
        {
            Categoria? categoria = null;

            if (id is not null)
            {
                categoria = await categorias.ObtenerAsync(id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(cat))
            {
                categoria = await categorias.ObtenerPorNombreAsync(cat);
            }

            if (categoria is null)
            {
                TempData["Error"] = "La categoría solicitada no existe.";
                return RedirectToAction(nameof(Index), "Categorias");
            }

            ViewData["Title"] = categoria.Nombre;
            ViewBag.Categoria = categoria;

            return View(await sitios.ListarPorCategoriaAsync(categoria.Id));
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var sitio = await sitios.ObtenerAsync(id);
            if (sitio is null)
            {
                TempData["Error"] = "Este sitio no está en nuestro sistema.";
                return RedirectToAction(nameof(Index), "Categorias");
            }

            var esAdministrador = User.IsInRole(Roles.Administrador);
            var nombreUsuario = User.Identity?.Name ?? string.Empty;

            ViewData["Title"] = sitio.Nombre;

            return View(new SitioDetalleViewModel
            {
                Sitio = sitio,
                Comentarios = await comentarios.ListarPorSitioAsync(id, esAdministrador),
                MiPuntuacion = await puntuaciones.ObtenerDeUsuarioAsync(id, nombreUsuario),
                PuedeParticipar = User.Identity?.IsAuthenticated ?? false,
                EsAdministrador = esAdministrador,
                NombreUsuario = nombreUsuario
            });
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Imagen(int id)
        {
            var sitio = await sitios.ObtenerAsync(id);

            if (sitio?.Imagen is null || sitio.Imagen.Length == 0)
            {
                return File("~/img/sin-imagen.svg", "image/svg+xml");
            }

            return File(sitio.Imagen, sitio.ImagenTipo ?? "image/jpeg");
        }
    }
}
