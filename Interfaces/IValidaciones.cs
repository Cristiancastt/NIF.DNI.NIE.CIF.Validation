using nif_dni_nie_cif_validation.Modelos;

namespace nif_dni_nie_cif_validation.Interfaces
{
    /// <summary>
    /// Interfaz principal del servicio de validación de documentos de identificación españoles.
    /// Proporciona métodos síncronos y asíncronos para validar DNI, NIE, NIF y CIF,
    /// tanto de forma individual como en lotes.
    /// </summary>
    /// <remarks>
    /// <para>La interfaz ofrece tres estilos de API:</para>
    /// <list type="number">
    /// <item><description><b>Métodos que lanzan excepción:</b> <c>ValidarDni</c>, <c>ValidarNie</c>, etc. Lanzan <see cref="Excepciones.DocumentoNoValidoException"/> si el documento no es válido.</description></item>
    /// <item><description><b>Métodos que devuelven resultado:</b> <c>IntentarValidarDni</c>, <c>IntentarValidarNie</c>, etc. Devuelven un <see cref="ResultadoValidacion"/> sin lanzar excepciones.</description></item>
    /// <item><description><b>Métodos booleanos:</b> <c>EsDniValido</c>, <c>EsNieValido</c>, etc. Devuelven true/false simplemente.</description></item>
    /// </list>
    /// <para>Además, incluye métodos para procesamiento por lotes (síncronos y asíncronos).</para>
    /// </remarks>
    public interface IValidadorDocumentos
    {
        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    MÉTODOS BOOLEANOS (SIMPLES)                      ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Comprueba si un DNI es válido.
        /// </summary>
        /// <param name="dni">DNI a validar.</param>
        /// <returns>true si el DNI es válido; false en caso contrario.</returns>
        bool EsDniValido(string dni);

        /// <summary>
        /// Comprueba si un NIE es válido.
        /// </summary>
        /// <param name="nie">NIE a validar.</param>
        /// <returns>true si el NIE es válido; false en caso contrario.</returns>
        bool EsNieValido(string nie);

        /// <summary>
        /// Comprueba si un NIF es válido (acepta DNI, NIE y NIF especiales K/L/M).
        /// </summary>
        /// <param name="nif">NIF a validar.</param>
        /// <returns>true si el NIF es válido; false en caso contrario.</returns>
        bool EsNifValido(string nif);

        /// <summary>
        /// Comprueba si un CIF es válido.
        /// </summary>
        /// <param name="cif">CIF a validar.</param>
        /// <returns>true si el CIF es válido; false en caso contrario.</returns>
        bool EsCifValido(string cif);

        /// <summary>
        /// Comprueba si un documento es válido (detecta automáticamente el tipo).
        /// </summary>
        /// <param name="documento">Documento a validar.</param>
        /// <returns>true si el documento es válido; false en caso contrario.</returns>
        bool EsDocumentoValido(string documento);

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║           MÉTODOS CON EXCEPCIÓN (LANZA SI NO ES VÁLIDO)            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Valida un DNI y devuelve la información del documento validado.
        /// Lanza <see cref="Excepciones.DocumentoNoValidoException"/> si no es válido.
        /// </summary>
        /// <param name="dni">DNI a validar.</param>
        /// <returns>Información del DNI validado.</returns>
        /// <exception cref="Excepciones.DocumentoNoValidoException">Si el DNI no es válido.</exception>
        DocumentoValidado ValidarDni(string dni);

        /// <summary>
        /// Valida un NIE y devuelve la información del documento validado.
        /// Lanza <see cref="Excepciones.DocumentoNoValidoException"/> si no es válido.
        /// </summary>
        /// <param name="nie">NIE a validar.</param>
        /// <returns>Información del NIE validado.</returns>
        /// <exception cref="Excepciones.DocumentoNoValidoException">Si el NIE no es válido.</exception>
        DocumentoValidado ValidarNie(string nie);

