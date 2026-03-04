using System.Buffers;
using System.Runtime.CompilerServices;

namespace NIF.DNI.NIE.CIF.Validation.Utiles
{
    /// <summary>
    /// Clase estática con métodos auxiliares para la validación y manipulación
    /// de documentos de identificación españoles.
    /// Optimizada con <see cref="ReadOnlySpan{T}"/>, <see cref="SearchValues{T}"/> (SIMD)
    /// y operaciones sin asignación de heap cuando es posible.
    /// Todos los métodos son thread-safe por ser estáticos y sin estado.
    /// </summary>
    public static partial class UtilesValidacion
    {
        /// <summary>
        /// Normaliza un documento eliminando espacios, guiones, puntos y convirtiéndolo a mayúsculas.
        /// Usa <see cref="SearchValues{T}"/> para detección vectorizada O(1) por carácter.
        /// Minimiza asignaciones de heap usando <c>stackalloc</c> para documentos cortos.
        /// </summary>
        /// <param name="documento">Documento a normalizar.</param>
        /// <returns>Documento normalizado o cadena vacía si es nulo.</returns>
        /// <example>
        /// <code>
        /// string normalizado = UtilesValidacion.Normalizar("12.345.678-z");
        /// // Resultado: "12345678Z"
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Normalizar(string? documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return string.Empty;

            ReadOnlySpan<char> entrada = documento.AsSpan();

            // Si no contiene caracteres no deseados, solo convertir a mayúsculas
            if (!entrada.ContainsAny(Constantes.CaracteresNoDeseados))
                return string.Create(entrada.Length, documento, static (destino, src) =>
                {
                    src.AsSpan().ToUpperInvariant(destino);
                });

            // Máximo posible: longitud original. Usar stack para <= 64 chars
            Span<char> buffer = entrada.Length <= 64
                ? stackalloc char[64]
                : new char[entrada.Length];

            int pos = 0;
            foreach (char c in entrada)
            {
                if (!Constantes.CaracteresNoDeseados.Contains(c))
                {
                    buffer[pos++] = char.ToUpperInvariant(c);
                }
            }

            return new string(buffer[..pos]);
        }

        /// <summary>
        /// Detecta el tipo de documento basándose en su formato.
        /// Usa <see cref="SearchValues{T}"/> para búsquedas de caracteres en O(1) con SIMD.
        /// Validación inline sin regex para máximo rendimiento.
        /// </summary>
        /// <param name="documento">Documento a analizar (se normalizará internamente).</param>
        /// <returns>Tipo de documento detectado.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Modelos.TipoDocumento DetectarTipoDocumento(string? documento)
        {
            string normalizado = Normalizar(documento);

            if (normalizado.Length != Constantes.LongitudDocumento)
                return Modelos.TipoDocumento.Desconocido;

            ReadOnlySpan<char> span = normalizado.AsSpan();
            char primerCaracter = span[0];

            // DNI: empieza por dígito, 8 dígitos + 1 letra
            if (char.IsAsciiDigit(primerCaracter))
            {
                return EsFormatoDni(span) ? Modelos.TipoDocumento.DNI : Modelos.TipoDocumento.Desconocido;
            }

            // NIE: empieza por X, Y o Z
            if (Constantes.LetrasNie.Contains(primerCaracter))
            {
                return EsFormatoNie(span) ? Modelos.TipoDocumento.NIE : Modelos.TipoDocumento.Desconocido;
            }

            // NIF especial: empieza por K, L o M
            if (Constantes.LetrasNifEspecial.Contains(primerCaracter))
            {
                return EsFormatoNifEspecial(span) ? Modelos.TipoDocumento.NIF : Modelos.TipoDocumento.Desconocido;
            }

            // CIF: empieza por letra válida de CIF
            if (Constantes.LetrasCif.Contains(primerCaracter))
            {
                return EsFormatoCif(span) ? Modelos.TipoDocumento.CIF : Modelos.TipoDocumento.Desconocido;
            }

            return Modelos.TipoDocumento.Desconocido;
        }

        /// <summary>
        /// Calcula la letra de control de un DNI dado su número.
        /// Operación O(1) con aritmética modular.
        /// </summary>
        /// <param name="numero">Número del DNI (8 dígitos).</param>
        /// <returns>Letra de control correspondiente.</returns>
        /// <exception cref="ArgumentException">Si el número no es un entero válido de 0 a 99999999.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char CalcularLetraDni(int numero)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(numero);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(numero, 99999999);

            return Constantes.LetrasDni[numero % Constantes.ModuloDni];
        }

