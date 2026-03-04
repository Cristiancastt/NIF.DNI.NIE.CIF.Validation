using FluentAssertions;
using NIF.DNI.NIE.CIF.Validation.Implentaciones;
using NIF.DNI.NIE.CIF.Validation.Interfaces;
using NIF.DNI.NIE.CIF.Validation.Modelos;

namespace NIF.DNI.NIE.CIF.Validation.Tests.Implentaciones
{
    /// <summary>
    /// Tests para los métodos asíncronos de ValidadorDocumentos:
    /// ValidarLoteDniAsync, ValidarLoteNieAsync, ValidarLoteNifAsync,
    /// ValidarLoteCifAsync, ValidarLoteDocumentosAsync.
    /// Incluye cancelación y thread-safety.
    /// </summary>
    public class ValidadorDocumentosAsyncTests
    {
        private readonly IValidadorDocumentos _validador = new ValidadorDocumentos();

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  LOTES ASÍNCRONOS — DNI                            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteDniAsync_MixtoValidoInvalido_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "00000000T", "INVALIDO", "12345678A" };

            var lote = await _validador.ValidarLoteDniAsync(documentos);

            lote.TotalProcesados.Should().Be(4);
            lote.TotalValidos.Should().Be(2);
            lote.TotalInvalidos.Should().Be(2);
        }

        [Fact]
        public async Task ValidarLoteDniAsync_TodosValidos_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "00000000T", "99999999R" };

            var lote = await _validador.ValidarLoteDniAsync(documentos);

            lote.TodosValidos.Should().BeTrue();
            lote.TotalValidos.Should().Be(3);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  LOTES ASÍNCRONOS — NIE                            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteNieAsync_Validos_ResultadoCorrecto()
        {
            var documentos = new[] { "X1234567L", "X0000000T", "Y0000000Z" };

            var lote = await _validador.ValidarLoteNieAsync(documentos);

            lote.TodosValidos.Should().BeTrue();
            lote.TotalValidos.Should().Be(3);
        }

        [Fact]
        public async Task ValidarLoteNieAsync_Invalidos_ResultadoCorrecto()
        {
            var documentos = new[] { "INVALIDO1", "INVALIDO2" };

            var lote = await _validador.ValidarLoteNieAsync(documentos);

            lote.TodosInvalidos.Should().BeTrue();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  LOTES ASÍNCRONOS — NIF                            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteNifAsync_MixtoTipos_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "X1234567L", "K0000000T", "INVALIDO" };

            var lote = await _validador.ValidarLoteNifAsync(documentos);

            lote.TotalValidos.Should().Be(3);
            lote.TotalInvalidos.Should().Be(1);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  LOTES ASÍNCRONOS — CIF                            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteCifAsync_Validos_ResultadoCorrecto()
        {
            var documentos = new[] { "A00000000", "B00000000" };

            var lote = await _validador.ValidarLoteCifAsync(documentos);

            lote.TodosValidos.Should().BeTrue();
        }

        [Fact]
        public async Task ValidarLoteCifAsync_Invalidos_ResultadoCorrecto()
        {
            var documentos = new[] { "A00000001", "B00000001" };

            var lote = await _validador.ValidarLoteCifAsync(documentos);

            lote.TodosInvalidos.Should().BeTrue();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  LOTES ASÍNCRONOS — DOCUMENTOS                     ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteDocumentosAsync_MixtoTipos_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "X1234567L", "A00000000", "INVALIDO" };

            var lote = await _validador.ValidarLoteDocumentosAsync(documentos);

            lote.TotalProcesados.Should().Be(4);
            lote.TotalValidos.Should().Be(3);
            lote.TotalInvalidos.Should().Be(1);
            lote.TieneValidos.Should().BeTrue();
            lote.TieneInvalidos.Should().BeTrue();
        }

        [Fact]
        public async Task ValidarLoteDocumentosAsync_LoteGrande_CompletaCorrectamente()
        {
            // Generar lote grande para probar paralelismo
            var documentos = Enumerable.Range(0, 1000)
                .Select(i => Dni.DesdeNumero(i).Valor)
                .ToArray();

            var lote = await _validador.ValidarLoteDocumentosAsync(documentos);

            lote.TotalProcesados.Should().Be(1000);
            lote.TodosValidos.Should().BeTrue();
            lote.TotalValidos.Should().Be(1000);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  EDGE CASES ASÍNCRONOS                             ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteDocumentosAsync_Null_DevuelveVacio()
        {
            var lote = await _validador.ValidarLoteDocumentosAsync(null!);

            lote.TotalProcesados.Should().Be(0);
            lote.Should().BeSameAs(ResultadoValidacionLote.Vacio());
        }

        [Fact]
        public async Task ValidarLoteDocumentosAsync_Vacio_DevuelveVacio()
        {
            var lote = await _validador.ValidarLoteDocumentosAsync(Array.Empty<string>());

            lote.TotalProcesados.Should().Be(0);
        }

        [Fact]
        public async Task ValidarLoteDniAsync_UnSoloElemento_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z" };

            var lote = await _validador.ValidarLoteDniAsync(documentos);

            lote.TotalProcesados.Should().Be(1);
            lote.TodosValidos.Should().BeTrue();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  CANCELACIÓN                                       ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteDocumentosAsync_CancelacionPrevia_LanzaOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var documentos = new[] { "12345678Z" };

            var act = () => _validador.ValidarLoteDocumentosAsync(documentos, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  THREAD SAFETY                                     ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteDocumentosAsync_MultiplesConcurrentes_SinCorrupcion()
        {
            // Ejecutar múltiples validaciones de lotes en paralelo
            var tarea1 = _validador.ValidarLoteDniAsync(new[] { "12345678Z", "00000000T" });
            var tarea2 = _validador.ValidarLoteNieAsync(new[] { "X1234567L", "X0000000T" });
            var tarea3 = _validador.ValidarLoteCifAsync(new[] { "A00000000", "B00000000" });

            await Task.WhenAll(tarea1, tarea2, tarea3);

            var lote1 = await tarea1;
            var lote2 = await tarea2;
            var lote3 = await tarea3;

            lote1.TodosValidos.Should().BeTrue();
            lote2.TodosValidos.Should().BeTrue();
            lote3.TodosValidos.Should().BeTrue();
        }

        [Fact]
        public async Task ValidadorDocumentos_InstanciasCompartidas_ThreadSafe()
        {
            // Misma instancia, múltiples hilos simultáneos
            var tareas = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() =>
                {
                    _validador.EsDniValido("12345678Z").Should().BeTrue();
                    _validador.EsNieValido("X1234567L").Should().BeTrue();
                    _validador.EsCifValido("A00000000").Should().BeTrue();
                }));

            await Task.WhenAll(tareas);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║            RESULTADOS ASYNC VS SYNC — CONSISTENCIA                 ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public async Task ValidarLoteDniAsync_MismoResultadoQueSync()
        {
            var documentos = new[] { "12345678Z", "INVALIDO", "00000000T" };

            var loteSync = _validador.ValidarLoteDni(documentos);
            var loteAsync = await _validador.ValidarLoteDniAsync(documentos);

            loteAsync.TotalProcesados.Should().Be(loteSync.TotalProcesados);
            loteAsync.TotalValidos.Should().Be(loteSync.TotalValidos);
            loteAsync.TotalInvalidos.Should().Be(loteSync.TotalInvalidos);
        }
    }
}