        /// <summary>
        /// Valida un NIF y devuelve la información del documento validado.
        /// Acepta DNI, NIE y NIF especiales (K, L, M).
        /// Lanza <see cref="Excepciones.DocumentoNoValidoException"/> si no es válido.
        /// </summary>
        /// <param name="nif">NIF a validar.</param>
        /// <returns>Información del NIF validado.</returns>
        /// <exception cref="Excepciones.DocumentoNoValidoException">Si el NIF no es válido.</exception>
        DocumentoValidado ValidarNif(string nif);

        /// <summary>
        /// Valida un CIF y devuelve la información del documento validado.
        /// Lanza <see cref="Excepciones.DocumentoNoValidoException"/> si no es válido.
        /// </summary>
        /// <param name="cif">CIF a validar.</param>
        /// <returns>Información del CIF validado.</returns>
        /// <exception cref="Excepciones.DocumentoNoValidoException">Si el CIF no es válido.</exception>
        DocumentoValidado ValidarCif(string cif);

        /// <summary>
        /// Valida un documento detectando automáticamente su tipo (DNI, NIE, NIF o CIF).
        /// Lanza <see cref="Excepciones.DocumentoNoValidoException"/> si no es válido.
        /// </summary>
        /// <param name="documento">Documento a validar.</param>
        /// <returns>Información del documento validado.</returns>
        /// <exception cref="Excepciones.DocumentoNoValidoException">Si el documento no es válido.</exception>
        DocumentoValidado ValidarDocumento(string documento);

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║        MÉTODOS SIN EXCEPCIÓN (DEVUELVEN RESULTADO)                 ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Intenta validar un DNI y devuelve un resultado detallado sin lanzar excepciones.
        /// </summary>
        /// <param name="dni">DNI a validar.</param>
        /// <returns>Resultado de la validación con toda la información.</returns>
        ResultadoValidacion IntentarValidarDni(string dni);

        /// <summary>
        /// Intenta validar un NIE y devuelve un resultado detallado sin lanzar excepciones.
        /// </summary>
        /// <param name="nie">NIE a validar.</param>
        /// <returns>Resultado de la validación con toda la información.</returns>
        ResultadoValidacion IntentarValidarNie(string nie);

        /// <summary>
        /// Intenta validar un NIF y devuelve un resultado detallado sin lanzar excepciones.
        /// Acepta DNI, NIE y NIF especiales (K, L, M).
        /// </summary>
        /// <param name="nif">NIF a validar.</param>
        /// <returns>Resultado de la validación con toda la información.</returns>
        ResultadoValidacion IntentarValidarNif(string nif);

        /// <summary>
        /// Intenta validar un CIF y devuelve un resultado detallado sin lanzar excepciones.
        /// </summary>
        /// <param name="cif">CIF a validar.</param>
        /// <returns>Resultado de la validación con toda la información.</returns>
        ResultadoValidacion IntentarValidarCif(string cif);

        /// <summary>
        /// Intenta validar un documento (detecta tipo automáticamente) y devuelve un resultado detallado sin lanzar excepciones.
        /// </summary>
        /// <param name="documento">Documento a validar.</param>
        /// <returns>Resultado de la validación con toda la información.</returns>
        ResultadoValidacion IntentarValidarDocumento(string documento);

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                   VALIDACIÓN POR LOTES (SÍNCRONA)                  ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Valida un lote de DNIs y devuelve un resultado completo con válidos, inválidos y estadísticas.
        /// </summary>
        /// <param name="dnis">Colección de DNIs a validar.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        ResultadoValidacionLote ValidarLoteDni(IEnumerable<string> dnis);

        /// <summary>
        /// Valida un lote de NIEs y devuelve un resultado completo con válidos, inválidos y estadísticas.
        /// </summary>
        /// <param name="nies">Colección de NIEs a validar.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        ResultadoValidacionLote ValidarLoteNie(IEnumerable<string> nies);