        /// <summary>
        /// Calcula la letra de control de un DNI dado su número como cadena.
        /// </summary>
        /// <param name="numeroTexto">Número del DNI como cadena de texto.</param>
        /// <returns>Letra de control correspondiente.</returns>
        /// <exception cref="ArgumentException">Si la cadena no representa un número válido.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char CalcularLetraDni(string numeroTexto)
        {
            if (!int.TryParse(numeroTexto, System.Globalization.NumberStyles.None, null, out int numero))
                throw new ArgumentException(
                    $"'{numeroTexto}' no es un número válido para calcular la letra del DNI.", nameof(numeroTexto));

            return CalcularLetraDni(numero);
        }

        /// <summary>
        /// Calcula la letra de control de un NIE.
        /// Usa <see cref="ReadOnlySpan{T}"/> para evitar asignaciones de subcadenas.
        /// </summary>
        /// <param name="nie">NIE completo o parcial (X/Y/Z + 7 dígitos).</param>
        /// <returns>Letra de control correspondiente.</returns>
        /// <exception cref="ArgumentException">Si el NIE no tiene un formato válido.</exception>
        public static char CalcularLetraNie(string nie)
        {
            string normalizado = Normalizar(nie);

            if (normalizado.Length < 8)
                throw new ArgumentException(
                    "El NIE debe tener al menos 8 caracteres (letra inicial + 7 dígitos).", nameof(nie));

            ReadOnlySpan<char> span = normalizado.AsSpan();
            char letraInicial = span[0];

            int prefijo = letraInicial switch
            {
                'X' => 0,
                'Y' => 1,
                'Z' => 2,
                _ => throw new ArgumentException(
                    $"La letra inicial del NIE debe ser X, Y o Z. Se recibió: '{letraInicial}'.", nameof(nie))
            };

            // Parsear los 7 dígitos directamente con Span sin crear substring
            if (!int.TryParse(span.Slice(1, 7), System.Globalization.NumberStyles.None, null, out int parteNum))
                throw new ArgumentException(
                    $"La parte numérica del NIE no es válida.", nameof(nie));

            int numero = prefijo * 10_000_000 + parteNum;
            return Constantes.LetrasDni[numero % Constantes.ModuloDni];
        }

        /// <summary>
        /// Calcula la letra de control de un NIF especial (K, L o M).
        /// </summary>
        /// <param name="nif">NIF especial (K/L/M + 7 dígitos).</param>
        /// <returns>Letra de control correspondiente.</returns>
        /// <exception cref="ArgumentException">Si el NIF no tiene un formato válido.</exception>
        public static char CalcularLetraNifEspecial(string nif)
        {
            string normalizado = Normalizar(nif);

            if (normalizado.Length < 8)
                throw new ArgumentException(
                    "El NIF especial debe tener al menos 8 caracteres (letra inicial + 7 dígitos).", nameof(nif));

            ReadOnlySpan<char> span = normalizado.AsSpan();
            char letraInicial = span[0];

            if (!Constantes.LetrasNifEspecial.Contains(letraInicial))
                throw new ArgumentException(
                    $"La letra inicial del NIF especial debe ser K, L o M. Se recibió: '{letraInicial}'.", nameof(nif));

            if (!int.TryParse(span.Slice(1, 7), System.Globalization.NumberStyles.None, null, out int numero))
                throw new ArgumentException(
                    $"La parte numérica del NIF especial no es válida.", nameof(nif));

            return Constantes.LetrasDni[numero % Constantes.ModuloDni];
        }

        /// <summary>
        /// Calcula el dígito o letra de control de un CIF.
        /// Usa aritmética directa sobre <see cref="ReadOnlySpan{T}"/> sin crear subcadenas.
        /// </summary>
        /// <param name="cif">CIF completo o parcial (letra + 7 dígitos).</param>
        /// <returns>
        /// Tupla con el dígito de control numérico y su equivalente en letra.
        /// </returns>
        /// <exception cref="ArgumentException">Si el CIF no tiene un formato válido.</exception>
        public static (char Digito, char Letra) CalcularControlCif(string cif)
        {
            string normalizado = Normalizar(cif);

            if (normalizado.Length < 8)
                throw new ArgumentException(
                    "El CIF debe tener al menos 8 caracteres (letra + 7 dígitos).", nameof(cif));

            ReadOnlySpan<char> span = normalizado.AsSpan();

            // Validar que posiciones 1-7 sean dígitos y calcular la suma directamente
            int sumaPares = 0;
            int sumaImpares = 0;

            for (int i = 1; i <= 7; i++)
            {
                char c = span[i];
                if (!char.IsAsciiDigit(c))
                    throw new ArgumentException(
                        $"Los caracteres en posiciones 2 a 8 del CIF deben ser dígitos.", nameof(cif));

                int valorDigito = c - '0';

                if ((i - 1) % 2 == 0) // Posiciones impares del CIF (índices 1, 3, 5, 7 del span → i-1 = 0, 2, 4, 6)
                {
                    int doble = valorDigito * 2;
                    sumaImpares += (doble / 10) + (doble % 10);
                }
                else // Posiciones pares del CIF
                {
                    sumaPares += valorDigito;
                }
            }

            int control = (10 - ((sumaPares + sumaImpares) % 10)) % 10;

            return ((char)('0' + control), Constantes.LetrasControlCif[control]);
        }

