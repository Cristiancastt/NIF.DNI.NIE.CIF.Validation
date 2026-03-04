using System.Buffers;
using System.Collections.Frozen;

namespace NIF.DNI.NIE.CIF.Validation.Utiles
{
    /// <summary>
    /// Constantes utilizadas en la validación de documentos de identificación españoles.
    /// Usa <see cref="SearchValues{T}"/> para búsquedas O(1) de caracteres,
    /// <see cref="FrozenDictionary{TKey, TValue}"/> para diccionarios inmutables de alto rendimiento
    /// y <see cref="FrozenSet{T}"/> para conjuntos congelados optimizados en lectura.
    /// Todas las estructuras son thread-safe por inmutabilidad.
    /// </summary>
    public static class Constantes
    {
        /// <summary>
        /// Tabla de letras para el cálculo del dígito de control del DNI/NIE.
        /// La letra se obtiene como: LETRAS_DNI[número % 23].
        /// </summary>
        public const string LetrasDni = "TRWAGMYFPDXBNJZSQVHLCKE";

        /// <summary>
        /// Divisor utilizado para calcular la letra de control del DNI/NIE (módulo 23).
        /// </summary>
        public const int ModuloDni = 23;

        /// <summary>
        /// Longitud esperada de cualquier documento (DNI, NIE, CIF, NIF): siempre 9 caracteres.
        /// </summary>
        public const int LongitudDocumento = 9;

        /// <summary>
        /// Letras para el dígito de control del CIF cuando es letra.
        /// Control 0=J, 1=A, 2=B, 3=C, 4=D, 5=E, 6=F, 7=G, 8=H, 9=I.
        /// </summary>
        public const string LetrasControlCif = "JABCDEFGHI";

        // ─── SearchValues<char>: búsquedas de caracteres en O(1) con SIMD ──────

        /// <summary>
        /// Letras iniciales válidas para un NIE (X, Y, Z). Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> LetrasNie = SearchValues.Create("XYZ");

        /// <summary>
        /// Letras iniciales válidas para NIF especiales (K, L, M). Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> LetrasNifEspecial = SearchValues.Create("KLM");

        /// <summary>
        /// Letras iniciales válidas para un CIF. Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> LetrasCif = SearchValues.Create("ABCDEFGHJNPQRSUVW");

        /// <summary>
        /// Tipos de CIF cuyo dígito de control debe ser numérico. Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> CifControlNumerico = SearchValues.Create("ABEH");

        /// <summary>
        /// Tipos de CIF cuyo dígito de control debe ser una letra. Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> CifControlLetra = SearchValues.Create("KPQS");

        /// <summary>
        /// Caracteres no deseados a eliminar durante la normalización. Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> CaracteresNoDeseados = SearchValues.Create(" \t\r\n-.,/\\");

        /// <summary>
        /// Dígitos 0-9 para validación rápida de caracteres numéricos. Búsqueda vectorizada O(1).
        /// </summary>
        public static readonly SearchValues<char> Digitos = SearchValues.Create("0123456789");

        // ─── FrozenDictionary: diccionarios congelados inmutables, O(1) por clave ─

        /// <summary>
        /// Descripciones de los tipos de entidad del CIF según su primera letra.
        /// Inmutable y thread-safe. Lectura O(1).
        /// </summary>
        public static readonly FrozenDictionary<char, string> DescripcionesTipoCif =
            new Dictionary<char, string>
            {
                ['A'] = "Sociedad Anónima",
                ['B'] = "Sociedad de Responsabilidad Limitada",
                ['C'] = "Sociedad Colectiva",
                ['D'] = "Sociedad Comanditaria",
                ['E'] = "Comunidad de Bienes",
                ['F'] = "Sociedad Cooperativa",
                ['G'] = "Asociación",
                ['H'] = "Comunidad de Propietarios",
                ['J'] = "Sociedad Civil",
                ['N'] = "Entidad Extranjera",
                ['P'] = "Corporación Local",
                ['Q'] = "Organismo Público",
                ['R'] = "Congregación Religiosa",
                ['S'] = "Órgano de la Administración del Estado",
                ['U'] = "Unión Temporal de Empresas",
                ['V'] = "Otro tipo no definido",
                ['W'] = "Establecimiento permanente de entidad no residente"
            }.ToFrozenDictionary();

        /// <summary>
        /// Descripciones de los tipos de NIF especial según su primera letra.
        /// Inmutable y thread-safe. Lectura O(1).
        /// </summary>
        public static readonly FrozenDictionary<char, string> DescripcionesNifEspecial =
            new Dictionary<char, string>
            {
                ['K'] = "Español menor de 14 años sin DNI",
                ['L'] = "Español residente en el extranjero",
                ['M'] = "Extranjero sin NIE"
            }.ToFrozenDictionary();
    }
}
