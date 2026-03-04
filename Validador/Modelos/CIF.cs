using NIF.DNI.NIE.CIF.Validation.Excepciones;
using NIF.DNI.NIE.CIF.Validation.Utiles;

namespace NIF.DNI.NIE.CIF.Validation.Modelos
{
    /// <summary>
    /// Objeto de valor (Value Object) que representa un CIF (Código de Identificación Fiscal) español válido.
    /// El CIF identifica a personas jurídicas (empresas, asociaciones, etc.).
    /// Esta clase es inmutable y solo puede crearse a través de los métodos de fábrica.
    /// Formato: 1 letra + 7 dígitos + 1 dígito/letra de control (ej: B12345678).
    /// </summary>
    /// <example>
    /// <code>
    /// // Crear un CIF válido
    /// var cif = Cif.Crear("B12345678");
    ///
    /// // Obtener el tipo de sociedad
    /// Console.WriteLine(cif.TipoEntidad); // "Sociedad de Responsabilidad Limitada"
    /// </code>
    /// </example>
    public sealed class Cif : ValueObject
    {
        /// <inheritdoc />
        public override TipoDocumento Tipo => TipoDocumento.CIF;

        /// <inheritdoc />
        public override string ParteNumerica { get; }

        /// <inheritdoc />
        public override string CaracterControl { get; }

        /// <summary>
        /// Letra que identifica el tipo de entidad.
        /// </summary>
        public char LetraTipoEntidad { get; }

        /// <summary>
        /// Descripción del tipo de entidad según la primera letra del CIF.
        /// </summary>
        public string TipoEntidad { get; }

        /// <summary>
        /// Código de provincia (los dos primeros dígitos del CIF).
        /// </summary>
        public string CodigoProvincia { get; }

        /// <summary>
        /// Constructor privado.
        /// </summary>
        private Cif(string valor, string parteNumerica, string caracterControl, char letraTipo, string tipoEntidad, string codigoProvincia)
            : base(valor)
        {
            ParteNumerica = parteNumerica;
            CaracterControl = caracterControl;
            LetraTipoEntidad = letraTipo;
            TipoEntidad = tipoEntidad;
            CodigoProvincia = codigoProvincia;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Cif"/> validando el valor proporcionado.
        /// Lanza una excepción si el CIF no es válido.
        /// </summary>
        /// <param name="valor">CIF a validar. Se normaliza automáticamente.</param>
        /// <returns>Instancia de <see cref="Cif"/> válida.</returns>
        /// <exception cref="DocumentoNoValidoException">Si el CIF no es válido.</exception>
        public static Cif Crear(string valor)
        {
            string normalizado = UtilesValidacion.Normalizar(valor);

            if (string.IsNullOrEmpty(normalizado))
                throw DocumentoNoValidoException.CifInvalido(valor ?? "",
                    "El CIF no puede ser nulo, vacío o contener solo espacios en blanco.");

            if (normalizado.Length != Constantes.LongitudDocumento)
                throw DocumentoNoValidoException.CifInvalido(valor,
                    $"El CIF debe tener exactamente {Constantes.LongitudDocumento} caracteres (1 letra + 7 dígitos + 1 control). Se recibieron {normalizado.Length} caracteres.");

            char letraTipo = normalizado[0];
            if (!Constantes.LetrasCif.Contains(letraTipo))
                throw DocumentoNoValidoException.CifInvalido(valor,
                    $"La primera letra del CIF debe ser una de las siguientes: ABCDEFGHJNPQRSUVW. Se recibió: '{letraTipo}'.");

            string parteNum = normalizado.Substring(1, 7);
            if (parteNum.AsSpan().ContainsAnyExcept(Constantes.Digitos))
                throw DocumentoNoValidoException.CifInvalido(valor,
                    $"Los caracteres 2 al 8 del CIF deben ser dígitos. Se recibió: '{parteNum}'.");

            char controlProporcionado = normalizado[8];
            var (digitoEsperado, letraEsperada) = UtilesValidacion.CalcularControlCif(normalizado);
            string tipoControl = UtilesValidacion.ObtenerTipoControlCif(letraTipo);

            bool esValido = tipoControl switch
            {
                "digito" => controlProporcionado == digitoEsperado,
                "letra" => controlProporcionado == letraEsperada,
                "ambos" => controlProporcionado == digitoEsperado || controlProporcionado == letraEsperada,
                _ => false
            };

            if (!esValido)
            {
                string esperado = tipoControl switch
                {
                    "digito" => $"'{digitoEsperado}' (dígito)",
                    "letra" => $"'{letraEsperada}' (letra)",
                    "ambos" => $"'{digitoEsperado}' (dígito) o '{letraEsperada}' (letra)",
                    _ => "desconocido"
                };
                throw DocumentoNoValidoException.CifInvalido(valor,
                    $"El carácter de control del CIF es incorrecto. Se esperaba {esperado} pero se recibió '{controlProporcionado}'.");
            }

            string tipoEntidad = UtilesValidacion.ObtenerDescripcionTipoCif(letraTipo);
            string codigoProvincia = parteNum[..2];

            return new Cif(normalizado, parteNum, controlProporcionado.ToString(), letraTipo, tipoEntidad, codigoProvincia);
        }

        /// <summary>
        /// Intenta crear una instancia de <see cref="Cif"/> sin lanzar excepciones.
        /// </summary>
        /// <param name="valor">CIF a validar.</param>
        /// <param name="resultado">Instancia de <see cref="Cif"/> si es válido, o null si no lo es.</param>
        /// <returns>true si el CIF es válido; false en caso contrario.</returns>
        public static bool Intentar(string valor, out Cif? resultado)
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
