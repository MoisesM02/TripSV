#:package Microsoft.Playwright@1.*
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property PublishAot=false

using System.Text.Json;
using Microsoft.Playwright;

var modo = args.Length > 0 ? args[0] : "todo";
var raiz = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
var urlBase = args.Length > 2 ? args[2] : "http://localhost:5255";

var carpetaDocumentacion = Path.Combine(raiz, "Documentacion");
var carpetaFuentes = Path.Combine(carpetaDocumentacion, "fuentes");
var carpetaCapturas = Path.Combine(carpetaFuentes, "capturas");
Directory.CreateDirectory(carpetaCapturas);

using var playwright = await Playwright.CreateAsync();
await using var navegador = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });

if (modo is "capturas" or "todo")
{
    await TomarCapturasAsync();
}

if (modo is "pdf" or "todo")
{
    await GenerarPdfsAsync();
}

async Task TomarCapturasAsync()
{
    var usuarios = LeerUsuariosIniciales();

    await using (var anonimo = await NuevoContextoAsync())
    {
        await CapturarAsync(anonimo, "/", "01-inicio");
        await CapturarAsync(anonimo, "/", "02-inicio-categorias", pagina => DesplazarAsync(pagina, "text=¿Qué tipo de viaje busca?"));
        await CapturarAsync(anonimo, "/Categorias", "03-categorias");
        await CapturarAsync(anonimo, "/Sitios?texto=lago&orden=nombre", "04-catalogo-busqueda");
        await CapturarAsync(anonimo, "/Sitios/Detalle/71", "05-detalle-sitio");
        await CapturarAsync(anonimo, "/Sitios/Detalle/71", "06-detalle-resenas", pagina => DesplazarAsync(pagina, "text=Reseñas de la comunidad"));
        await CapturarAsync(anonimo, "/Cuenta/Registrar", "07-registro");
        await CapturarAsync(anonimo, "/Cuenta/IniciarSesion", "08-iniciar-sesion");
        await CapturarAsync(anonimo, "/Cuenta/RecuperarPassword", "09-recuperar-contrasena");
        await CapturarAsync(anonimo, "/pagina-que-no-existe", "23-error-404");
    }

    await using (var visitante = await IniciarSesionAsync(usuarios.Usuario, usuarios.ClaveUsuario))
    {
        await CapturarAsync(visitante, "/Sitios/Detalle/71", "10-detalle-usuario");
        await CapturarAsync(visitante, "/Sitios/Detalle/71", "11-detalle-publicar-resena", async pagina =>
        {
            var hilo = pagina.Locator(".comments-container > .card").Last;
            await hilo.Locator("button:has-text('Responder')").ClickAsync();
            await pagina.WaitForTimeoutAsync(500);
            await hilo.EvaluateAsync("elemento => elemento.scrollIntoView({ block: 'start' })");
            await pagina.EvaluateAsync("() => window.scrollBy(0, -250)");
            await pagina.WaitForTimeoutAsync(400);
        });
        await CapturarAsync(visitante, "/Sitios", "12-menu-usuario", async pagina =>
        {
            await pagina.ClickAsync("#menuUsuario");
            await pagina.WaitForSelectorAsync(".dropdown-menu.show");
        });
        await CapturarAsync(visitante, "/Favoritos", "13-mis-favoritos");
        await CapturarAsync(visitante, "/Itinerarios", "14-mis-itinerarios");
        await CapturarAsync(visitante, "/Itinerarios/Crear", "15-nuevo-itinerario");
        await CapturarAsync(visitante, "/Itinerarios", "16-planificador", async pagina =>
        {
            await pagina.Locator("text=Abrir planificador").First.ClickAsync();
            await pagina.WaitForLoadStateAsync(LoadState.Load);
            await pagina.WaitForTimeoutAsync(600);
        });
    }

    await using (var administrador = await IniciarSesionAsync(usuarios.Administrador, usuarios.ClaveAdministrador))
    {
        await CapturarAsync(administrador, "/", "17-menu-administrar", async pagina =>
        {
            await pagina.ClickAsync("#menuAdministrar");
            await pagina.WaitForSelectorAsync(".dropdown-menu.show");
        });
        await CapturarAsync(administrador, "/Administracion/Sitios", "18-admin-sitios");
        await CapturarAsync(administrador, "/Administracion/Sitios/Agregar", "19-admin-agregar-sitio", pagina => pagina.WaitForTimeoutAsync(1500));
        await CapturarAsync(administrador, "/Administracion/Categorias", "20-admin-categorias");
        await CapturarAsync(administrador, "/Administracion/Moderacion", "21-admin-moderacion");
    }

    await using (var movil = await navegador.NewContextAsync(new()
    {
        ViewportSize = new() { Width = 390, Height = 844 },
        DeviceScaleFactor = 2,
        IsMobile = true,
        HasTouch = true
    }))
    {
        await CapturarAsync(movil, "/", "22-inicio-movil");
    }
}

