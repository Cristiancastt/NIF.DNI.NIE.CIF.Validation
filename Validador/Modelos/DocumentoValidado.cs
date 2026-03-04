namespace NIF.DNI.NIE.CIF.Validation.Modelos
{
    /// <summary>
    /// Representa un documento que ha sido validado exitosamente.
    /// Contiene toda la información del documento validado.
    /// </summary>
    public class DocumentoValidado
    {
        /// <summary>
        /// Valor original del documento tal y como fue proporcionado.
        /// </summary>
        public string ValorOriginal { get; }

        /// <summary>
        /// Valor normalizado del documento (sin espacios, guiones, en mayúsculas).
        /// </summary>
        public string ValorNormalizado { get; }

        /// <summary>
        /// Tipo de documento identificado.
        /// </summary>
        public TipoDocumento TipoDocumento { get; }

        /// <summary>
        /// Letra o dígito de control del documento.
        /// </summary>
        public string? CaracterControl { get; }

        /// <summary>
        /// Parte numérica del documento.
        /// </summary>
        public string ParteNumerica { get; }

        /// <summary>
        /// Crea una nueva instancia de <see cref="DocumentoValidado"/>.
        /// </summary>
        /// <param name="valorOriginal">Valor original del documento.</param>
        /// <param name="valorNormalizado">Valor normalizado.</param>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="caracterControl">Carácter de control.</param>
        /// <param name="parteNumerica">Parte numérica del documento.</param>
        public DocumentoValidado(
            string valorOriginal,
            string valorNormalizado,
            TipoDocumento tipoDocumento,
            string? caracterControl,
            string parteNumerica)
        {
            ValorOriginal = valorOriginal;
            ValorNormalizado = valorNormalizado;
            TipoDocumento = tipoDocumento;
            CaracterControl = caracterControl;
            ParteNumerica = parteNumerica;
        }

        /// <summary>
        /// Crea un <see cref="DocumentoValidado"/> a partir de un resultado de validación exitoso.
        /// </summary>
        /// <param name="resultado">Resultado de validación válido.</param>
        /// <param name="parteNumerica">Parte numérica del documento.</param>
        /// <returns>Nueva instancia de <see cref="DocumentoValidado"/>.</returns>
        public static DocumentoValidado DesdeResultado(ResultadoValidacion resultado, string parteNumerica)
        {
            return new DocumentoValidado(
                resultado.ValorOriginal,
                resultado.ValorNormalizado,
                resultado.TipoDocumento,
                resultado.CaracterControl,
                parteNumerica);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{TipoDocumento}: {ValorNormalizado}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is DocumentoValidado otro)
                return ValorNormalizado == otro.ValorNormalizado && TipoDocumento == otro.TipoDocumento;
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ValorNormalizado, TipoDocumento);
        }
    }
}
