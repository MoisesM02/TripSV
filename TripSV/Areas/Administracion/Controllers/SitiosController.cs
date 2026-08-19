using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TripSV.Modelos;
using TripSV.Servicios;
using TripSV.ViewModels;

namespace TripSV.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Authorize(Roles = Roles.Administrador)]
    public class SitiosController : Controller
    {
        private readonly ISitiosServicio sitios;
        private readonly ICategoriasServicio categorias;

        public SitiosController(ISitiosServicio sitios, ICategoriasServicio categorias)
        {
            this.sitios = sitios;
            this.categorias = categorias;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Modificar sitios";
            return View(await sitios.ListarAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Agregar()
        {
            ViewData["Title"] = "Agregar sitio";
            return View(new SitioFormularioViewModel { Categorias = await ListaCategoriasAsync() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Agregar(SitioFormularioViewModel modelo)
        {
            ViewData["Title"] = "Agregar sitio";

            if (!ModelState.IsValid)
            {
                modelo.Categorias = await ListaCategoriasAsync();
                return View(modelo);
            }

            var sitio = new Sitio
            {
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                Ubicacion = modelo.Ubicacion,
                CategoriaId = modelo.CategoriaId,
                Informacion = modelo.Informacion
            };

            var resultado = await sitios.CrearAsync(sitio, modelo.Imagen);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                modelo.Categorias = await ListaCategoriasAsync();
                return View(modelo);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var sitio = await sitios.ObtenerAsync(id);
            if (sitio is null)
            {
                TempData["Error"] = "El sitio no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Modificar sitio";

            return View(new SitioFormularioViewModel
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Descripcion = sitio.Descripcion,
                Ubicacion = sitio.Ubicacion,
                CategoriaId = sitio.CategoriaId,
                Informacion = sitio.Informacion,
                TieneImagen = sitio.Imagen is not null && sitio.Imagen.Length > 0,
                Categorias = await ListaCategoriasAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Modificar(SitioFormularioViewModel modelo)
        {
            ViewData["Title"] = "Modificar sitio";

            if (!ModelState.IsValid)
            {
                modelo.Categorias = await ListaCategoriasAsync();
                return View(modelo);
            }

            var sitio = new Sitio
            {
                Id = modelo.Id,
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                Ubicacion = modelo.Ubicacion,
                CategoriaId = modelo.CategoriaId,
                Informacion = modelo.Informacion
            };

            var resultado = await sitios.ActualizarAsync(sitio, modelo.Imagen);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                modelo.Categorias = await ListaCategoriasAsync();
                return View(modelo);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int[] eliminar)
        {
            var resultado = await sitios.EliminarVariosAsync(eliminar);
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> ListaCategoriasAsync() =>
            (await categorias.ListarAsync())
                .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
                .ToList();
    }
}
