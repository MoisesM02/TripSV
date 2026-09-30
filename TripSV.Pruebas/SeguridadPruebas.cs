using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripSV.Controllers;
using TripSV.Modelos;

namespace TripSV.Pruebas
{
    public class SeguridadPruebas
    {
        private static readonly Type[] Controladores = typeof(InicioController).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Controller).IsAssignableFrom(t))
            .ToArray();

        public static TheoryData<string> ControladoresDeAdministracion() =>
            new(Controladores
                .Where(t => t.Namespace == "TripSV.Areas.Administracion.Controllers")
                .Select(t => t.FullName!));

        public static TheoryData<string> ControladoresDeUsuario() => new(
            typeof(FavoritosController).FullName!,
            typeof(ItinerariosController).FullName!,
            typeof(ComentariosController).FullName!,
            typeof(PuntuacionesController).FullName!);

        public static TheoryData<string> ControladoresPublicos() => new(
            typeof(InicioController).FullName!,
            typeof(CategoriasController).FullName!,
            typeof(SitiosController).FullName!);

        private static Type Buscar(string nombreCompleto) => Controladores.Single(t => t.FullName == nombreCompleto);

        [Fact]
        public void ExistenControladoresDeAdministracion()
        {
            Assert.Equal(3, ControladoresDeAdministracion().Count);
        }

        [Theory]
        [MemberData(nameof(ControladoresDeAdministracion))]
        public void ControladorDeAdministracion_ExigeRolAdministrador(string controlador)
        {
            var autorizacion = Buscar(controlador).GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(autorizacion);
            Assert.Equal(Roles.Administrador, autorizacion!.Roles);
        }

        [Theory]
        [MemberData(nameof(ControladoresDeUsuario))]
        public void ControladorDeUsuario_ExigeIniciarSesion(string controlador)
        {
            Assert.NotNull(Buscar(controlador).GetCustomAttribute<AuthorizeAttribute>());
        }

        [Theory]
        [MemberData(nameof(ControladoresPublicos))]
        public void ControladorPublico_PermiteVisitantesAnonimos(string controlador)
        {
            Assert.Null(Buscar(controlador).GetCustomAttribute<AuthorizeAttribute>());
        }

        [Fact]
        public void TodasLasAccionesPost_ValidanElTokenAntifalsificacion()
        {
            var accionesSinProteccion = Controladores
                .SelectMany(c => c.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(m => m.GetCustomAttribute<HttpPostAttribute>() is not null)
                .Where(m => m.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() is null &&
                            m.DeclaringType!.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() is null)
                .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")
                .ToList();

            Assert.Empty(accionesSinProteccion);
        }
    }
}
