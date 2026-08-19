using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using TripSV.Modelos;
using TripSV.Servicios;
using TripSV.ViewModels;

namespace TripSV.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<Usuario> gestorUsuarios;
        private readonly SignInManager<Usuario> gestorSesion;
        private readonly ILogger<CuentaController> registro;

        public CuentaController(
            UserManager<Usuario> gestorUsuarios,
            SignInManager<Usuario> gestorSesion,
            ILogger<CuentaController> registro)
        {
            this.gestorUsuarios = gestorUsuarios;
            this.gestorSesion = gestorSesion;
            this.registro = registro;
        }

        [HttpGet]
        public IActionResult IniciarSesion(string? urlRetorno)
        {
            if (gestorSesion.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Inicio");
            }

            ViewData["Title"] = "Iniciar sesión";
            return View(new IniciarSesionViewModel { UrlRetorno = urlRetorno });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(IniciarSesionViewModel modelo)
        {
            ViewData["Title"] = "Iniciar sesión";

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = await gestorSesion.PasswordSignInAsync(
                modelo.Usuario,
                modelo.Password,
                modelo.Recordarme,
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(modelo.UrlRetorno) && Url.IsLocalUrl(modelo.UrlRetorno))
                {
                    return Redirect(modelo.UrlRetorno);
                }

                return RedirectToAction("Index", "Inicio");
            }

            if (resultado.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "La cuenta está bloqueada temporalmente por intentos fallidos.");
                return View(modelo);
            }

            ModelState.AddModelError(string.Empty, "Los datos ingresados no coinciden.");
            return View(modelo);
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            if (gestorSesion.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Inicio");
            }

            ViewData["Title"] = "Crear cuenta";
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroViewModel modelo)
        {
            ViewData["Title"] = "Crear cuenta";

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var nombre = modelo.Usuario.Trim().ToLowerInvariant();

            if (await gestorUsuarios.FindByNameAsync(nombre) is not null)
            {
                ModelState.AddModelError(nameof(modelo.Usuario), "Este usuario ya existe. Pruebe con otro.");
                return View(modelo);
            }

            var usuario = new Usuario
            {
                UserName = nombre,
                Email = modelo.Correo.Trim(),
                EmailConfirmed = true,
                FechaRegistro = FechaHora.Ahora
            };

            var resultado = await gestorUsuarios.CreateAsync(usuario, modelo.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modelo);
            }

            await gestorUsuarios.AddToRoleAsync(usuario, Roles.Usuario);

            TempData["Exito"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await gestorSesion.SignOutAsync();
            return RedirectToAction("Index", "Inicio");
        }

        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            ViewData["Title"] = "Recuperar contraseña";
            return View(new RecuperarPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel modelo)
        {
            ViewData["Title"] = "Recuperar contraseña";

            if (string.IsNullOrWhiteSpace(modelo.Correo) && string.IsNullOrWhiteSpace(modelo.Usuario))
            {
                ModelState.AddModelError(string.Empty, "Ingrese su correo o su nombre de usuario.");
                return View(modelo);
            }

            var usuario = string.IsNullOrWhiteSpace(modelo.Usuario)
                ? await gestorUsuarios.FindByEmailAsync(modelo.Correo!)
                : await gestorUsuarios.FindByNameAsync(modelo.Usuario);

            if (usuario is null)
            {
                TempData["Exito"] = "Si los datos corresponden a una cuenta, se enviará el enlace de recuperación.";
                return RedirectToAction(nameof(IniciarSesion));
            }

            var token = await gestorUsuarios.GeneratePasswordResetTokenAsync(usuario);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var enlace = Url.Action(
                nameof(RestablecerPassword),
                "Cuenta",
                new { usuario = usuario.UserName, token = tokenCodificado },
                Request.Scheme);

            registro.LogInformation("Enlace de recuperación para {Usuario}: {Enlace}", usuario.UserName, enlace);

            TempData["Exito"] = "Se generó el enlace de recuperación.";
            ViewBag.Enlace = enlace;

            return View(new RecuperarPasswordViewModel());
        }

        [HttpGet]
        public IActionResult RestablecerPassword(string usuario, string token)
        {
            ViewData["Title"] = "Cambiar contraseña";
            return View(new RestablecerPasswordViewModel { Usuario = usuario, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordViewModel modelo)
        {
            ViewData["Title"] = "Cambiar contraseña";

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = await gestorUsuarios.FindByNameAsync(modelo.Usuario);
            if (usuario is null)
            {
                ModelState.AddModelError(string.Empty, "No encontramos esta solicitud de recuperación.");
                return View(modelo);
            }

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(modelo.Token));
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, "El enlace de recuperación no es válido.");
                return View(modelo);
            }

            var resultado = await gestorUsuarios.ResetPasswordAsync(usuario, token, modelo.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modelo);
            }

            TempData["Exito"] = "Contraseña actualizada correctamente.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            ViewData["Title"] = "Acceso denegado";
            return View();
        }
    }
}
