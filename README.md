# NIF-DNI-NIE-CIF Validation

Biblioteca .NET para validar números de identificación españoles: **NIF**, **DNI**, **NIE** y **CIF**.

Proporciona métodos síncronos, asíncronos, individuales y por lotes para verificar el formato y los dígitos de control de documentos de identificación fiscal y personal de España.

---

## Características

- **Validación completa** de DNI, NIE, NIF (incluye K/L/M) y CIF
- **Tres estilos de API**:
  - Métodos booleanos simples (`EsDniValido`)
  - Métodos con excepción (`ValidarDni`) que lanzan `DocumentoNoValidoException`
  - Métodos con resultado (`IntentarValidarDni`) que devuelven `ResultadoValidacion`
- **Validación por lotes** (síncrona y asíncrona con procesamiento en paralelo)
- **Objetos de valor (Value Objects)** inmutables: `Dni`, `Nie`, `Nif`, `Cif`
- **100% documentado** con XML docs en español
- **Normalización automática** (elimina espacios, guiones, puntos)
- **Utilidades** para calcular letras/dígitos de control
- **Resultado de lote completo** con válidos, inválidos, estadísticas y porcentajes

---

## Instalación

```bash
dotnet add package NIF.DNI.NIE.CIF.Validation
```

---

## Uso Rápido

```csharp
using nif_dni_nie_cif_validation.Implentaciones;

var validador = new ValidadorDocumentos();

// ✅ Validación booleana simple
bool esValido = validador.EsDniValido("12345678Z");

// ✅ Validación con resultado detallado (sin excepción)
var resultado = validador.IntentarValidarDni("12345678Z");
if (resultado.EsValido)
    Console.WriteLine(resultado.Mensaje);

// ✅ Validación con excepción si es inválido
try
{
    var documento = validador.ValidarDni("12345678Z");
    Console.WriteLine($"Control: {documento.CaracterControl}");
}
catch (DocumentoNoValidoException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## Tipos de Documento Soportados

| Tipo | Formato | Ejemplo | Descripción |
|------|---------|---------|-------------|
| **DNI** | 8 dígitos + 1 letra | `12345678Z` | Documento Nacional de Identidad |
| **NIE** | X/Y/Z + 7 dígitos + 1 letra | `X1234567L` | Número de Identidad de Extranjero |
| **NIF** | DNI, NIE o K/L/M + 7 dígitos + letra | `K1234567A` | Número de Identificación Fiscal |
| **CIF** | Letra + 7 dígitos + control | `B12345678` | Código de Identificación Fiscal |

---

## API Completa

### Métodos Booleanos (simples)

```csharp
bool EsDniValido(string dni);
bool EsNieValido(string nie);
bool EsNifValido(string nif);
bool EsCifValido(string cif);
bool EsDocumentoValido(string documento); // Auto-detecta tipo
```

### Métodos con Excepción

Lanzan `DocumentoNoValidoException` si el documento no es válido. Devuelven `DocumentoValidado`.

```csharp
DocumentoValidado ValidarDni(string dni);
DocumentoValidado ValidarNie(string nie);
DocumentoValidado ValidarNif(string nif);
DocumentoValidado ValidarCif(string cif);
DocumentoValidado ValidarDocumento(string documento); // Auto-detecta tipo
```

### Métodos sin Excepción (resultado detallado)

Devuelven `ResultadoValidacion` sin lanzar excepciones.

```csharp
ResultadoValidacion IntentarValidarDni(string dni);
ResultadoValidacion IntentarValidarNie(string nie);
ResultadoValidacion IntentarValidarNif(string nif);
ResultadoValidacion IntentarValidarCif(string cif);
ResultadoValidacion IntentarValidarDocumento(string documento);
```

### Validación por Lotes (síncrona)

```csharp
ResultadoValidacionLote ValidarLoteDni(IEnumerable<string> dnis);
ResultadoValidacionLote ValidarLoteNie(IEnumerable<string> nies);
ResultadoValidacionLote ValidarLoteNif(IEnumerable<string> nifs);
ResultadoValidacionLote ValidarLoteCif(IEnumerable<string> cifs);
ResultadoValidacionLote ValidarLoteDocumentos(IEnumerable<string> documentos);
```

### Validación por Lotes (asíncrona, con paralelismo)

```csharp
Task<ResultadoValidacionLote> ValidarLoteDniAsync(IEnumerable<string> dnis, CancellationToken ct = default);
Task<ResultadoValidacionLote> ValidarLoteNieAsync(IEnumerable<string> nies, CancellationToken ct = default);
Task<ResultadoValidacionLote> ValidarLoteNifAsync(IEnumerable<string> nifs, CancellationToken ct = default);
Task<ResultadoValidacionLote> ValidarLoteCifAsync(IEnumerable<string> cifs, CancellationToken ct = default);
Task<ResultadoValidacionLote> ValidarLoteDocumentosAsync(IEnumerable<string> documentos, CancellationToken ct = default);
```

### Utilidades

```csharp
TipoDocumento DetectarTipo(string documento);
string Normalizar(string documento);
char ObtenerLetraDni(int numero);
char ObtenerLetraNie(string nie);
(char Digito, char Letra) ObtenerControlCif(string cif);
```

---

## Objetos de Valor (Value Objects)

Los Value Objects son inmutables y solo pueden crearse con valores válidos:

```csharp
using nif_dni_nie_cif_validation.Modelos;

// Crear un DNI válido (lanza excepción si inválido)
var dni = Dni.Crear("12345678Z");
Console.WriteLine(dni.Numero);  // 12345678
Console.WriteLine(dni.Letra);   // 'Z'