async Task GenerarPdfsAsync()
{
    foreach (var documento in new[] { "Manual del Programador", "Manual del Usuario", "Documento de Pruebas" })
    {
        var origen = Path.Combine(carpetaFuentes, documento + ".html");
        if (!File.Exists(origen))
        {
            Console.WriteLine($"No se encontró {origen}");
            continue;
        }

        var pagina = await navegador.NewPageAsync();
        await pagina.GotoAsync(new Uri(origen).AbsoluteUri, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await pagina.PdfAsync(new()
        {
            Path = Path.Combine(carpetaDocumentacion, documento + ".pdf"),
            Format = "Letter",
            PrintBackground = true,
            DisplayHeaderFooter = true,
            HeaderTemplate = "<span></span>",
            FooterTemplate = $"<div style=\"font-size:8px;width:100%;padding:0 16mm;color:#6b7280;display:flex;justify-content:space-between\"><span>Trips SV · {documento}</span><span>Página <span class=\"pageNumber\"></span> de <span class=\"totalPages\"></span></span></div>",
            Margin = new() { Top = "16mm", Bottom = "18mm", Left = "14mm", Right = "14mm" }
        });
        await pagina.CloseAsync();
        Console.WriteLine($"Generado: {documento}.pdf");
    }
}

async Task<IBrowserContext> NuevoContextoAsync() =>
    await navegador.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 800 } });

async Task<IBrowserContext> IniciarSesionAsync(string usuario, string clave)
{
    var contexto = await NuevoContextoAsync();
    var pagina = await contexto.NewPageAsync();
    await pagina.GotoAsync(urlBase + "/Cuenta/IniciarSesion");
    await pagina.FillAsync("#Usuario", usuario);
    await pagina.FillAsync("#Password", clave);
    await pagina.ClickAsync("main form button[type=submit]");
    await pagina.WaitForURLAsync(url => !url.Contains("IniciarSesion"));
    await pagina.CloseAsync();
    return contexto;
}

async Task CapturarAsync(IBrowserContext contexto, string ruta, string nombre, Func<IPage, Task>? preparar = null)
{
    var pagina = await contexto.NewPageAsync();
    await pagina.GotoAsync(urlBase + ruta, new() { WaitUntil = WaitUntilState.Load });
    await pagina.AddStyleTagAsync(new() { Content = "html{scroll-behavior:auto !important}.alert-dismissible{display:none !important}" });
    await pagina.WaitForTimeoutAsync(700);

    if (preparar is not null)
    {
        await preparar(pagina);
    }

    await pagina.ScreenshotAsync(new()
    {
        Path = Path.Combine(carpetaCapturas, nombre + ".jpg"),
        Type = ScreenshotType.Jpeg,
        Quality = 82
    });
    await pagina.CloseAsync();
    Console.WriteLine($"Captura: {nombre}.jpg");
}

async Task DesplazarAsync(IPage pagina, string selector)
{
    await pagina.Locator(selector).First.EvaluateAsync("elemento => elemento.scrollIntoView({ block: 'start' })");
    await pagina.EvaluateAsync("() => window.scrollBy(0, -110)");
    await pagina.WaitForTimeoutAsync(500);
}

(string Administrador, string ClaveAdministrador, string Usuario, string ClaveUsuario) LeerUsuariosIniciales()
{
    using var configuracion = JsonDocument.Parse(File.ReadAllText(Path.Combine(raiz, "TripSV", "appsettings.json")));
    var iniciales = configuracion.RootElement.GetProperty("UsuariosIniciales");
    var administrador = iniciales.GetProperty("Administrador");
    var usuario = iniciales.GetProperty("Usuario");

    return (
        administrador.GetProperty("Usuario").GetString()!,
        administrador.GetProperty("Password").GetString()!,
        usuario.GetProperty("Usuario").GetString()!,
        usuario.GetProperty("Password").GetString()!);
}