        /// <summary>
        /// Determina si el dígito de control del CIF debe ser numérico, letra o ambos.
        /// Búsqueda O(1) con <see cref="SearchValues{T}"/>.
        /// </summary>
        /// <param name="letraInicial">Primera letra del CIF.</param>
        /// <returns>"digito", "letra" o "ambos".</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ObtenerTipoControlCif(char letraInicial)
        {
            letraInicial = char.ToUpperInvariant(letraInicial);

            if (Constantes.CifControlNumerico.Contains(letraInicial))
                return "digito";
            if (Constantes.CifControlLetra.Contains(letraInicial))
                return "letra";
            return "ambos";
        }

        /// <summary>
        /// Obtiene la descripción del tipo de entidad del CIF según su primera letra.
        /// Lectura O(1) con <see cref="System.Collections.Frozen.FrozenDictionary{TKey, TValue}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ObtenerDescripcionTipoCif(char letraInicial)
        {
            letraInicial = char.ToUpperInvariant(letraInicial);
            return Constantes.DescripcionesTipoCif.TryGetValue(letraInicial, out string? desc)
                ? desc
                : "Tipo de entidad desconocido";
        }

        /// <summary>
        /// Obtiene la descripción del tipo de NIF especial según su primera letra.
        /// Lectura O(1) con <see cref="System.Collections.Frozen.FrozenDictionary{TKey, TValue}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ObtenerDescripcionNifEspecial(char letraInicial)
        {
            letraInicial = char.ToUpperInvariant(letraInicial);
            return Constantes.DescripcionesNifEspecial.TryGetValue(letraInicial, out string? desc)
                ? desc
                : "Tipo de NIF especial desconocido";
        }

        /// <summary>
        /// Extrae la parte numérica de un documento de identificación.
        /// Usa <c>stackalloc</c> para evitar asignaciones en heap.
        /// </summary>
        /// <param name="documento">Documento normalizado.</param>
        /// <returns>Parte numérica del documento.</returns>
        public static string ExtraerParteNumerica(string documento)
        {
            ReadOnlySpan<char> span = documento.AsSpan();
            Span<char> buffer = span.Length <= 16 ? stackalloc char[16] : new char[span.Length];
            int pos = 0;

            foreach (char c in span)
            {
                if (char.IsAsciiDigit(c))
                    buffer[pos++] = c;
            }

            return new string(buffer[..pos]);
        }

        // ─── Validación de formato inline (sin regex) ──────────────────────────────

        /// <summary>
        /// Valida formato DNI: 8 dígitos + 1 letra. Sin regex, O(n) con n=9 fijo → O(1) amortizado.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EsFormatoDni(ReadOnlySpan<char> span)
        {
            // span.Length == 9 ya verificado por el llamador
            for (int i = 0; i < 8; i++)
            {
                if (!char.IsAsciiDigit(span[i]))
                    return false;
            }
            return char.IsAsciiLetter(span[8]);
        }

        /// <summary>
        /// Valida formato NIE: X/Y/Z + 7 dígitos + 1 letra. Sin regex, O(1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EsFormatoNie(ReadOnlySpan<char> span)
        {
            // span[0] ya verificado como X/Y/Z
            for (int i = 1; i < 8; i++)
            {
                if (!char.IsAsciiDigit(span[i]))
                    return false;
            }
            return char.IsAsciiLetter(span[8]);
        }

        /// <summary>
        /// Valida formato NIF especial: K/L/M + 7 dígitos + 1 letra. Sin regex, O(1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EsFormatoNifEspecial(ReadOnlySpan<char> span)
        {
            // span[0] ya verificado como K/L/M
            for (int i = 1; i < 8; i++)
            {
                if (!char.IsAsciiDigit(span[i]))
                    return false;
            }
            return char.IsAsciiLetter(span[8]);
        }

        /// <summary>
        /// Valida formato CIF: letra válida + 7 dígitos + 1 dígito/letra. Sin regex, O(1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EsFormatoCif(ReadOnlySpan<char> span)
        {
            // span[0] ya verificado como letra CIF válida
            for (int i = 1; i < 8; i++)
            {
                if (!char.IsAsciiDigit(span[i]))
                    return false;
            }
            char ultimo = span[8];
            return char.IsAsciiDigit(ultimo) || char.IsAsciiLetter(ultimo);
        }
    }
}
