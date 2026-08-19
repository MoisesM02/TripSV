using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TripSV.Datos;
using TripSV.Modelos;
using TripSV.Servicios;

var constructor = WebApplication.CreateBuilder(args);

var cadenaConexion = constructor.Configuration.GetConnectionString("ConexionTripSV")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión ConexionTripSV.");

constructor.Services.AddDbContext<ContextoTripSV>(opciones => opciones.UseSqlServer(cadenaConexion));

constructor.Services
    .AddIdentity<Usuario, IdentityRole>(opciones =>
    {
        opciones.Password.RequiredLength = 8;
        opciones.Password.RequireDigit = true;
        opciones.Password.RequireUppercase = false;
        opciones.Password.RequireLowercase = true;
        opciones.Password.RequireNonAlphanumeric = false;
        opciones.User.RequireUniqueEmail = false;
        opciones.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
        opciones.SignIn.RequireConfirmedAccount = false;
        opciones.Lockout.MaxFailedAccessAttempts = 5;
        opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<ContextoTripSV>()
    .AddDefaultTokenProviders();

constructor.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Cuenta/IniciarSesion";
    opciones.LogoutPath = "/Cuenta/CerrarSesion";
    opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    opciones.ExpireTimeSpan = TimeSpan.FromMinutes(90);
    opciones.SlidingExpiration = true;
});

constructor.Services.AddScoped<ICategoriasServicio, CategoriasServicio>();
constructor.Services.AddScoped<ISitiosServicio, SitiosServicio>();
constructor.Services.AddScoped<IComentariosServicio, ComentariosServicio>();
constructor.Services.AddScoped<IPuntuacionesServicio, PuntuacionesServicio>();
constructor.Services.AddScoped<SembradorDatos>();

constructor.Services.AddControllersWithViews();

var app = constructor.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Inicio/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Inicio}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

using (var alcance = app.Services.CreateScope())
{
    var sembrador = alcance.ServiceProvider.GetRequiredService<SembradorDatos>();
    await sembrador.SembrarAsync();
}

app.Run();
