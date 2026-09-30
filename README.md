# TripsSV — CMS de turismo de El Salvador

Aplicación web para la promoción del turismo en El Salvador, desarrollada en ASP.NET Core MVC (.NET 10) con
Entity Framework Core Code-First, SQL Server y ASP.NET Core Identity.

## Información del proyecto

**Nombre y código de materia:** Desarrollo de Software Empresarial DES104

**Grupo teórico:** G01T

**Número de grupo:** 3

| Integrante | Carnet |
|---|---|
| Alberto Ramos Cruz | RC220772 |
| Moises Alonso Marroquin Ayala | MA220150 |
| Rene Eduardo Hernandez Castro | HC220857 |
| Rafael Adolfo Ruiz García | RG210380 |

**Gestión del proyecto:** https://trello.com/b/mb5UHC9n

**Mock ups / Diseños:** Incluidos en el documento

**Licencia:** Este proyecto está bajo licencia [Creative Commons BY-NC-SA 4.0 / la que corresponda] — [enlace a la licencia]

## Documentación

| Documento | Contenido |
|---|---|
| [Manual del Programador](Documentacion/Manual%20del%20Programador.pdf) | Arquitectura y su justificación, modelo de datos, acceso a datos, seguridad, rendimiento, escalabilidad, configuración del entorno y despliegue |
| [Manual del Usuario](Documentacion/Manual%20del%20Usuario.pdf) | Guía paso a paso con capturas de pantalla para visitantes, usuarios registrados y administradores |
| [Documento de Pruebas](Documentacion/Documento%20de%20Pruebas.pdf) | Pruebas automatizadas y de integración, cobertura de casos críticos y resultados |

Las fuentes de los documentos están en `Documentacion/fuentes`. Con la aplicación en ejecución, se regeneran las
capturas y los PDF con:

```bash
dotnet run Herramientas/GenerarDocumentacion.cs -- todo .
```

## Puesta en marcha

1. Ajustar la cadena de conexión `ConexionTripSV` en `TripSV/appsettings.json`, o definirla fuera del repositorio
   con User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConexionTripSV" "Server=localhost;Database=TripSV;Trusted_Connection=True;TrustServerCertificate=True" --project TripSV
```

2. Crear la base de datos con las migraciones:

```bash
dotnet ef database update --project TripSV
```

3. Ejecutar la aplicación. Al iniciar se crean los roles y los dos usuarios definidos en
   `appsettings.json`:

```bash
dotnet run --project TripSV
```

## Pruebas

El proyecto `TripSV.Pruebas` contiene 80 casos de prueba automatizados (xUnit) sobre calificaciones, reseñas y
moderación, favoritos, itinerarios, categorías y destinos, validación de imágenes, saneamiento de HTML y control
de acceso. No requieren SQL Server: usan una base SQLite en memoria.

```bash
dotnet test
```

## Ejecutable

`Ejecutable/TripSV.zip` contiene la aplicación publicada en modo Release para Windows x64. Requiere el
ASP.NET Core Runtime 10 y acceso a SQL Server. Descomprimir, ajustar `appsettings.json` y ejecutar `TripSV.exe`.
Los pasos para otros entornos están en el Manual del Programador.

## Usuarios iniciales

| Usuario | Contraseña | Rol |
|---|---|---|
| administrador | Administrador123$ | Administrador |
| visitante | Visitante123$ | Usuario |
