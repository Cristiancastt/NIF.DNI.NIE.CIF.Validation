namespace NIF.DNI.NIE.CIF.Validation.Modelos
{
    /// <summary>
    /// Representa un error producido durante la validación de un documento.
    /// Contiene información detallada sobre el motivo del fallo.
    /// </summary>
    public class ErrorValidacion
    {
        /// <summary>
        /// Valor original del documento que no pasó la validación.
        /// </summary>
        public string Valor { get; }

        /// <summary>
        /// Valor normalizado del documento (sin espacios, guiones, en mayúsculas).
        /// </summary>
        public string ValorNormalizado { get; }

        /// <summary>
        /// Mensaje descriptivo del error de validación.
        /// </summary>
        public string Mensaje { get; }

        /// <summary>
        /// Tipo de documento esperado (si se conoce).
        /// </summary>
        public TipoDocumento TipoDocumentoEsperado { get; }

        /// <summary>
        /// Código de error interno para identificar programáticamente el tipo de fallo.
        /// </summary>
        public string CodigoError { get; }

        /// <summary>
        /// Crea una nueva instancia de <see cref="ErrorValidacion"/>.
        /// </summary>
        /// <param name="valor">Valor original del documento.</param>
        /// <param name="valorNormalizado">Valor normalizado.</param>
        /// <param name="mensaje">Mensaje de error.</param>
        /// <param name="tipoDocumentoEsperado">Tipo de documento esperado.</param>
        /// <param name="codigoError">Código de error.</param>
        public ErrorValidacion(
            string valor,
            string valorNormalizado,
            string mensaje,
            TipoDocumento tipoDocumentoEsperado = TipoDocumento.Desconocido,
            string codigoError = "VALIDACION_FALLIDA")
        {
            Valor = valor;
            ValorNormalizado = valorNormalizado;
            Mensaje = mensaje;
            TipoDocumentoEsperado = tipoDocumentoEsperado;
            CodigoError = codigoError;
        }

        /// <summary>
        /// Crea un error a partir de un resultado de validación inválido.
        /// </summary>
        /// <param name="resultado">Resultado de validación que ha fallado.</param>
        /// <param name="codigoError">Código de error opcional.</param>
        /// <returns>Nueva instancia de <see cref="ErrorValidacion"/>.</returns>
        public static ErrorValidacion DesdeResultado(ResultadoValidacion resultado, string codigoError = "VALIDACION_FALLIDA")
        {
            return new ErrorValidacion(
                resultado.ValorOriginal,
                resultado.ValorNormalizado,
                resultado.Mensaje,
                resultado.TipoDocumento,
                codigoError);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{CodigoError}] {Valor}: {Mensaje}";
        }
    }
}
