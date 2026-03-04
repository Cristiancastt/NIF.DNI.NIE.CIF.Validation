using System.Collections.Frozen;

namespace NIF.DNI.NIE.CIF.Validation.Modelos
{
    /// <summary>
    /// Representa el resultado de la validación de un lote (conjunto) de documentos.
    /// Todos los datos son inmutables y thread-safe tras la construcción.
    /// Las búsquedas por tipo son O(1) gracias a <see cref="FrozenDictionary{TKey, TValue}"/>.
    /// Las colecciones públicas son <see cref="IReadOnlyList{T}"/> respaldadas por arrays (acceso O(1) por índice).
    /// </summary>
    public sealed class ResultadoValidacionLote
    {
        // ─── Colecciones inmutables (arrays internos, expuestos como IReadOnlyList) ─

        /// <summary>
        /// Lista de documentos que pasaron la validación exitosamente. Acceso O(1) por índice.
        /// Inmutable y thread-safe.
        /// </summary>
        public IReadOnlyList<DocumentoValidado> Validos { get; }

        /// <summary>
        /// Lista de errores de los documentos que no pasaron la validación. Acceso O(1) por índice.
        /// Inmutable y thread-safe.
        /// </summary>
        public IReadOnlyList<ErrorValidacion> Invalidos { get; }

        /// <summary>
        /// Lista completa de resultados individuales (tanto válidos como inválidos). Acceso O(1) por índice.
        /// Inmutable y thread-safe.
        /// </summary>
        public IReadOnlyList<ResultadoValidacion> ResultadosIndividuales { get; }

        // ─── Diccionarios congelados para búsqueda O(1) por tipo ────────────────

        /// <summary>
        /// Documentos válidos agrupados por tipo. Búsqueda O(1) por clave.
        /// Inmutable y thread-safe.
        /// </summary>
        private readonly FrozenDictionary<TipoDocumento, IReadOnlyList<DocumentoValidado>> _validosPorTipo;

        /// <summary>
        /// Errores agrupados por tipo de documento esperado. Búsqueda O(1) por clave.
        /// Inmutable y thread-safe.
        /// </summary>
        private readonly FrozenDictionary<TipoDocumento, IReadOnlyList<ErrorValidacion>> _invalidosPorTipo;

        // ─── Estadísticas pre-calculadas (O(1) de acceso, calculadas una sola vez) ─

        /// <summary>
        /// Número total de documentos procesados.
        /// </summary>
        public int TotalProcesados { get; }

        /// <summary>
        /// Número de documentos válidos encontrados. Pre-calculado, O(1).
        /// </summary>
        public int TotalValidos { get; }

        /// <summary>
        /// Número de documentos inválidos encontrados. Pre-calculado, O(1).
        /// </summary>
        public int TotalInvalidos { get; }

        /// <summary>
        /// Indica si todos los documentos del lote son válidos. Pre-calculado, O(1).
        /// </summary>
        public bool TodosValidos { get; }

        /// <summary>
        /// Indica si todos los documentos del lote son inválidos. Pre-calculado, O(1).
        /// </summary>
        public bool TodosInvalidos { get; }

        /// <summary>
        /// Indica si hay al menos un documento válido en el lote. Pre-calculado, O(1).
        /// </summary>
        public bool TieneValidos { get; }

        /// <summary>
        /// Indica si hay al menos un documento inválido en el lote. Pre-calculado, O(1).
        /// </summary>
        public bool TieneInvalidos { get; }

        /// <summary>
        /// Porcentaje de documentos válidos respecto al total procesado (0-100). Pre-calculado, O(1).
        /// </summary>
        public double PorcentajeValidos { get; }

        /// <summary>
        /// Porcentaje de documentos inválidos respecto al total procesado (0-100). Pre-calculado, O(1).
        /// </summary>
        public double PorcentajeInvalidos { get; }

        /// <summary>
        /// Resumen textual del resultado del lote. Pre-calculado, O(1).
        /// </summary>
        public string Resumen { get; }

        /// <summary>
        /// Crea una nueva instancia inmutable de <see cref="ResultadoValidacionLote"/>.
        /// Pre-computa todos los diccionarios y estadísticas durante la construcción para garantizar
        /// acceso O(1) en todas las operaciones de consulta posteriores.
        /// </summary>
        /// <param name="validos">Documentos válidos.</param>
        /// <param name="invalidos">Errores de validación.</param>
        /// <param name="resultadosIndividuales">Resultados individuales completos.</param>
        /// <param name="totalProcesados">Número total de documentos procesados.</param>
        public ResultadoValidacionLote(
            IReadOnlyList<DocumentoValidado> validos,
            IReadOnlyList<ErrorValidacion> invalidos,
            IReadOnlyList<ResultadoValidacion> resultadosIndividuales,
            int totalProcesados)
        {
            Validos = validos ?? Array.Empty<DocumentoValidado>();
            Invalidos = invalidos ?? Array.Empty<ErrorValidacion>();
            ResultadosIndividuales = resultadosIndividuales ?? Array.Empty<ResultadoValidacion>();
            TotalProcesados = totalProcesados;

            // Pre-calcular estadísticas una sola vez → O(1) en lectura
            TotalValidos = Validos.Count;
            TotalInvalidos = Invalidos.Count;
            TodosValidos = TotalInvalidos == 0 && TotalProcesados > 0;
            TodosInvalidos = TotalValidos == 0 && TotalProcesados > 0;
            TieneValidos = TotalValidos > 0;
            TieneInvalidos = TotalInvalidos > 0;
            PorcentajeValidos = TotalProcesados > 0
                ? Math.Round((double)TotalValidos / TotalProcesados * 100, 2)
                : 0;
            PorcentajeInvalidos = TotalProcesados > 0
                ? Math.Round((double)TotalInvalidos / TotalProcesados * 100, 2)
                : 0;
            Resumen = $"Procesados: {TotalProcesados} | Válidos: {TotalValidos} ({PorcentajeValidos}%) | Inválidos: {TotalInvalidos} ({PorcentajeInvalidos}%)";

            // Pre-computar agrupación por tipo → FrozenDictionary para O(1)
            _validosPorTipo = Validos
                .GroupBy(static v => v.TipoDocumento)
                .ToFrozenDictionary(
                    static g => g.Key,
                    static g => (IReadOnlyList<DocumentoValidado>)g.ToArray());

            _invalidosPorTipo = Invalidos
                .GroupBy(static i => i.TipoDocumentoEsperado)
                .ToFrozenDictionary(
                    static g => g.Key,
                    static g => (IReadOnlyList<ErrorValidacion>)g.ToArray());
        }

