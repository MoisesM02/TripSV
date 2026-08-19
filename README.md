# TripsSV — CMS de turismo de El Salvador

Migración de la aplicación original en PHP nativo + MySQL (`PTCEn`) a ASP.NET Core MVC (.NET 10) con
Entity Framework Core Code-First, SQL Server y ASP.NET Core Identity.

## Información del proyecto

**Materia:** [Código de materia] — Grupo teórico [Número]
**Grupo de trabajo:** [Número de grupo]

| Integrante | Carnet |
| Alberto Ramos Cruz | RC220772 |
| Moises Alonso Marroquin Ayala | MA220150|
| Rene Eduardo Hernandez Castro | HC220857 |
| Rafael Adolfo Ruiz García | RG210380 |

**Gestión del proyecto:** https://trello.com/b/mb5UHC9n
**Mock ups / Diseños:** Incluidos en el documento
**Licencia:** Este proyecto está bajo licencia [Creative Commons BY-NC-SA 4.0 / la que corresponda] — [enlace a la licencia]


## Puesta en marcha

1. Ajustar la cadena de conexión `ConexionTripSV` en `TripSV/appsettings.json`.

2. Crear la base de datos con las migraciones:

```bash
dotnet ef database update --project TripSV
```

3. Ejecutar la aplicación. Al iniciar se crean los roles y los dos usuarios definidos en
   `appsettings.json`:

```bash
dotnet run --project TripSV
```

## Usuarios iniciales

| Usuario | Contraseña | Rol |
|---|---|---|
| administrador | Administrador123$ | Administrador |
| visitante | Visitante123$ | Usuario |