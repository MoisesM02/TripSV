using System.Globalization;
using System.Text;

var rutaDump = @"D:\udb\DES\PTCEn\BD.sql";
var rutaSalida = @"D:\udb\DES_catedra\TripSV\TripSV\Datos\Migracion\datos-mysql.sql";

var tablas = new Dictionary<string, List<List<Valor>>>(StringComparer.OrdinalIgnoreCase);
var interes = new[] { "categories", "places", "comentarios", "puntuaciones" };

foreach (var tabla in interes) tablas[tabla] = new List<List<Valor>>();

var texto = File.ReadAllText(rutaDump, Encoding.UTF8);
var posicion = 0;

while (true)
{
    var indice = texto.IndexOf("INSERT INTO `", posicion, StringComparison.Ordinal);
    if (indice < 0) break;

    var inicioNombre = indice + "INSERT INTO `".Length;
    var finNombre = texto.IndexOf('`', inicioNombre);
    var tabla = texto.Substring(inicioNombre, finNombre - inicioNombre);

    var indiceValues = texto.IndexOf(" VALUES", finNombre, StringComparison.Ordinal);
    if (indiceValues < 0) break;

    var cursor = indiceValues + " VALUES".Length;
    var fin = BuscarFinSentencia(texto, cursor);

    if (tablas.ContainsKey(tabla))
    {
        foreach (var fila in LeerFilas(texto, cursor, fin))
        {
            tablas[tabla].Add(fila);
        }
    }

    posicion = fin;
}

foreach (var tabla in interes)
{
    Console.WriteLine($"{tabla}: {tablas[tabla].Count} filas");
}

var avisos = new List<string>();
var salida = new StringBuilder();

salida.AppendLine("SET NOCOUNT ON;");
salida.AppendLine("SET XACT_ABORT ON;");
salida.AppendLine();
salida.AppendLine("IF EXISTS (SELECT 1 FROM categorias) OR EXISTS (SELECT 1 FROM sitios)");
salida.AppendLine("BEGIN");
salida.AppendLine("    RAISERROR (N'Las tablas categorias o sitios ya contienen datos. No se ejecuto la carga.', 16, 1);");
salida.AppendLine("    RETURN;");
salida.AppendLine("END;");
salida.AppendLine();
salida.AppendLine("BEGIN TRANSACTION;");
salida.AppendLine();

var categorias = new List<(int Id, string Nombre)>();

salida.AppendLine("SET IDENTITY_INSERT categorias ON;");
salida.AppendLine();

