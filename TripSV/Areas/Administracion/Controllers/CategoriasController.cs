using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripSV.Modelos;
using TripSV.Servicios;

namespace TripSV.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Authorize(Roles = Roles.Administrador)]
    public class CategoriasController : Controller
    {
        private readonly ICategoriasServicio categorias;

        public CategoriasController(ICategoriasServicio categorias)
        {
            this.categorias = categorias;
        }

        public async Task<IActionResult> Index(string? buscar)
        {
            ViewData["Title"] = "Categorías";
            ViewBag.Buscar = buscar;
            return View(await categorias.BuscarAsync(buscar ?? string.Empty));
        }

        [HttpGet]
        public IActionResult Crear()
        {
            ViewData["Title"] = "Nueva categoría";
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Categoria categoria, IFormFile? imagen)
        {
            ViewData["Title"] = "Nueva categoría";

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var resultado = await categorias.CrearAsync(categoria, imagen);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(categoria);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await categorias.ObtenerAsync(id);
            if (categoria is null)
            {
                TempData["Error"] = "La categoría no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Editar categoría";
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Categoria categoria, IFormFile? imagen)
        {
            ViewData["Title"] = "Editar categoría";

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var resultado = await categorias.ActualizarAsync(categoria, imagen);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(categoria);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await categorias.EliminarAsync(id);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }
    }
}
