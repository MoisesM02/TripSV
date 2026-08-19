using Microsoft.AspNetCore.Mvc;
using TripSV.Servicios;

namespace TripSV.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ICategoriasServicio categorias;

        public CategoriasController(ICategoriasServicio categorias)
        {
            this.categorias = categorias;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Categorías";
            return View(await categorias.ListarAsync());
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Imagen(int id)
        {
            var categoria = await categorias.ObtenerAsync(id);

            if (categoria?.Imagen is null || categoria.Imagen.Length == 0)
            {
                return File("~/img/sin-imagen.svg", "image/svg+xml");
            }

            return File(categoria.Imagen, categoria.ImagenTipo ?? "image/jpeg");
        }
    }
}
