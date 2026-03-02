using nif_dni_nie_cif_validation.Excepciones;
using nif_dni_nie_cif_validation.Utiles;

namespace nif_dni_nie_cif_validation.Modelos
{
    /// <summary>
    /// Objeto de valor (Value Object) que representa un NIF (Número de Identificación Fiscal) para personas físicas.
    /// El NIF puede ser un DNI, un NIE o un NIF especial (K, L, M).
    /// Esta clase es inmutable y solo puede crearse a través de los métodos de fábrica.
    /// </summary>
    /// <remarks>
    /// Formatos aceptados:
    /// <list type="bullet">
    /// <item><description>DNI: 8 dígitos + 1 letra (12345678Z)</description></item>
    /// <item><description>NIE: X/Y/Z + 7 dígitos + 1 letra (X1234567L)</description></item>
    /// <item><description>NIF especial K: Españoles menores de 14 años (K1234567A)</description></item>
    /// <item><description>NIF especial L: Españoles residentes en el extranjero (L1234567A)</description></item>
    /// <item><description>NIF especial M: Extranjeros sin NIE (M1234567A)</description></item>
    /// </list>
    /// </remarks>
    public sealed class Nif : ValueObject
    {
        /// <inheritdoc />
        public override TipoDocumento Tipo => TipoDocumento.NIF;

        /// <inheritdoc />
        public override string ParteNumerica { get; }

        /// <inheritdoc />
        public override string CaracterControl { get; }

        /// <summary>
        /// Subtipo específico del NIF (DNI, NIE o NIF especial).
        /// </summary>
        public TipoDocumento SubTipo { get; }

        /// <summary>
        /// Descripción del tipo de NIF (ej: "DNI", "NIE", "Español menor de 14 años sin DNI").
        /// </summary>
        public string DescripcionTipo { get; }

        /// <summary>
        /// Constructor privado.
        /// </summary>
        private Nif(string valor, string parteNumerica, string caracterControl, TipoDocumento subTipo, string descripcionTipo) 
            : base(valor)
        {
            ParteNumerica = parteNumerica;
            CaracterControl = caracterControl;
            SubTipo = subTipo;
            DescripcionTipo = descripcionTipo;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Nif"/> validando el valor proporcionado.
        /// Acepta DNI, NIE y NIF especiales (K, L, M).
        /// Lanza una excepción si el NIF no es válido.
        /// </summary>
        /// <param name="valor">NIF a validar. Se normaliza automáticamente.</param>
        /// <returns>Instancia de <see cref="Nif"/> válida.</returns>
        /// <exception cref="DocumentoNoValidoException">Si el NIF no es válido.</exception>
        public static Nif Crear(string valor)
        {
            string normalizado = UtilesValidacion.Normalizar(valor);

            if (string.IsNullOrEmpty(normalizado))
                throw DocumentoNoValidoException.NifInvalido(valor ?? "",
                    "El NIF no puede ser nulo, vacío o contener solo espacios en blanco.");

            if (normalizado.Length != 9)
                throw DocumentoNoValidoException.NifInvalido(valor,
                    $"El NIF debe tener exactamente 9 caracteres. Se recibieron {normalizado.Length} caracteres.");

            char primerCaracter = normalizado[0];

            // ── DNI (empieza por dígito)
            if (char.IsAsciiDigit(primerCaracter))
            {
                var dni = Dni.Crear(valor);
                return new Nif(dni.Valor, dni.ParteNumerica, dni.CaracterControl, TipoDocumento.DNI, "Documento Nacional de Identidad");
            }

            // ── NIE (empieza por X, Y o Z)
            if (Constantes.LetrasNie.Contains(primerCaracter))
            {
                var nie = Nie.Crear(valor);
                return new Nif(nie.Valor, nie.ParteNumerica, nie.CaracterControl, TipoDocumento.NIE, "Número de Identidad de Extranjero");
            }

            // ── NIF especial (empieza por K, L o M)
            if (Constantes.LetrasNifEspecial.Contains(primerCaracter))
            {
                string parteNum = normalizado.Substring(1, 7);
                if (parteNum.AsSpan().ContainsAnyExcept(Constantes.Digitos))
                    throw DocumentoNoValidoException.NifInvalido(valor,
                        $"Los caracteres 2 al 8 del NIF especial deben ser dígitos. Se recibió: '{parteNum}'.");

                char letraProporcionada = normalizado[8];
                if (!char.IsLetter(letraProporcionada))
                    throw DocumentoNoValidoException.NifInvalido(valor,
                        $"El último carácter del NIF especial debe ser una letra. Se recibió: '{letraProporcionada}'.");

                char letraEsperada = UtilesValidacion.CalcularLetraNifEspecial(normalizado);
                if (letraProporcionada != letraEsperada)
                    throw DocumentoNoValidoException.NifInvalido(valor,
                        $"La letra de control del NIF especial es incorrecta. Se esperaba '{letraEsperada}' pero se recibió '{letraProporcionada}'.");

                string descripcion = UtilesValidacion.ObtenerDescripcionNifEspecial(primerCaracter);
                return new Nif(normalizado, parteNum, letraEsperada.ToString(), TipoDocumento.NIF, descripcion);
            }

            throw DocumentoNoValidoException.NifInvalido(valor,
                $"El NIF debe empezar por un dígito (DNI), por X/Y/Z (NIE) o por K/L/M (NIF especial). Se recibió: '{primerCaracter}'.");
        }

        /// <summary>
        /// Intenta crear una instancia de <see cref="Nif"/> sin lanzar excepciones.
        /// </summary>
        /// <param name="valor">NIF a validar.</param>
        /// <param name="resultado">Instancia de <see cref="Nif"/> si es válido, o null si no lo es.</param>
        /// <returns>true si el NIF es válido; false en caso contrario.</returns>
        public static bool Intentar(string valor, out Nif? resultado)
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