foreach (var fila in tablas["categories"])
{
    var id = int.Parse(fila[0].Texto!);
    var nombre = Limpiar(fila[1].Texto!).Trim();
    var descripcion = Limpiar(fila[2].Texto ?? string.Empty).Trim();

    if (categorias.Any(c => string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
    {
        avisos.Add($"categoria duplicada omitida: id={id} nombre={nombre}");
        continue;
    }

    categorias.Add((id, nombre));

    salida.AppendLine($"INSERT INTO categorias (id, nombre, descripcion, imagen, imagen_tipo) VALUES ({id}, {Cadena(nombre)}, {Cadena(Recortar(descripcion, 200))},");
    salida.AppendLine(Binario(fila[3]) + ", N'image/jpeg');");
}

salida.AppendLine();
salida.AppendLine("SET IDENTITY_INSERT categorias OFF;");
salida.AppendLine();

var sitios = new List<(int Id, string Nombre)>();
var filasSitios = tablas["places"]
    .Select(f => new
    {
        Id = int.Parse(f[0].Texto!),
        Nombre = Limpiar(f[1].Texto!).Trim(),
        Descripcion = Limpiar(f[2].Texto ?? string.Empty).Trim(),
        Imagen = f[3],
        Calificacion = decimal.Parse(f[4].Texto!, CultureInfo.InvariantCulture),
        Ubicacion = Limpiar(f[5].Texto ?? string.Empty).Trim(),
        Categoria = Limpiar(f[6].Texto ?? string.Empty).Trim(),
        Informacion = Uri.UnescapeDataString((f[7].Texto ?? string.Empty).Replace("+", " ")),
        Peso = (f[2].Texto?.Length ?? 0) + (f[7].Texto?.Length ?? 0)
    })
    .OrderByDescending(f => f.Peso)
    .ToList();

salida.AppendLine("SET IDENTITY_INSERT sitios ON;");
salida.AppendLine();

foreach (var fila in filasSitios.OrderBy(f => f.Id))
{
    if (sitios.Any(s => string.Equals(s.Nombre, fila.Nombre, StringComparison.OrdinalIgnoreCase)))
    {
        avisos.Add($"sitio duplicado omitido: id={fila.Id} nombre={fila.Nombre}");
        continue;
    }

    var categoria = categorias.FirstOrDefault(c => string.Equals(c.Nombre, fila.Categoria, StringComparison.OrdinalIgnoreCase));
    if (categoria.Id == 0)
    {
        avisos.Add($"sitio sin categoria coincidente omitido: id={fila.Id} nombre={fila.Nombre} categoria={fila.Categoria}");
        continue;
    }

    sitios.Add((fila.Id, fila.Nombre));

    var descripcion = string.IsNullOrWhiteSpace(fila.Descripcion) ? fila.Nombre : fila.Descripcion;

    salida.AppendLine($"INSERT INTO sitios (id, nombre, descripcion, imagen, imagen_tipo, calificacion, ubicacion, categoria_id, informacion, fecha_creacion, total_puntuaciones)");
    salida.AppendLine($"VALUES ({fila.Id}, {Cadena(Recortar(fila.Nombre, 100))}, {Cadena(Recortar(descripcion, 500))},");
    salida.AppendLine(Binario(fila.Imagen) + ",");
    salida.AppendLine($"N'image/jpeg', {fila.Calificacion.ToString("0.00", CultureInfo.InvariantCulture)}, {Cadena(Recortar(fila.Ubicacion, 60))}, {categoria.Id},");
    salida.AppendLine($"{Cadena(fila.Informacion)}, '2020-10-08T00:00:00', 0);");
    salida.AppendLine();
}

salida.AppendLine("SET IDENTITY_INSERT sitios OFF;");
salida.AppendLine();

salida.AppendLine("SET IDENTITY_INSERT comentarios ON;");
salida.AppendLine();

var comentariosOmitidos = 0;

foreach (var fila in tablas["comentarios"])
{
    var id = int.Parse(fila[0].Texto!);
    var usuario = Limpiar(fila[1].Texto ?? string.Empty).Trim();
    var comentario = Limpiar(fila[2].Texto ?? string.Empty).Trim();
    var fechaTexto = Limpiar(fila[3].Texto ?? string.Empty).Trim();
    var lugar = Limpiar(fila[4].Texto ?? string.Empty).Trim();

    var sitio = sitios.FirstOrDefault(s => string.Equals(s.Nombre, lugar, StringComparison.OrdinalIgnoreCase));
    if (sitio.Id == 0 || string.IsNullOrWhiteSpace(comentario) || string.IsNullOrWhiteSpace(usuario))
    {
        comentariosOmitidos++;
        continue;
    }

    var fecha = ConvertirFecha(fechaTexto);

    salida.AppendLine($"INSERT INTO comentarios (id, usuario_id, nombre_usuario, comentario, fecha, sitio_id, respuesta_a_id, oculto)");
    salida.AppendLine($"VALUES ({id}, NULL, {Cadena(Recortar(usuario, 64))}, {Cadena(Recortar(comentario, 1000))}, '{fecha:yyyy-MM-ddTHH:mm:ss}', {sitio.Id}, NULL, 0);");
}

salida.AppendLine();
salida.AppendLine("SET IDENTITY_INSERT comentarios OFF;");
salida.AppendLine();

salida.AppendLine("SET IDENTITY_INSERT puntuaciones ON;");
salida.AppendLine();

var puntuacionesOmitidas = 0;
var vistas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

foreach (var fila in tablas["puntuaciones"].OrderByDescending(f => int.Parse(f[0].Texto!)))
{
    var id = int.Parse(fila[0].Texto!);
    var usuario = Limpiar(fila[1].Texto ?? string.Empty).Trim();
    var lugar = Limpiar(fila[2].Texto ?? string.Empty).Trim();
    var valor = int.Parse(fila[3].Texto!);

    var sitio = sitios.FirstOrDefault(s => string.Equals(s.Nombre, lugar, StringComparison.OrdinalIgnoreCase));

    if (sitio.Id == 0 || string.IsNullOrWhiteSpace(usuario) || valor < 1 || valor > 5)
    {
        puntuacionesOmitidas++;
        continue;
    }

    if (!vistas.Add($"{sitio.Id}|{usuario}"))
    {
        puntuacionesOmitidas++;
        continue;
    }

    salida.AppendLine($"INSERT INTO puntuaciones (id, usuario_id, nombre_usuario, sitio_id, puntuacion, fecha) VALUES ({id}, NULL, {Cadena(Recortar(usuario, 64))}, {sitio.Id}, {valor}, '2020-10-08T00:00:00');");
}

salida.AppendLine();
salida.AppendLine("SET IDENTITY_INSERT puntuaciones OFF;");
salida.AppendLine();

salida.AppendLine("UPDATE s");
salida.AppendLine("SET s.total_puntuaciones = ISNULL(p.total, 0),");
salida.AppendLine("    s.calificacion = ISNULL(ROUND(p.promedio, 1), 0)");
salida.AppendLine("FROM sitios s");
salida.AppendLine("LEFT JOIN (");
salida.AppendLine("    SELECT sitio_id, COUNT(*) AS total, AVG(CAST(puntuacion AS decimal(3,2))) AS promedio");
salida.AppendLine("    FROM puntuaciones");
salida.AppendLine("    GROUP BY sitio_id");
salida.AppendLine(") p ON p.sitio_id = s.id;");
salida.AppendLine();
salida.AppendLine("COMMIT TRANSACTION;");

Directory.CreateDirectory(Path.GetDirectoryName(rutaSalida)!);
File.WriteAllText(rutaSalida, salida.ToString(), new UTF8Encoding(true));

Console.WriteLine();
Console.WriteLine($"categorias generadas: {categorias.Count}");
Console.WriteLine($"sitios generados: {sitios.Count}");
Console.WriteLine($"comentarios omitidos: {comentariosOmitidos}");
Console.WriteLine($"puntuaciones omitidas: {puntuacionesOmitidas}");
Console.WriteLine($"tamano del script: {new FileInfo(rutaSalida).Length / 1024} KB");
Console.WriteLine();
foreach (var aviso in avisos) Console.WriteLine("AVISO: " + aviso);

static int BuscarFinSentencia(string texto, int desde)
{
    var enCadena = false;

    for (var i = desde; i < texto.Length; i++)
    {
        var c = texto[i];

        if (enCadena)
        {
            if (c == '\\') { i++; continue; }
            if (c == '\'') enCadena = false;
            continue;
        }

        if (c == '\'') { enCadena = true; continue; }
        if (c == ';') return i + 1;
    }

    return texto.Length;
}

static IEnumerable<List<Valor>> LeerFilas(string texto, int desde, int hasta)
{
    var i = desde;

    while (i < hasta)
    {
        while (i < hasta && texto[i] != '(') i++;
        if (i >= hasta) break;

        i++;
        var fila = new List<Valor>();

        while (i < hasta)
        {
            while (i < hasta && (texto[i] == ' ' || texto[i] == '\r' || texto[i] == '\n' || texto[i] == '\t')) i++;

            if (texto[i] == '\'')
            {
                i++;
                var constructor = new StringBuilder();

                while (i < hasta && texto[i] != '\'')
                {
                    if (texto[i] == '\\')
                    {
                        i++;
                        constructor.Append(texto[i] switch
                        {
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            '0' => '\0',
                            'b' => '\b',
                            'Z' => (char)26,
                            var otro => otro
                        });
                        i++;
                        continue;
                    }

                    constructor.Append(texto[i]);
                    i++;
                }

                i++;
                fila.Add(new Valor { Texto = constructor.ToString() });
            }
            else
            {
                var inicio = i;
                while (i < hasta && texto[i] != ',' && texto[i] != ')') i++;
                var crudo = texto.Substring(inicio, i - inicio).Trim();

                if (crudo.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    fila.Add(new Valor { Hex = crudo.Substring(2) });
                }
                else if (string.Equals(crudo, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    fila.Add(new Valor());
                }
                else
                {
                    fila.Add(new Valor { Texto = crudo });
                }
            }

            while (i < hasta && (texto[i] == ' ' || texto[i] == '\r' || texto[i] == '\n' || texto[i] == '\t')) i++;

            if (i < hasta && texto[i] == ',') { i++; continue; }
            if (i < hasta && texto[i] == ')') { i++; break; }
        }

        yield return fila;

        while (i < hasta && (texto[i] == ' ' || texto[i] == ',' || texto[i] == '\r' || texto[i] == '\n' || texto[i] == '\t')) i++;
        if (i < hasta && texto[i] == ';') break;
    }
}

static string Limpiar(string valor)
{
    if (string.IsNullOrEmpty(valor)) return string.Empty;

    var normalizado = valor.Replace("\0", string.Empty);

    if (normalizado.Contains('Ã') || normalizado.Contains('Â'))
    {
        try
        {
            var bytes = Encoding.Latin1.GetBytes(normalizado);
            var decodificacion = new UTF8Encoding(false, true);
            var reparado = decodificacion.GetString(bytes);
            if (!reparado.Contains('\uFFFD')) normalizado = reparado;
        }
        catch (DecoderFallbackException)
        {
        }
    }

    return System.Net.WebUtility.HtmlDecode(normalizado);
}

static string Recortar(string valor, int maximo) =>
    valor.Length <= maximo ? valor : valor.Substring(0, maximo);

static string Cadena(string valor) => "N'" + valor.Replace("'", "''") + "'";

static string Binario(Valor valor)
{
    if (valor.Hex is null || valor.Hex.Length == 0) return "NULL";

    var trozos = new List<string>();
    const int tamano = 8000;

    for (var i = 0; i < valor.Hex.Length; i += tamano)
    {
        var largo = Math.Min(tamano, valor.Hex.Length - i);
        trozos.Add("CONVERT(varbinary(max), 0x" + valor.Hex.Substring(i, largo) + ")");
    }

    return string.Join(" +" + Environment.NewLine, trozos);
}

static DateTime ConvertirFecha(string valor)
{
    var formatos = new[]
    {
        "dd 'de' MMMM 'de' yyyy hh:mm:ss tt",
        "d 'de' MMMM 'de' yyyy hh:mm:ss tt"
    };

    var culturas = new[] { new CultureInfo("en-US"), new CultureInfo("es-SV") };

    foreach (var cultura in culturas)
    {
        if (DateTime.TryParseExact(valor, formatos, cultura, DateTimeStyles.None, out var fecha))
        {
            return fecha;
        }
    }

    return new DateTime(2020, 10, 8);
}

class Valor
{
    public string? Texto { get; set; }

    public string? Hex { get; set; }
}