        /// <summary>
        /// Instancia singleton vacía reutilizable. Evita crear objetos innecesarios.
        /// Thread-safe por <see langword="static readonly"/>.
        /// </summary>
        private static readonly ResultadoValidacionLote _vacio = new(
            Array.Empty<DocumentoValidado>(),
            Array.Empty<ErrorValidacion>(),
            Array.Empty<ResultadoValidacion>(),
            0);

        /// <summary>
        /// Devuelve una instancia singleton vacía. No crea objetos nuevos. O(1).
        /// </summary>
        /// <returns>Resultado de validación de lote vacío (singleton).</returns>
        public static ResultadoValidacionLote Vacio() => _vacio;

        /// <summary>
        /// Obtiene los documentos válidos filtrados por tipo.
        /// Búsqueda O(1) con <see cref="FrozenDictionary{TKey, TValue}"/>. Thread-safe.
        /// </summary>
        /// <param name="tipo">Tipo de documento a filtrar.</param>
        /// <returns>Lista inmutable de documentos válidos del tipo especificado, o lista vacía si no hay ninguno.</returns>
        public IReadOnlyList<DocumentoValidado> ObtenerValidosPorTipo(TipoDocumento tipo)
        {
            return _validosPorTipo.TryGetValue(tipo, out var lista)
                ? lista
                : Array.Empty<DocumentoValidado>();
        }

        /// <summary>
        /// Obtiene los errores filtrados por tipo de documento esperado.
        /// Búsqueda O(1) con <see cref="FrozenDictionary{TKey, TValue}"/>. Thread-safe.
        /// </summary>
        /// <param name="tipo">Tipo de documento a filtrar.</param>
        /// <returns>Lista inmutable de errores del tipo especificado, o lista vacía si no hay ninguno.</returns>
        public IReadOnlyList<ErrorValidacion> ObtenerInvalidosPorTipo(TipoDocumento tipo)
        {
            return _invalidosPorTipo.TryGetValue(tipo, out var lista)
                ? lista
                : Array.Empty<ErrorValidacion>();
        }

        /// <summary>
        /// Obtiene los tipos de documento presentes entre los documentos válidos. O(1).
        /// </summary>
        /// <returns>Colección inmutable de tipos de documento con al menos un documento válido.</returns>
        public IEnumerable<TipoDocumento> ObtenerTiposValidos() => _validosPorTipo.Keys;

        /// <summary>
        /// Obtiene los tipos de documento presentes entre los errores. O(1).
        /// </summary>
        /// <returns>Colección inmutable de tipos de documento con al menos un error.</returns>
        public IEnumerable<TipoDocumento> ObtenerTiposInvalidos() => _invalidosPorTipo.Keys;

        /// <summary>
        /// Indica si hay documentos válidos del tipo especificado. O(1).
        /// </summary>
        /// <param name="tipo">Tipo de documento.</param>
        /// <returns>true si hay al menos un documento válido del tipo.</returns>
        public bool TieneValidosDeTipo(TipoDocumento tipo) => _validosPorTipo.ContainsKey(tipo);

        /// <summary>
        /// Indica si hay errores del tipo especificado. O(1).
        /// </summary>
        /// <param name="tipo">Tipo de documento.</param>
        /// <returns>true si hay al menos un error del tipo.</returns>
        public bool TieneInvalidosDeTipo(TipoDocumento tipo) => _invalidosPorTipo.ContainsKey(tipo);

        /// <summary>
        /// Obtiene el número de documentos válidos de un tipo específico. O(1).
        /// </summary>
        /// <param name="tipo">Tipo de documento.</param>
        /// <returns>Número de documentos válidos del tipo.</returns>
        public int ContarValidosPorTipo(TipoDocumento tipo)
        {
            return _validosPorTipo.TryGetValue(tipo, out var lista) ? lista.Count : 0;
        }

        /// <summary>
        /// Obtiene el número de errores de un tipo específico. O(1).
        /// </summary>
        /// <param name="tipo">Tipo de documento.</param>
        /// <returns>Número de errores del tipo.</returns>
        public int ContarInvalidosPorTipo(TipoDocumento tipo)
        {
            return _invalidosPorTipo.TryGetValue(tipo, out var lista) ? lista.Count : 0;
        }

        /// <inheritdoc />
        public override string ToString() => Resumen;
    }
}