        /// <summary>
        /// Valida un lote de NIFs y devuelve un resultado completo con válidos, inválidos y estadísticas.
        /// </summary>
        /// <param name="nifs">Colección de NIFs a validar.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        ResultadoValidacionLote ValidarLoteNif(IEnumerable<string> nifs);

        /// <summary>
        /// Valida un lote de CIFs y devuelve un resultado completo con válidos, inválidos y estadísticas.
        /// </summary>
        /// <param name="cifs">Colección de CIFs a validar.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        ResultadoValidacionLote ValidarLoteCif(IEnumerable<string> cifs);

        /// <summary>
        /// Valida un lote de documentos de cualquier tipo (detecta automáticamente) y devuelve un resultado completo.
        /// </summary>
        /// <param name="documentos">Colección de documentos a validar.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        ResultadoValidacionLote ValidarLoteDocumentos(IEnumerable<string> documentos);

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  VALIDACIÓN POR LOTES (ASÍNCRONA)                  ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Valida un lote de DNIs de forma asíncrona con procesamiento en paralelo.
        /// </summary>
        /// <param name="dnis">Colección de DNIs a validar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        Task<ResultadoValidacionLote> ValidarLoteDniAsync(IEnumerable<string> dnis, CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida un lote de NIEs de forma asíncrona con procesamiento en paralelo.
        /// </summary>
        /// <param name="nies">Colección de NIEs a validar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        Task<ResultadoValidacionLote> ValidarLoteNieAsync(IEnumerable<string> nies, CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida un lote de NIFs de forma asíncrona con procesamiento en paralelo.
        /// </summary>
        /// <param name="nifs">Colección de NIFs a validar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        Task<ResultadoValidacionLote> ValidarLoteNifAsync(IEnumerable<string> nifs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida un lote de CIFs de forma asíncrona con procesamiento en paralelo.
        /// </summary>
        /// <param name="cifs">Colección de CIFs a validar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        Task<ResultadoValidacionLote> ValidarLoteCifAsync(IEnumerable<string> cifs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida un lote de documentos de cualquier tipo de forma asíncrona con procesamiento en paralelo.
        /// </summary>
        /// <param name="documentos">Colección de documentos a validar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Resultado del procesamiento del lote.</returns>
        Task<ResultadoValidacionLote> ValidarLoteDocumentosAsync(IEnumerable<string> documentos, CancellationToken cancellationToken = default);

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                     UTILIDADES Y HERRAMIENTAS                       ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Detecta el tipo de documento basándose en su formato.
        /// </summary>
        /// <param name="documento">Documento a analizar.</param>
        /// <returns>Tipo de documento detectado.</returns>
        TipoDocumento DetectarTipo(string documento);

        /// <summary>
        /// Normaliza un documento eliminando espacios, guiones, puntos y convirtiéndolo a mayúsculas.
        /// </summary>
        /// <param name="documento">Documento a normalizar.</param>
        /// <returns>Documento normalizado.</returns>
        string Normalizar(string documento);

        /// <summary>
        /// Calcula la letra de control de un DNI dado su número.
        /// </summary>
        /// <param name="numero">Número del DNI (0 a 99.999.999).</param>
        /// <returns>Letra de control correspondiente.</returns>
        char ObtenerLetraDni(int numero);

        /// <summary>
        /// Calcula la letra de control de un NIE.
        /// </summary>
        /// <param name="nie">NIE (X/Y/Z + 7 dígitos).</param>
        /// <returns>Letra de control correspondiente.</returns>
        char ObtenerLetraNie(string nie);

        /// <summary>
        /// Calcula el carácter de control de un CIF.
        /// </summary>
        /// <param name="cif">CIF (letra + 7 dígitos).</param>
        /// <returns>Tupla con el dígito y la letra de control calculados.</returns>
        (char Digito, char Letra) ObtenerControlCif(string cif);
    }
}
