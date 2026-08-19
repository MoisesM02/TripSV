using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TripSV.Modelos;

namespace TripSV.Datos
{
    public class SembradorDatos
    {
        private readonly ContextoTripSV contexto;
        private readonly UserManager<Usuario> gestorUsuarios;
        private readonly RoleManager<IdentityRole> gestorRoles;
        private readonly IConfiguration configuracion;
        private readonly ILogger<SembradorDatos> registro;

        public SembradorDatos(
            ContextoTripSV contexto,
            UserManager<Usuario> gestorUsuarios,
            RoleManager<IdentityRole> gestorRoles,
            IConfiguration configuracion,
            ILogger<SembradorDatos> registro)
        {
            this.contexto = contexto;
            this.gestorUsuarios = gestorUsuarios;
            this.gestorRoles = gestorRoles;
            this.configuracion = configuracion;
            this.registro = registro;
        }

        public async Task SembrarAsync()
        {
            await contexto.Database.MigrateAsync();
            await SembrarRolesAsync();
            await SembrarUsuariosAsync();
        }

        private async Task SembrarRolesAsync()
        {
            foreach (var rol in Roles.Todos)
            {
                if (!await gestorRoles.RoleExistsAsync(rol))
                {
                    await gestorRoles.CreateAsync(new IdentityRole(rol));
                    registro.LogInformation("Rol {Rol} creado", rol);
                }
            }
        }

        private async Task SembrarUsuariosAsync()
        {
            var administrador = configuracion.GetSection("UsuariosIniciales:Administrador");
            var usuario = configuracion.GetSection("UsuariosIniciales:Usuario");

            await CrearUsuarioAsync(
                administrador["Usuario"] ?? "administrador",
                administrador["Correo"] ?? "administrador@tripssv.com",
                administrador["Password"] ?? "Administrador123$",
                Roles.Administrador);

            await CrearUsuarioAsync(
                usuario["Usuario"] ?? "visitante",
                usuario["Correo"] ?? "visitante@tripssv.com",
                usuario["Password"] ?? "Visitante123$",
                Roles.Usuario);
        }

        private async Task CrearUsuarioAsync(string nombre, string correo, string password, string rol)
        {
            var existente = await gestorUsuarios.FindByNameAsync(nombre);
            if (existente is not null)
            {
                if (!await gestorUsuarios.IsInRoleAsync(existente, rol))
                {
                    await gestorUsuarios.AddToRoleAsync(existente, rol);
                }
                return;
            }

            var nuevo = new Usuario
            {
                UserName = nombre,
                Email = correo,
                EmailConfirmed = true,
                FechaRegistro = DateTime.Now
            };

            var resultado = await gestorUsuarios.CreateAsync(nuevo, password);
            if (resultado.Succeeded)
            {
                await gestorUsuarios.AddToRoleAsync(nuevo, rol);
                registro.LogInformation("Usuario {Usuario} creado con rol {Rol}", nombre, rol);
            }
            else
            {
                registro.LogWarning(
                    "No se pudo crear el usuario {Usuario}: {Errores}",
                    nombre,
                    string.Join(", ", resultado.Errors.Select(e => e.Description)));
            }
        }
    }
}