// Intentar crear sin excepción (patrón TryParse)
if (Dni.Intentar("12345678Z", out var dniValido))
    Console.WriteLine(dniValido!.Valor);

// Generar un DNI a partir de un número
var dniGenerado = Dni.DesdeNumero(12345678);

// Crear un NIE
var nie = Nie.Crear("X1234567L");
Console.WriteLine(nie.LetraInicial);  // 'X'

// Crear un CIF
var cif = Cif.Crear("B12345678");
Console.WriteLine(cif.TipoEntidad);      // "Sociedad de Responsabilidad Limitada"
Console.WriteLine(cif.CodigoProvincia);   // "12"

// Conversión implícita a string
string texto = dni; // "12345678Z"
```

---

## Validación por Lotes - Ejemplo Completo

```csharp
var validador = new ValidadorDocumentos();

var documentos = new[]
{
    "12345678Z",  // DNI
    "X1234567L",  // NIE
    "B12345678",  // CIF
    "INVALIDO",   // Inválido
    "00000000T",  // DNI
    "999",        // Inválido
};

// Síncrono
var lote = validador.ValidarLoteDocumentos(documentos);

Console.WriteLine(lote.Resumen);
// "Procesados: 6 | Válidos: 4 (66.67%) | Inválidos: 2 (33.33%)"

Console.WriteLine($"Todos válidos: {lote.TodosValidos}");     // false
Console.WriteLine($"Tiene errores: {lote.TieneInvalidos}");    // true

// Iterar válidos
foreach (var valido in lote.Validos)
    Console.WriteLine($"  {valido.TipoDocumento}: {valido.ValorNormalizado}");

// Iterar inválidos
foreach (var error in lote.Invalidos)
    Console.WriteLine($"  Error: {error.Valor} - {error.Mensaje}");

// Filtrar por tipo
var dnis = lote.ObtenerValidosPorTipo(TipoDocumento.DNI);

// Asíncrono con cancelación
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var loteAsync = await validador.ValidarLoteDocumentosAsync(documentos, cts.Token);
```

---

## Modelo de Resultados

### `ResultadoValidacion`

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `EsValido` | `bool` | Si el documento es válido |
| `ValorOriginal` | `string` | Valor tal como fue proporcionado |
| `ValorNormalizado` | `string` | Valor sin espacios/guiones, en mayúsculas |
| `TipoDocumento` | `TipoDocumento` | Tipo detectado |
| `Mensaje` | `string` | Mensaje descriptivo |
| `CaracterControl` | `string?` | Letra o dígito de control |
| `CaracterControlEsperado` | `string?` | Control esperado (para depuración) |

### `ResultadoValidacionLote`

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Validos` | `List<DocumentoValidado>` | Documentos válidos |
| `Invalidos` | `List<ErrorValidacion>` | Errores de validación |
| `ResultadosIndividuales` | `List<ResultadoValidacion>` | Todos los resultados |
| `TotalProcesados` | `int` | Total procesado |
| `TotalValidos` | `int` | Cantidad de válidos |
| `TotalInvalidos` | `int` | Cantidad de inválidos |
| `TodosValidos` | `bool` | Si todos son válidos |
| `TodosInvalidos` | `bool` | Si todos son inválidos |
| `TieneValidos` | `bool` | Si hay algún válido |
| `TieneInvalidos` | `bool` | Si hay algún inválido |
| `PorcentajeValidos` | `double` | % válidos (0-100) |
| `PorcentajeInvalidos` | `double` | % inválidos (0-100) |
| `Resumen` | `string` | Resumen textual |

### `DocumentoValidado`

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `ValorOriginal` | `string` | Valor original |
| `ValorNormalizado` | `string` | Valor normalizado |
| `TipoDocumento` | `TipoDocumento` | Tipo de documento |
| `CaracterControl` | `string?` | Carácter de control |
| `ParteNumerica` | `string` | Parte numérica |

### `ErrorValidacion`

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Valor` | `string` | Valor original |
| `ValorNormalizado` | `string` | Valor normalizado |
| `Mensaje` | `string` | Descripción del error |
| `TipoDocumentoEsperado` | `TipoDocumento` | Tipo esperado |
| `CodigoError` | `string` | Código programático |

---

## Inyección de Dependencias

```csharp
// En Program.cs o Startup.cs
services.AddSingleton<IValidadorDocumentos, ValidadorDocumentos>();

// En tu servicio/controlador
public class MiServicio
{
    private readonly IValidadorDocumentos _validador;

    public MiServicio(IValidadorDocumentos validador)
    {
        _validador = validador;
    }

    public bool VerificarCliente(string documentoIdentidad)
    {
        return _validador.EsDocumentoValido(documentoIdentidad);
    }
}
```

---

## Algoritmos de Validación

### DNI
1. Formato: 8 dígitos + 1 letra
2. Letra = `"TRWAGMYFPDXBNJZSQVHLCKE"[número % 23]`

### NIE
1. Formato: X/Y/Z + 7 dígitos + 1 letra
2. Reemplazar X→0, Y→1, Z→2
3. Calcular letra como DNI

### NIF Especial (K/L/M)
1. Formato: K/L/M + 7 dígitos + 1 letra
2. Calcular letra como DNI sobre los 7 dígitos

### CIF
1. Formato: Letra tipo + 7 dígitos + control
2. Posiciones impares (1ª, 3ª, 5ª, 7ª): multiplicar por 2, sumar dígitos del resultado
3. Posiciones pares (2ª, 4ª, 6ª): sumar directamente
4. Control = (10 - (suma total % 10)) % 10
5. Según tipo: A/B/E/H→dígito, K/P/Q/S→letra, resto→ambos

---

## Licencia

MIT

## Autor

Cristian Arana Castiñeiras
