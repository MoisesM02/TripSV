using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TripSV.Servicios;
using TripSV.ViewModels;

namespace TripSV.Controllers
{
    public class InicioController : Controller
    {
        private readonly ICategoriasServicio categorias;
        private readonly ISitiosServicio sitios;

        public InicioController(ICategoriasServicio categorias, ISitiosServicio sitios)
        {
            this.categorias = categorias;
            this.sitios = sitios;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Explora El Salvador";
            ViewBag.Destacados = await sitios.ListarDestacadosAsync(6);
            return View(await categorias.ListarAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
