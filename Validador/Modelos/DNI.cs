using NIF.DNI.NIE.CIF.Validation.Excepciones;
using NIF.DNI.NIE.CIF.Validation.Utiles;

namespace NIF.DNI.NIE.CIF.Validation.Modelos
{
    /// <summary>
    /// Objeto de valor (Value Object) que representa un DNI (Documento Nacional de Identidad) español válido.
    /// Esta clase es inmutable y solo puede crearse a través de los métodos de fábrica.
    /// Un DNI tiene el formato: 8 dígitos + 1 letra de control (ej: 12345678Z).
    /// </summary>
    /// <example>
    /// <code>
    /// // Crear un DNI válido
    /// var dni = Dni.Crear("12345678Z");
    ///
    /// // Intentar crear sin excepción
    /// if (Dni.Intentar("12345678Z", out var dniValido))
    /// {
    ///     Console.WriteLine(dniValido.Valor); // "12345678Z"
    /// }
    ///
    /// // Conversión implícita a string
    /// string texto = dni;
    /// </code>
    /// </example>
    public sealed class Dni : ValueObject
    {
        /// <inheritdoc />
        public override TipoDocumento Tipo => TipoDocumento.DNI;

        /// <inheritdoc />
        public override string ParteNumerica { get; }

        /// <inheritdoc />
        public override string CaracterControl { get; }

        /// <summary>
        /// Número del DNI como entero.
        /// </summary>
        public int Numero { get; }

        /// <summary>
        /// Letra de control del DNI.
        /// </summary>
        public char Letra => CaracterControl[0];

        /// <summary>
        /// Constructor privado. Usar los métodos de fábrica <see cref="Crear"/> o <see cref="Intentar"/>.
        /// </summary>
        private Dni(string valor, string parteNumerica, int numero, char letra) : base(valor)
        {
            ParteNumerica = parteNumerica;
            Numero = numero;
            CaracterControl = letra.ToString();
        }

        /// <summary>
        /// Crea una instancia de <see cref="Dni"/> validando el valor proporcionado.
        /// Lanza una excepción si el DNI no es válido.
        /// </summary>
        /// <param name="valor">DNI a validar. Se normaliza automáticamente.</param>
        /// <returns>Instancia de <see cref="Dni"/> válida.</returns>
        /// <exception cref="DocumentoNoValidoException">Si el DNI no es válido.</exception>
        /// <example>
        /// <code>
        /// var dni = Dni.Crear("12345678Z");
        /// Console.WriteLine(dni.Numero); // 12345678
        /// Console.WriteLine(dni.Letra);  // 'Z'
        /// </code>
        /// </example>
        public static Dni Crear(string valor)
        {
            string normalizado = UtilesValidacion.Normalizar(valor);

            if (string.IsNullOrEmpty(normalizado))
                throw DocumentoNoValidoException.DniInvalido(valor ?? "",
                    "El DNI no puede ser nulo, vacío o contener solo espacios en blanco.");

            if (normalizado.Length != Constantes.LongitudDocumento)
                throw DocumentoNoValidoException.DniInvalido(valor,
                    $"El DNI debe tener exactamente {Constantes.LongitudDocumento} caracteres (8 dígitos + 1 letra). Se recibieron {normalizado.Length} caracteres.");

            string parteNum = normalizado[..8];
            char letraProporcionada = normalizado[8];

            if (!int.TryParse(parteNum, out int numero))
                throw DocumentoNoValidoException.DniInvalido(valor,
                    $"Los primeros 8 caracteres del DNI deben ser dígitos. Se recibió: '{parteNum}'.");

            if (!char.IsLetter(letraProporcionada))
                throw DocumentoNoValidoException.DniInvalido(valor,
                    $"El último carácter del DNI debe ser una letra. Se recibió: '{letraProporcionada}'.");

            char letraEsperada = UtilesValidacion.CalcularLetraDni(numero);
            if (letraProporcionada != letraEsperada)
                throw DocumentoNoValidoException.DniInvalido(valor,
                    $"La letra de control del DNI es incorrecta. Se esperaba '{letraEsperada}' pero se recibió '{letraProporcionada}'.");

            return new Dni(normalizado, parteNum, numero, letraEsperada);
        }

        /// <summary>
        /// Intenta crear una instancia de <see cref="Dni"/> sin lanzar excepciones.
        /// </summary>
        /// <param name="valor">DNI a validar.</param>
        /// <param name="resultado">Instancia de <see cref="Dni"/> si es válido, o null si no lo es.</param>
        /// <returns>true si el DNI es válido; false en caso contrario.</returns>
        /// <example>
        /// <code>
        /// if (Dni.Intentar("12345678Z", out var dni))
        /// {
        ///     Console.WriteLine($"DNI válido: {dni}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("DNI inválido");
        /// }
        /// </code>
        /// </example>
        public static bool Intentar(string valor, out Dni? resultado)
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

        /// <summary>
        /// Genera un DNI válido a partir de un número.
        /// </summary>
        /// <param name="numero">Número del DNI (0-99999999).</param>
        /// <returns>Instancia de <see cref="Dni"/> válida.</returns>
        /// <exception cref="ArgumentException">Si el número está fuera de rango.</exception>
        /// <example>
        /// <code>
        /// var dni = Dni.DesdeNumero(12345678);
        /// Console.WriteLine(dni.Valor); // "12345678Z"
        /// </code>
        /// </example>
        public static Dni DesdeNumero(int numero)
        {
            if (numero < 0 || numero > 99999999)
                throw new ArgumentException(
                    "El número del DNI debe estar entre 0 y 99.999.999.", nameof(numero));

            char letra = UtilesValidacion.CalcularLetraDni(numero);
            string parteNum = numero.ToString("D8");
            string valor = parteNum + letra;

            return new Dni(valor, parteNum, numero, letra);
        }
    }
}
