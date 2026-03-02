using nif_dni_nie_cif_validation.Excepciones;
using nif_dni_nie_cif_validation.Utiles;

namespace nif_dni_nie_cif_validation.Modelos
{
    /// <summary>
    /// Objeto de valor (Value Object) que representa un NIE (Número de Identidad de Extranjero) español válido.
    /// Esta clase es inmutable y solo puede crearse a través de los métodos de fábrica.
    /// Un NIE tiene el formato: 1 letra (X, Y o Z) + 7 dígitos + 1 letra de control (ej: X1234567L).
    /// </summary>
    /// <example>
    /// <code>
    /// // Crear un NIE válido
    /// var nie = Nie.Crear("X1234567L");
    ///
    /// // Intentar crear sin excepción
    /// if (Nie.Intentar("X1234567L", out var nieValido))
    /// {
    ///     Console.WriteLine(nieValido.LetraInicial); // 'X'
    /// }
    /// </code>
    /// </example>
    public sealed class Nie : ValueObject
    {
        /// <inheritdoc />
        public override TipoDocumento Tipo => TipoDocumento.NIE;

        /// <inheritdoc />
        public override string ParteNumerica { get; }

        /// <inheritdoc />
        public override string CaracterControl { get; }

        /// <summary>
        /// Letra inicial del NIE (X, Y o Z).
        /// </summary>
        public char LetraInicial { get; }

        /// <summary>
        /// Letra de control del NIE.
        /// </summary>
        public char LetraControl => CaracterControl[0];

        /// <summary>
        /// Constructor privado.
        /// </summary>
        private Nie(string valor, string parteNumerica, char letraInicial, char letraControl) : base(valor)
        {
            ParteNumerica = parteNumerica;
            LetraInicial = letraInicial;
            CaracterControl = letraControl.ToString();
        }

        /// <summary>
        /// Crea una instancia de <see cref="Nie"/> validando el valor proporcionado.
        /// Lanza una excepción si el NIE no es válido.
        /// </summary>
        /// <param name="valor">NIE a validar. Se normaliza automáticamente.</param>
        /// <returns>Instancia de <see cref="Nie"/> válida.</returns>
        /// <exception cref="DocumentoNoValidoException">Si el NIE no es válido.</exception>
        public static Nie Crear(string valor)
        {
            string normalizado = UtilesValidacion.Normalizar(valor);

            if (string.IsNullOrEmpty(normalizado))
                throw DocumentoNoValidoException.NieInvalido(valor ?? "",
                    "El NIE no puede ser nulo, vacío o contener solo espacios en blanco.");

            if (normalizado.Length != Constantes.LongitudDocumento)
                throw DocumentoNoValidoException.NieInvalido(valor,
                    $"El NIE debe tener exactamente {Constantes.LongitudDocumento} caracteres (1 letra + 7 dígitos + 1 letra). Se recibieron {normalizado.Length} caracteres.");

            char letraInicial = normalizado[0];
            if (!Constantes.LetrasNie.Contains(letraInicial))
                throw DocumentoNoValidoException.NieInvalido(valor,
                    $"La primera letra del NIE debe ser X, Y o Z. Se recibió: '{letraInicial}'.");

            string parteNum = normalizado.Substring(1, 7);
            if (parteNum.AsSpan().ContainsAnyExcept(Constantes.Digitos))
                throw DocumentoNoValidoException.NieInvalido(valor,
                    $"Los caracteres 2 al 8 del NIE deben ser dígitos. Se recibió: '{parteNum}'.");

            char letraProporcionada = normalizado[8];
            if (!char.IsLetter(letraProporcionada))
                throw DocumentoNoValidoException.NieInvalido(valor,
                    $"El último carácter del NIE debe ser una letra. Se recibió: '{letraProporcionada}'.");

            char letraEsperada = UtilesValidacion.CalcularLetraNie(normalizado);
            if (letraProporcionada != letraEsperada)
                throw DocumentoNoValidoException.NieInvalido(valor,
                    $"La letra de control del NIE es incorrecta. Se esperaba '{letraEsperada}' pero se recibió '{letraProporcionada}'.");

            return new Nie(normalizado, parteNum, letraInicial, letraEsperada);
        }

        /// <summary>
        /// Intenta crear una instancia de <see cref="Nie"/> sin lanzar excepciones.
        /// </summary>
        /// <param name="valor">NIE a validar.</param>
        /// <param name="resultado">Instancia de <see cref="Nie"/> si es válido, o null si no lo es.</param>
        /// <returns>true si el NIE es válido; false en caso contrario.</returns>
        public static bool Intentar(string valor, out Nie? resultado)
        {
            try
            {
                resultado = Crear(valor);
                return true;
            }
            catch
            {
                resultado = null;
                return false;
            }
        }
    }
}
