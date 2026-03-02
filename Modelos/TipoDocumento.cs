namespace nif_dni_nie_cif_validation.Modelos
{
    /// <summary>
    /// Enumeración que representa los tipos de documentos de identificación fiscal y personal en España.
    /// </summary>
    public enum TipoDocumento
    {
        /// <summary>
        /// Documento Nacional de Identidad.
        /// Formato: 8 dígitos + 1 letra de control (ej: 12345678Z).
        /// </summary>
        DNI = 0,

        /// <summary>
        /// Número de Identidad de Extranjero.
        /// Formato: 1 letra (X, Y o Z) + 7 dígitos + 1 letra de control (ej: X1234567L).
        /// </summary>
        NIE = 1,

        /// <summary>
        /// Número de Identificación Fiscal para personas físicas.
        /// Engloba tanto el DNI como el NIE, además de NIFs especiales (K, L, M).
        /// </summary>
        NIF = 2,

        /// <summary>
        /// Código de Identificación Fiscal para personas jurídicas (empresas, asociaciones, etc.).
        /// Formato: 1 letra + 7 dígitos + 1 dígito/letra de control (ej: B12345678).
        /// </summary>
        CIF = 3,

        /// <summary>
        /// Tipo de documento no reconocido o no válido.
        /// </summary>
        Desconocido = -1
    }
}
