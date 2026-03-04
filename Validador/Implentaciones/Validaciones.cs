using System.Collections.Concurrent;
using NIF.DNI.NIE.CIF.Validation.Excepciones;
using NIF.DNI.NIE.CIF.Validation.Interfaces;
using NIF.DNI.NIE.CIF.Validation.Modelos;
using NIF.DNI.NIE.CIF.Validation.Utiles;

namespace NIF.DNI.NIE.CIF.Validation.Implentaciones
{
    /// <summary>
    /// Implementación completa del servicio de validación de documentos de identificación españoles.
    /// Proporciona métodos para validar DNI, NIE, NIF y CIF de forma individual y por lotes,
    /// con versiones síncronas y asíncronas.
    /// </summary>
    /// <remarks>
    /// <para><b>Ejemplo de uso básico:</b></para>
    /// <code>
    /// var validador = new ValidadorDocumentos();
    ///
    /// // Validación simple (bool)
    /// bool esDniValido = validador.EsDniValido("12345678Z");
    ///
    /// // Validación con resultado detallado (sin excepción)
    /// var resultado = validador.IntentarValidarDni("12345678Z");
    /// if (resultado.EsValido)
    ///     Console.WriteLine(resultado.Mensaje);
    ///
    /// // Validación con excepción si es inválido
    /// try
    /// {
    ///     var doc = validador.ValidarDni("12345678Z");
    ///     Console.WriteLine(doc.CaracterControl);
    /// }
    /// catch (DocumentoNoValidoException ex)
    /// {
    ///     Console.WriteLine(ex.Message);
    /// }
    ///
    /// // Validación por lotes
    /// var lote = validador.ValidarLoteDocumentos(new[] { "12345678Z", "X1234567L", "INVALIDO" });
    /// Console.WriteLine(lote.Resumen);
    /// </code>
    /// </remarks>
    public class ValidadorDocumentos : IValidadorDocumentos
    {
        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    MÉTODOS BOOLEANOS (SIMPLES)                      ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <inheritdoc />
        public bool EsDniValido(string dni)
        {
            return Dni.Intentar(dni, out _);
        }

        /// <inheritdoc />
        public bool EsNieValido(string nie)
        {
            return Nie.Intentar(nie, out _);
        }

        /// <inheritdoc />
        public bool EsNifValido(string nif)
        {
            return Nif.Intentar(nif, out _);
        }

        /// <inheritdoc />
        public bool EsCifValido(string cif)
        {
            return Cif.Intentar(cif, out _);
        }

