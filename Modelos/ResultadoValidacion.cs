namespace nif_dni_nie_cif_validation.Modelos
{
    /// <summary>
    /// Representa el resultado de la validación de un único documento.
    /// Contiene toda la información relevante sobre el proceso de validación.
    /// </summary>
    public class ResultadoValidacion
    {
        /// <summary>
        /// Indica si el documento es válido.
        /// </summary>
        public bool EsValido { get; }

        /// <summary>
        /// Valor original proporcionado para la validación (sin modificar).
        /// </summary>
        public string ValorOriginal { get; }

        /// <summary>
        /// Valor normalizado del documento (sin espacios, guiones, en mayúsculas).
        /// </summary>
        public string ValorNormalizado { get; }

        /// <summary>
        /// Tipo de documento detectado o esperado.
        /// </summary>
        public TipoDocumento TipoDocumento { get; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la validación.
        /// En caso de error, describe el motivo por el que es inválido.
        /// En caso de éxito, confirma la validez.
        /// </summary>
        public string Mensaje { get; }

        /// <summary>
        /// Letra o dígito de control del documento (si es válido).
        /// </summary>
        public string? CaracterControl { get; }

        /// <summary>
        /// Letra o dígito de control esperado según el algoritmo (útil para depuración).
        /// </summary>
        public string? CaracterControlEsperado { get; }

        /// <summary>
        /// Crea un resultado de validación exitoso.
        /// </summary>
        internal ResultadoValidacion(
            string valorOriginal,
            string valorNormalizado,
            TipoDocumento tipoDocumento,
            string mensaje,
            string? caracterControl,
            bool esValido)
        {
            EsValido = esValido;
            ValorOriginal = valorOriginal;
            ValorNormalizado = valorNormalizado;
            TipoDocumento = tipoDocumento;
            Mensaje = mensaje;
            CaracterControl = caracterControl;
            CaracterControlEsperado = caracterControl;
        }

        /// <summary>
        /// Crea un resultado de validación con todos los campos.
        /// </summary>
        internal ResultadoValidacion(
            string valorOriginal,
            string valorNormalizado,
            TipoDocumento tipoDocumento,
            string mensaje,
            string? caracterControl,
            string? caracterControlEsperado,
            bool esValido)
        {
            EsValido = esValido;
            ValorOriginal = valorOriginal;
            ValorNormalizado = valorNormalizado;
            TipoDocumento = tipoDocumento;
            Mensaje = mensaje;
            CaracterControl = caracterControl;
            CaracterControlEsperado = caracterControlEsperado;
        }

        /// <summary>
        /// Crea un resultado de validación exitoso.
        /// </summary>
        /// <param name="valorOriginal">Valor original proporcionado.</param>
        /// <param name="valorNormalizado">Valor normalizado.</param>
        /// <param name="tipoDocumento">Tipo de documento detectado.</param>
        /// <param name="caracterControl">Carácter de control.</param>
        /// <returns>Resultado de validación indicando éxito.</returns>
        public static ResultadoValidacion Valido(
            string valorOriginal,
            string valorNormalizado,
            TipoDocumento tipoDocumento,
            string? caracterControl = null)
        {
            string tipoTexto = tipoDocumento switch
            {
                TipoDocumento.DNI => "DNI",
                TipoDocumento.NIE => "NIE",
                TipoDocumento.NIF => "NIF",
                TipoDocumento.CIF => "CIF",
                _ => "Documento"
            };

            return new ResultadoValidacion(
                valorOriginal,
                valorNormalizado,
                tipoDocumento,
                $"El {tipoTexto} '{valorNormalizado}' es válido.",
                caracterControl,
                esValido: true);
        }

        /// <summary>
        /// Crea un resultado de validación fallido.
        /// </summary>
        /// <param name="valorOriginal">Valor original proporcionado.</param>
        /// <param name="valorNormalizado">Valor normalizado.</param>
        /// <param name="tipoDocumento">Tipo de documento esperado.</param>
        /// <param name="mensaje">Mensaje de error descriptivo.</param>
        /// <param name="caracterControlActual">Carácter de control encontrado.</param>
        /// <param name="caracterControlEsperado">Carácter de control esperado.</param>
        /// <returns>Resultado de validación indicando fallo.</returns>
        public static ResultadoValidacion Invalido(
            string valorOriginal,
            string valorNormalizado,
            TipoDocumento tipoDocumento,
            string mensaje,
            string? caracterControlActual = null,
            string? caracterControlEsperado = null)
        {
            return new ResultadoValidacion(
                valorOriginal,
                valorNormalizado,
                tipoDocumento,
                mensaje,
                caracterControlActual,
                caracterControlEsperado,
                esValido: false);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return EsValido
                ? $"[VÁLIDO] {TipoDocumento}: {ValorNormalizado}"
                : $"[INVÁLIDO] {TipoDocumento}: {ValorOriginal} - {Mensaje}";
        }
    }
}
