namespace NIF.DNI.NIE.CIF.Validation.Excepciones
{
    /// <summary>
    /// Excepción que se lanza cuando un documento de identificación no es válido.
    /// Contiene información detallada sobre el motivo del fallo de validación.
    /// </summary>
    public class DocumentoNoValidoException : Exception
    {
        /// <summary>
        /// Valor del documento que no pasó la validación.
        /// </summary>
        public string Valor { get; }

        /// <summary>
        /// Tipo de documento que se intentó validar.
        /// </summary>
        public Modelos.TipoDocumento TipoDocumento { get; }

        /// <summary>
        /// Código de error para identificar programáticamente el tipo de fallo.
        /// </summary>
        public string CodigoError { get; }

        /// <summary>
        /// Crea una nueva instancia de <see cref="DocumentoNoValidoException"/>.
        /// </summary>
        /// <param name="valor">Valor del documento inválido.</param>
        /// <param name="tipoDocumento">Tipo de documento esperado.</param>
        /// <param name="mensaje">Mensaje descriptivo del error.</param>
        /// <param name="codigoError">Código de error.</param>
        public DocumentoNoValidoException(
            string valor,
            Modelos.TipoDocumento tipoDocumento,
            string mensaje,
            string codigoError = "DOCUMENTO_NO_VALIDO")
            : base(mensaje)
        {
            Valor = valor;
            TipoDocumento = tipoDocumento;
            CodigoError = codigoError;
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="DocumentoNoValidoException"/> con excepción interna.
        /// </summary>
        /// <param name="valor">Valor del documento inválido.</param>
        /// <param name="tipoDocumento">Tipo de documento esperado.</param>
        /// <param name="mensaje">Mensaje descriptivo del error.</param>
        /// <param name="innerException">Excepción interna.</param>
        /// <param name="codigoError">Código de error.</param>
        public DocumentoNoValidoException(
            string valor,
            Modelos.TipoDocumento tipoDocumento,
            string mensaje,
            Exception innerException,
            string codigoError = "DOCUMENTO_NO_VALIDO")
            : base(mensaje, innerException)
        {
            Valor = valor;
            TipoDocumento = tipoDocumento;
            CodigoError = codigoError;
        }

        /// <summary>
        /// Crea una excepción específica para DNI inválido.
        /// </summary>
        public static DocumentoNoValidoException DniInvalido(string valor, string mensaje)
        {
            return new DocumentoNoValidoException(valor, Modelos.TipoDocumento.DNI, mensaje, "DNI_NO_VALIDO");
        }

        /// <summary>
        /// Crea una excepción específica para NIE inválido.
        /// </summary>
        public static DocumentoNoValidoException NieInvalido(string valor, string mensaje)
        {
            return new DocumentoNoValidoException(valor, Modelos.TipoDocumento.NIE, mensaje, "NIE_NO_VALIDO");
        }

        /// <summary>
        /// Crea una excepción específica para NIF inválido.
        /// </summary>
        public static DocumentoNoValidoException NifInvalido(string valor, string mensaje)
        {
            return new DocumentoNoValidoException(valor, Modelos.TipoDocumento.NIF, mensaje, "NIF_NO_VALIDO");
        }

        /// <summary>
        /// Crea una excepción específica para CIF inválido.
        /// </summary>
        public static DocumentoNoValidoException CifInvalido(string valor, string mensaje)
        {
            return new DocumentoNoValidoException(valor, Modelos.TipoDocumento.CIF, mensaje, "CIF_NO_VALIDO");
        }

        /// <summary>
        /// Crea una excepción para un documento cuyo tipo no se pudo determinar.
        /// </summary>
        public static DocumentoNoValidoException TipoDesconocido(string valor)
        {
            return new DocumentoNoValidoException(
                valor,
                Modelos.TipoDocumento.Desconocido,
                $"No se pudo determinar el tipo de documento para '{valor}'. No coincide con el formato de DNI, NIE ni CIF.",
                "TIPO_DESCONOCIDO");
        }

        /// <summary>
        /// Crea una excepción para un valor nulo o vacío.
        /// </summary>
        public static DocumentoNoValidoException ValorVacio()
        {
            return new DocumentoNoValidoException(
                string.Empty,
                Modelos.TipoDocumento.Desconocido,
                "El valor del documento no puede ser nulo, vacío o contener solo espacios en blanco.",
                "VALOR_VACIO");
        }
    }
}