        /// <inheritdoc />
        public bool EsDocumentoValido(string documento)
        {
            var tipo = UtilesValidacion.DetectarTipoDocumento(documento);
            return tipo switch
            {
                TipoDocumento.DNI => EsDniValido(documento),
                TipoDocumento.NIE => EsNieValido(documento),
                TipoDocumento.NIF => EsNifValido(documento),
                TipoDocumento.CIF => EsCifValido(documento),
                _ => false
            };
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║           MÉTODOS CON EXCEPCIÓN (LANZA SI NO ES VÁLIDO)            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <inheritdoc />
        public DocumentoValidado ValidarDni(string dni)
        {
            var dniObj = Dni.Crear(dni);
            return new DocumentoValidado(
                dni,
                dniObj.Valor,
                TipoDocumento.DNI,
                dniObj.CaracterControl,
                dniObj.ParteNumerica);
        }

        /// <inheritdoc />
        public DocumentoValidado ValidarNie(string nie)
        {
            var nieObj = Nie.Crear(nie);
            return new DocumentoValidado(
                nie,
                nieObj.Valor,
                TipoDocumento.NIE,
                nieObj.CaracterControl,
                nieObj.ParteNumerica);
        }

        /// <inheritdoc />
        public DocumentoValidado ValidarNif(string nif)
        {
            var nifObj = Nif.Crear(nif);
            return new DocumentoValidado(
                nif,
                nifObj.Valor,
                nifObj.SubTipo,
                nifObj.CaracterControl,
                nifObj.ParteNumerica);
        }

        /// <inheritdoc />
        public DocumentoValidado ValidarCif(string cif)
        {
            var cifObj = Cif.Crear(cif);
            return new DocumentoValidado(
                cif,
                cifObj.Valor,
                TipoDocumento.CIF,
                cifObj.CaracterControl,
                cifObj.ParteNumerica);
        }

        /// <inheritdoc />
        public DocumentoValidado ValidarDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                throw DocumentoNoValidoException.ValorVacio();

            var tipo = UtilesValidacion.DetectarTipoDocumento(documento);

            return tipo switch
            {
                TipoDocumento.DNI => ValidarDni(documento),
                TipoDocumento.NIE => ValidarNie(documento),
                TipoDocumento.NIF => ValidarNif(documento),
                TipoDocumento.CIF => ValidarCif(documento),
                _ => throw DocumentoNoValidoException.TipoDesconocido(documento)
            };
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║        MÉTODOS SIN EXCEPCIÓN (DEVUELVEN RESULTADO)                 ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <inheritdoc />
        public ResultadoValidacion IntentarValidarDni(string dni)
        {
            string normalizado = UtilesValidacion.Normalizar(dni);
            try
            {
                var dniObj = Dni.Crear(dni);
                return ResultadoValidacion.Valido(
                    dni,
                    dniObj.Valor,
                    TipoDocumento.DNI,
                    dniObj.CaracterControl);
            }
            catch (DocumentoNoValidoException ex)
            {
                return ResultadoValidacion.Invalido(
                    dni ?? "",
                    normalizado,
                    TipoDocumento.DNI,
                    ex.Message);
            }
        }

        /// <inheritdoc />
        public ResultadoValidacion IntentarValidarNie(string nie)
        {
            string normalizado = UtilesValidacion.Normalizar(nie);
            try
            {
                var nieObj = Nie.Crear(nie);
                return ResultadoValidacion.Valido(
                    nie,
                    nieObj.Valor,
                    TipoDocumento.NIE,
                    nieObj.CaracterControl);
            }
            catch (DocumentoNoValidoException ex)
            {
                return ResultadoValidacion.Invalido(
                    nie ?? "",
                    normalizado,
                    TipoDocumento.NIE,
                    ex.Message);
            }
        }

        /// <inheritdoc />
        public ResultadoValidacion IntentarValidarNif(string nif)
        {
            string normalizado = UtilesValidacion.Normalizar(nif);
            try
            {
                var nifObj = Nif.Crear(nif);
                return ResultadoValidacion.Valido(
                    nif,
                    nifObj.Valor,
                    nifObj.SubTipo,
                    nifObj.CaracterControl);
            }
            catch (DocumentoNoValidoException ex)
            {
                return ResultadoValidacion.Invalido(
                    nif ?? "",
                    normalizado,
                    TipoDocumento.NIF,
                    ex.Message);
            }
        }

        /// <inheritdoc />
        public ResultadoValidacion IntentarValidarCif(string cif)
        {
            string normalizado = UtilesValidacion.Normalizar(cif);
            try
            {
                var cifObj = Cif.Crear(cif);
                return ResultadoValidacion.Valido(
                    cif,
                    cifObj.Valor,
                    TipoDocumento.CIF,
                    cifObj.CaracterControl);
            }
            catch (DocumentoNoValidoException ex)
            {
                return ResultadoValidacion.Invalido(
                    cif ?? "",
                    normalizado,
                    TipoDocumento.CIF,
                    ex.Message);
            }
        }

        /// <inheritdoc />
        public ResultadoValidacion IntentarValidarDocumento(string documento)
        {
            string normalizado = UtilesValidacion.Normalizar(documento);

            if (string.IsNullOrEmpty(normalizado))
            {
                return ResultadoValidacion.Invalido(
                    documento ?? "",
                    normalizado,
                    TipoDocumento.Desconocido,
                    "El documento no puede ser nulo, vacío o contener solo espacios en blanco.");
            }

            var tipo = UtilesValidacion.DetectarTipoDocumento(documento);

            return tipo switch
            {
                TipoDocumento.DNI => IntentarValidarDni(documento),
                TipoDocumento.NIE => IntentarValidarNie(documento),
                TipoDocumento.NIF => IntentarValidarNif(documento),
                TipoDocumento.CIF => IntentarValidarCif(documento),
                _ => ResultadoValidacion.Invalido(
                    documento,
                    normalizado,
                    TipoDocumento.Desconocido,
                    $"No se pudo determinar el tipo de documento para '{documento}'. No coincide con el formato de DNI, NIE, NIF ni CIF.")
            };
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                   VALIDACIÓN POR LOTES (SÍNCRONA)                  ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <inheritdoc />
        public ResultadoValidacionLote ValidarLoteDni(IEnumerable<string> dnis)
        {
            return ProcesarLote(dnis, IntentarValidarDni);
        }

        /// <inheritdoc />
        public ResultadoValidacionLote ValidarLoteNie(IEnumerable<string> nies)
        {
            return ProcesarLote(nies, IntentarValidarNie);
        }

        /// <inheritdoc />
        public ResultadoValidacionLote ValidarLoteNif(IEnumerable<string> nifs)
        {
            return ProcesarLote(nifs, IntentarValidarNif);
        }

        /// <inheritdoc />
        public ResultadoValidacionLote ValidarLoteCif(IEnumerable<string> cifs)
        {
            return ProcesarLote(cifs, IntentarValidarCif);
        }

        /// <inheritdoc />
        public ResultadoValidacionLote ValidarLoteDocumentos(IEnumerable<string> documentos)
        {
            return ProcesarLote(documentos, IntentarValidarDocumento);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  VALIDACIÓN POR LOTES (ASÍNCRONA)                  ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <inheritdoc />
        public Task<ResultadoValidacionLote> ValidarLoteDniAsync(IEnumerable<string> dnis, CancellationToken cancellationToken = default)
        {
            return ProcesarLoteAsync(dnis, IntentarValidarDni, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ResultadoValidacionLote> ValidarLoteNieAsync(IEnumerable<string> nies, CancellationToken cancellationToken = default)
        {
            return ProcesarLoteAsync(nies, IntentarValidarNie, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ResultadoValidacionLote> ValidarLoteNifAsync(IEnumerable<string> nifs, CancellationToken cancellationToken = default)
        {
            return ProcesarLoteAsync(nifs, IntentarValidarNif, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ResultadoValidacionLote> ValidarLoteCifAsync(IEnumerable<string> cifs, CancellationToken cancellationToken = default)
        {
            return ProcesarLoteAsync(cifs, IntentarValidarCif, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ResultadoValidacionLote> ValidarLoteDocumentosAsync(IEnumerable<string> documentos, CancellationToken cancellationToken = default)
        {
            return ProcesarLoteAsync(documentos, IntentarValidarDocumento, cancellationToken);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                     UTILIDADES Y HERRAMIENTAS                       ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <inheritdoc />
        public TipoDocumento DetectarTipo(string documento)
        {
            return UtilesValidacion.DetectarTipoDocumento(documento);
        }

        /// <inheritdoc />
        public string Normalizar(string documento)
        {
            return UtilesValidacion.Normalizar(documento);
        }

        /// <inheritdoc />
        public char ObtenerLetraDni(int numero)
        {
            return UtilesValidacion.CalcularLetraDni(numero);
        }

        /// <inheritdoc />
        public char ObtenerLetraNie(string nie)
        {
            return UtilesValidacion.CalcularLetraNie(nie);
        }

        /// <inheritdoc />
        public (char Digito, char Letra) ObtenerControlCif(string cif)
        {
            return UtilesValidacion.CalcularControlCif(cif);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                        MÉTODOS PRIVADOS                            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Procesa un lote de documentos de forma síncrona utilizando la función de validación proporcionada.
        /// Pre-asigna capacidad en las listas internas para evitar redimensionamientos.
        /// </summary>
        private static ResultadoValidacionLote ProcesarLote(
            IEnumerable<string> documentos,
            Func<string, ResultadoValidacion> funcionValidacion)
        {
            if (documentos is null)
                return ResultadoValidacionLote.Vacio();

            // Materializar a array para conocer tamaño y evitar múltiples enumeraciones
            var arregloDocumentos = documentos as string[] ?? documentos.ToArray();

            if (arregloDocumentos.Length == 0)
                return ResultadoValidacionLote.Vacio();

            int total = arregloDocumentos.Length;
            var validos = new List<DocumentoValidado>(total);
            var invalidos = new List<ErrorValidacion>(total);
            var resultados = new ResultadoValidacion[total];

            for (int i = 0; i < total; i++)
            {
                var resultado = funcionValidacion(arregloDocumentos[i]);
                resultados[i] = resultado;

                if (resultado.EsValido)
                {
                    validos.Add(new DocumentoValidado(
                        resultado.ValorOriginal,
                        resultado.ValorNormalizado,
                        resultado.TipoDocumento,
                        resultado.CaracterControl,
                        UtilesValidacion.ExtraerParteNumerica(resultado.ValorNormalizado)));
                }
                else
                {
                    invalidos.Add(ErrorValidacion.DesdeResultado(resultado));
                }
            }

            return new ResultadoValidacionLote(
                validos.ToArray(),
                invalidos.ToArray(),
                resultados,
                total);
        }

        /// <summary>
        /// Procesa un lote de documentos de forma asíncrona utilizando procesamiento en paralelo.
        /// Divide el trabajo en particiones para aprovechar múltiples núcleos del procesador.
        /// Es seguro para hilos (thread-safe) mediante <see cref="ConcurrentBag{T}"/>.
        /// </summary>
        private static async Task<ResultadoValidacionLote> ProcesarLoteAsync(
            IEnumerable<string> documentos,
            Func<string, ResultadoValidacion> funcionValidacion,
            CancellationToken cancellationToken)
        {
            if (documentos is null)
                return ResultadoValidacionLote.Vacio();

            var arregloDocumentos = documentos as string[] ?? documentos.ToArray();

            if (arregloDocumentos.Length == 0)
                return ResultadoValidacionLote.Vacio();

            var validosConcurrente = new ConcurrentBag<DocumentoValidado>();
            var invalidosConcurrente = new ConcurrentBag<ErrorValidacion>();
            var resultadosConcurrente = new ConcurrentBag<ResultadoValidacion>();

            await Parallel.ForEachAsync(
                arregloDocumentos,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                (documento, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    var resultado = funcionValidacion(documento);
                    resultadosConcurrente.Add(resultado);

                    if (resultado.EsValido)
                    {
                        validosConcurrente.Add(new DocumentoValidado(
                            resultado.ValorOriginal,
                            resultado.ValorNormalizado,
                            resultado.TipoDocumento,
                            resultado.CaracterControl,
                            UtilesValidacion.ExtraerParteNumerica(resultado.ValorNormalizado)));
                    }
                    else
                    {
                        invalidosConcurrente.Add(ErrorValidacion.DesdeResultado(resultado));
                    }

                    return ValueTask.CompletedTask;
                });

            return new ResultadoValidacionLote(
                validosConcurrente.ToArray(),
                invalidosConcurrente.ToArray(),
                resultadosConcurrente.ToArray(),
                arregloDocumentos.Length);
        }
    }
}
