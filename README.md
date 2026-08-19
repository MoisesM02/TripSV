# TripsSV — CMS de turismo de El Salvador

Migración de la aplicación original en PHP nativo + MySQL (`PTCEn`) a ASP.NET Core MVC (.NET 10) con
Entity Framework Core Code-First, SQL Server y ASP.NET Core Identity.

## Estructura

```
TripSV/
├── Modelos/            Categoria, Sitio, Comentario, Puntuacion, Usuario, Roles
├── Datos/
│   ├── ContextoTripSV.cs
│   ├── SembradorDatos.cs
│   ├── Configuraciones/     Fluent API por entidad (tablas y columnas en español)
│   ├── Migraciones/         Migraciones de EF Core
│   └── Migracion/           datos-mysql.sql (carga de datos migrados del dump original)
├── Servicios/          Servicios de negocio por módulo + validador de imágenes
├── ViewModels/
├── Controllers/        Inicio, Categorias, Sitios, Comentarios, Puntuaciones, Cuenta
├── Areas/Administracion/    Categorias, Sitios, Moderacion  (solo rol Administrador)
├── Views/
└── wwwroot/            Bootstrap, jQuery, CKEditor, imágenes y CSS del proyecto original

Herramientas/
└── ConvertirDumpMySql.cs    Genera datos-mysql.sql a partir de BD.sql (dump de MySQL)
```

## Puesta en marcha

1. Ajustar la cadena de conexión `ConexionTripSV` en `TripSV/appsettings.json`.

2. Crear la base de datos con las migraciones:

```bash
dotnet ef database update --project TripSV
```

3. Cargar los datos migrados desde MySQL (9 categorías, 27 sitios con sus imágenes, 83 comentarios y
   38 calificaciones). El script se detiene solo si las tablas ya tienen datos:

```bash
sqlcmd -S localhost -d TripSV -E -i TripSV/Datos/Migracion/datos-mysql.sql
```

4. Ejecutar la aplicación. Al iniciar se crean los roles y los dos usuarios definidos en
   `appsettings.json`:

```bash
dotnet run --project TripSV
```

## Usuarios iniciales

| Usuario | Contraseña | Rol |
|---|---|---|
| administrador | Administrador123$ | Administrador |
| visitante | Visitante123$ | Usuario |

## Regenerar el script de datos

Si cambia el dump de MySQL, se regenera el script con:

```bash
dotnet run Herramientas/ConvertirDumpMySql.cs
```

## Equivalencias con la aplicación PHP original

| PHP original | ASP.NET Core MVC |
|---|---|
| `index.php` | `Inicio/Index` |
| `catEN.php` | `Categorias/Index` |
| `mostrarSitios.php?cat=` | `Sitios/MostrarSitios` |
| `comments.php?place=` | `Sitios/Detalle/{id}` |
| `Backend/AddComments.php` | `Comentarios/Agregar` |
| `Backend/deleteComments.php` | `Comentarios/Eliminar` |
| `Backend/rating.php` | `Puntuaciones/Calificar` |
| `login.php` / `register.php` / `cerrarSesion.php` | `Cuenta/IniciarSesion`, `Cuenta/Registrar`, `Cuenta/CerrarSesion` |
| `password.php` / `formRecuperar.php` | `Cuenta/RecuperarPassword`, `Cuenta/RestablecerPassword` |
| `editCats.html` + `Backend/cats-*.php` | `Administracion/Categorias` |
| `addSites.php` + `Backend/uploadSite.php` | `Administracion/Sitios/Agregar` |
| `editSitios.php` | `Administracion/Sitios/Index` |
| `modificarSitio.php` + `prueba.php` | `Administracion/Sitios/Modificar` |
| (no existía) | `Administracion/Moderacion` |
