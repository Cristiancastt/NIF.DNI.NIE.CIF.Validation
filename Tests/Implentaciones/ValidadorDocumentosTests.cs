using FluentAssertions;
using nif_dni_nie_cif_validation.Excepciones;
using nif_dni_nie_cif_validation.Implentaciones;
using nif_dni_nie_cif_validation.Interfaces;
using nif_dni_nie_cif_validation.Modelos;

namespace nif_dni_nie_cif_validation.Tests.Implentaciones
{
    /// <summary>
    /// Tests completos para ValidadorDocumentos — métodos síncronos:
    /// EsXxxValido, ValidarXxx (excepción), IntentarValidarXxx (resultado), lotes síncronos, utilidades.
    /// </summary>
    public class ValidadorDocumentosTests
    {
        private readonly IValidadorDocumentos _validador = new ValidadorDocumentos();

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    MÉTODOS BOOLEANOS                               ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        // ── EsDniValido ─────────────────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z", true)]
        [InlineData("00000000T", true)]
        [InlineData("99999999R", true)]
        [InlineData("12345678A", false)]   // letra incorrecta
        [InlineData("", false)]
        [InlineData("INVALIDO", false)]
        public void EsDniValido_DevuelveResultadoEsperado(string entrada, bool esperado)
        {
            _validador.EsDniValido(entrada).Should().Be(esperado);
        }

        [Fact]
        public void EsDniValido_Null_DevuelveFalse()
        {
            _validador.EsDniValido(null!).Should().BeFalse();
        }

        // ── EsNieValido ─────────────────────────────────────────────────────

        [Theory]
        [InlineData("X1234567L", true)]
        [InlineData("X0000000T", true)]
        [InlineData("Y0000000Z", true)]
        [InlineData("Z0000000M", true)]
        [InlineData("X1234567A", false)]   // letra incorrecta
        [InlineData("A1234567L", false)]   // no empieza por X/Y/Z
        [InlineData("", false)]
        public void EsNieValido_DevuelveResultadoEsperado(string entrada, bool esperado)
        {
            _validador.EsNieValido(entrada).Should().Be(esperado);
        }

        // ── EsNifValido ─────────────────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z", true)]    // DNI como NIF
        [InlineData("X1234567L", true)]    // NIE como NIF
        [InlineData("K0000000T", true)]    // NIF especial K
        [InlineData("L0000000T", true)]    // NIF especial L
        [InlineData("M0000000T", true)]    // NIF especial M
        [InlineData("12345678A", false)]   // DNI con letra incorrecta
        [InlineData("", false)]
        public void EsNifValido_DevuelveResultadoEsperado(string entrada, bool esperado)
        {
            _validador.EsNifValido(entrada).Should().Be(esperado);
        }

        // ── EsCifValido ─────────────────────────────────────────────────────

        [Theory]
        [InlineData("A00000000", true)]
        [InlineData("B00000000", true)]
        [InlineData("P0000000J", true)]    // control letra
        [InlineData("G00000000", true)]    // control ambos (dígito)
        [InlineData("G0000000J", true)]    // control ambos (letra)
        [InlineData("A00000001", false)]   // control incorrecto
        [InlineData("", false)]
        public void EsCifValido_DevuelveResultadoEsperado(string entrada, bool esperado)
        {
            _validador.EsCifValido(entrada).Should().Be(esperado);
        }

        // ── EsDocumentoValido ─────────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z", true)]    // DNI
        [InlineData("X1234567L", true)]    // NIE
        [InlineData("A00000000", true)]    // CIF
        [InlineData("K0000000T", true)]    // NIF especial
        [InlineData("", false)]
        [InlineData("INVALIDO!", false)]
        public void EsDocumentoValido_DevuelveResultadoEsperado(string entrada, bool esperado)
        {
            _validador.EsDocumentoValido(entrada).Should().Be(esperado);
        }

        [Fact]
        public void EsDocumentoValido_Null_DevuelveFalse()
        {
            _validador.EsDocumentoValido(null!).Should().BeFalse();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║           MÉTODOS CON EXCEPCIÓN                                    ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        // ── ValidarDni ──────────────────────────────────────────────────────

        [Fact]
        public void ValidarDni_Valido_DevuelveDocumentoValidado()
        {
            var doc = _validador.ValidarDni("12345678Z");

            doc.ValorNormalizado.Should().Be("12345678Z");
            doc.TipoDocumento.Should().Be(TipoDocumento.DNI);
            doc.CaracterControl.Should().Be("Z");
            doc.ParteNumerica.Should().Be("12345678");
        }

        [Fact]
        public void ValidarDni_Invalido_LanzaExcepcion()
        {
            var act = () => _validador.ValidarDni("12345678A");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.TipoDocumento.Should().Be(TipoDocumento.DNI);
        }

        // ── ValidarNie ──────────────────────────────────────────────────────

        [Fact]
        public void ValidarNie_Valido_DevuelveDocumentoValidado()
        {
            var doc = _validador.ValidarNie("X1234567L");

            doc.ValorNormalizado.Should().Be("X1234567L");
            doc.TipoDocumento.Should().Be(TipoDocumento.NIE);
        }

        [Fact]
        public void ValidarNie_Invalido_LanzaExcepcion()
        {
            var act = () => _validador.ValidarNie("X1234567A");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        // ── ValidarNif ──────────────────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z", TipoDocumento.DNI)]
        [InlineData("X1234567L", TipoDocumento.NIE)]
        [InlineData("K0000000T", TipoDocumento.NIF)]
        public void ValidarNif_Valido_DevuelveSubTipoCorrecto(string entrada, TipoDocumento subTipoEsperado)
        {
            var doc = _validador.ValidarNif(entrada);
            doc.TipoDocumento.Should().Be(subTipoEsperado);
        }

        [Fact]
        public void ValidarNif_Invalido_LanzaExcepcion()
        {
            var act = () => _validador.ValidarNif("INVALIDO!!");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        // ── ValidarCif ──────────────────────────────────────────────────────

        [Fact]
        public void ValidarCif_Valido_DevuelveDocumentoValidado()
        {
            var doc = _validador.ValidarCif("A00000000");

            doc.TipoDocumento.Should().Be(TipoDocumento.CIF);
            doc.ValorNormalizado.Should().Be("A00000000");
        }

        [Fact]
        public void ValidarCif_Invalido_LanzaExcepcion()
        {
            var act = () => _validador.ValidarCif("A00000001");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        // ── ValidarDocumento ─────────────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z", TipoDocumento.DNI)]
        [InlineData("X1234567L", TipoDocumento.NIE)]
        [InlineData("A00000000", TipoDocumento.CIF)]
        public void ValidarDocumento_Valido_DetectaTipoCorrecto(string entrada, TipoDocumento tipoEsperado)
        {
            var doc = _validador.ValidarDocumento(entrada);
            doc.TipoDocumento.Should().Be(tipoEsperado);
        }

        [Fact]
        public void ValidarDocumento_Null_LanzaExcepcion()
        {
            var act = () => _validador.ValidarDocumento(null!);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Fact]
        public void ValidarDocumento_Vacio_LanzaExcepcion()
        {
            var act = () => _validador.ValidarDocumento("");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Fact]
        public void ValidarDocumento_TipoDesconocido_LanzaExcepcion()
        {
            var act = () => _validador.ValidarDocumento("ZZZZZZZZZ");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║        MÉTODOS SIN EXCEPCIÓN (RESULTADO)                           ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        // ── IntentarValidarDni ──────────────────────────────────────────────

        [Fact]
        public void IntentarValidarDni_Valido_DevuelveResultadoValido()
        {
            var resultado = _validador.IntentarValidarDni("12345678Z");

            resultado.EsValido.Should().BeTrue();
            resultado.TipoDocumento.Should().Be(TipoDocumento.DNI);
            resultado.CaracterControl.Should().Be("Z");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345678A")]
        [InlineData("INVALIDO")]
        public void IntentarValidarDni_Invalido_DevuelveResultadoInvalido(string? entrada)
        {
            var resultado = _validador.IntentarValidarDni(entrada!);

            resultado.EsValido.Should().BeFalse();
            resultado.TipoDocumento.Should().Be(TipoDocumento.DNI);
        }

        // ── IntentarValidarNie ──────────────────────────────────────────────

        [Fact]
        public void IntentarValidarNie_Valido_DevuelveResultadoValido()
        {
            var resultado = _validador.IntentarValidarNie("X1234567L");

            resultado.EsValido.Should().BeTrue();
            resultado.TipoDocumento.Should().Be(TipoDocumento.NIE);
        }

        [Fact]
        public void IntentarValidarNie_Invalido_DevuelveResultadoInvalido()
        {
            var resultado = _validador.IntentarValidarNie("X1234567A");

            resultado.EsValido.Should().BeFalse();
            resultado.Mensaje.Should().NotBeNullOrEmpty();
        }

        // ── IntentarValidarNif ──────────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z")]
        [InlineData("X1234567L")]
        [InlineData("K0000000T")]
        public void IntentarValidarNif_Valido_DevuelveResultadoValido(string entrada)
        {
            var resultado = _validador.IntentarValidarNif(entrada);
            resultado.EsValido.Should().BeTrue();
        }

        [Fact]
        public void IntentarValidarNif_Invalido_DevuelveResultadoInvalido()
        {
            var resultado = _validador.IntentarValidarNif("INVALIDO!");
            resultado.EsValido.Should().BeFalse();
        }

        // ── IntentarValidarCif ──────────────────────────────────────────────

        [Fact]
        public void IntentarValidarCif_Valido_DevuelveResultadoValido()
        {
            var resultado = _validador.IntentarValidarCif("A00000000");
            resultado.EsValido.Should().BeTrue();
            resultado.TipoDocumento.Should().Be(TipoDocumento.CIF);
        }

        [Fact]
        public void IntentarValidarCif_Invalido_DevuelveResultadoInvalido()
        {
            var resultado = _validador.IntentarValidarCif("A00000001");
            resultado.EsValido.Should().BeFalse();
        }

        // ── IntentarValidarDocumento ─────────────────────────────────────────

        [Theory]
        [InlineData("12345678Z", TipoDocumento.DNI)]
        [InlineData("X1234567L", TipoDocumento.NIE)]
        [InlineData("A00000000", TipoDocumento.CIF)]
        public void IntentarValidarDocumento_Valido_DetectaTipo(string entrada, TipoDocumento tipoEsperado)
        {
            var resultado = _validador.IntentarValidarDocumento(entrada);

            resultado.EsValido.Should().BeTrue();
            resultado.TipoDocumento.Should().Be(tipoEsperado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ZZZZZZZZZ")]
        public void IntentarValidarDocumento_Invalido_DevuelveInvalido(string? entrada)
        {
            var resultado = _validador.IntentarValidarDocumento(entrada!);
            resultado.EsValido.Should().BeFalse();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                   VALIDACIÓN POR LOTES (SÍNCRONA)                  ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public void ValidarLoteDni_MixtoValidoInvalido_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "00000000T", "INVALIDO", "12345678A" };

            var lote = _validador.ValidarLoteDni(documentos);

            lote.TotalProcesados.Should().Be(4);
            lote.TotalValidos.Should().Be(2);
            lote.TotalInvalidos.Should().Be(2);
            lote.Validos.Should().HaveCount(2);
            lote.Invalidos.Should().HaveCount(2);
        }

        [Fact]
        public void ValidarLoteNie_Validos_ResultadoCorrecto()
        {
            var documentos = new[] { "X1234567L", "X0000000T" };

            var lote = _validador.ValidarLoteNie(documentos);

            lote.TodosValidos.Should().BeTrue();
            lote.TotalValidos.Should().Be(2);
        }

        [Fact]
        public void ValidarLoteNif_Mixto_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "X1234567L", "K0000000T", "INVALIDO" };

            var lote = _validador.ValidarLoteNif(documentos);

            lote.TotalValidos.Should().Be(3);
            lote.TotalInvalidos.Should().Be(1);
        }

        [Fact]
        public void ValidarLoteCif_Validos_ResultadoCorrecto()
        {
            var documentos = new[] { "A00000000", "B00000000" };

            var lote = _validador.ValidarLoteCif(documentos);

            lote.TodosValidos.Should().BeTrue();
        }

        [Fact]
        public void ValidarLoteDocumentos_MixtoTipos_ResultadoCorrecto()
        {
            var documentos = new[] { "12345678Z", "X1234567L", "A00000000", "INVALIDO" };

            var lote = _validador.ValidarLoteDocumentos(documentos);

            lote.TotalProcesados.Should().Be(4);
            lote.TotalValidos.Should().Be(3);
            lote.TotalInvalidos.Should().Be(1);
            lote.TieneValidos.Should().BeTrue();
            lote.TieneInvalidos.Should().BeTrue();
        }

        [Fact]
        public void ValidarLoteDocumentos_Null_DevuelveVacio()
        {
            var lote = _validador.ValidarLoteDocumentos(null!);

            lote.TotalProcesados.Should().Be(0);
            lote.Should().BeSameAs(ResultadoValidacionLote.Vacio());
        }

        [Fact]
        public void ValidarLoteDocumentos_Vacio_DevuelveVacio()
        {
            var lote = _validador.ValidarLoteDocumentos(Array.Empty<string>());

            lote.TotalProcesados.Should().Be(0);
        }

        [Fact]
        public void ValidarLoteDni_TodosInvalidos_EstadisticaCorrecta()
        {
            var documentos = new[] { "INVALIDO1", "INVALIDO2", "INVALIDO3" };

            var lote = _validador.ValidarLoteDni(documentos);

            lote.TodosInvalidos.Should().BeTrue();
            lote.TodosValidos.Should().BeFalse();
            lote.TieneValidos.Should().BeFalse();
        }

        [Fact]
        public void ValidarLoteDocumentos_VerificaPorcentajes()
        {
            var documentos = new[] { "12345678Z", "INVALIDO" };

            var lote = _validador.ValidarLoteDocumentos(documentos);

            lote.PorcentajeValidos.Should().Be(50);
            lote.PorcentajeInvalidos.Should().Be(50);
        }

        [Fact]
        public void ValidarLoteDocumentos_ResultadosIndividuales_TienenDetalle()
        {
            var documentos = new[] { "12345678Z", "INVALIDO" };

            var lote = _validador.ValidarLoteDocumentos(documentos);

            lote.ResultadosIndividuales.Should().HaveCount(2);
            lote.ResultadosIndividuales.Should().Contain(r => r.EsValido);
            lote.ResultadosIndividuales.Should().Contain(r => !r.EsValido);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                     UTILIDADES Y HERRAMIENTAS                       ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData("12345678Z", TipoDocumento.DNI)]
        [InlineData("X1234567L", TipoDocumento.NIE)]
        [InlineData("K0000000T", TipoDocumento.NIF)]
        [InlineData("A00000000", TipoDocumento.CIF)]
        [InlineData("INVALIDO!", TipoDocumento.Desconocido)]
        public void DetectarTipo_DevuelveTipoCorrecto(string entrada, TipoDocumento esperado)
        {
            _validador.DetectarTipo(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData("12.345.678-z", "12345678Z")]
        [InlineData("X 1234567 L", "X1234567L")]
        [InlineData("  abc  ", "ABC")]
        public void Normalizar_DevuelveNormalizado(string entrada, string esperado)
        {
            _validador.Normalizar(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData(0, 'T')]
        [InlineData(1, 'R')]
        [InlineData(12345678, 'Z')]
        public void ObtenerLetraDni_DevuelveLetraCorrecta(int numero, char esperada)
        {
            _validador.ObtenerLetraDni(numero).Should().Be(esperada);
        }

        [Fact]
        public void ObtenerLetraNie_DevuelveLetraCorrecta()
        {
            _validador.ObtenerLetraNie("X1234567").Should().Be('L');
        }

        [Fact]
        public void ObtenerControlCif_DevuelveTupla()
        {
            var (digito, letra) = _validador.ObtenerControlCif("A0000000");
            digito.Should().Be('0');
            letra.Should().Be('J');
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                  NORMALIZACIÓN EDGE CASES                          ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public void ValidarDni_ConEspaciosYGuiones_FuncionaCorrectamente()
        {
            var doc = _validador.ValidarDni("12.345.678-Z");
            doc.ValorNormalizado.Should().Be("12345678Z");
        }

        [Fact]
        public void ValidarNie_ConMinusculas_FuncionaCorrectamente()
        {
            var doc = _validador.ValidarNie("x1234567l");
            doc.ValorNormalizado.Should().Be("X1234567L");
        }

        [Fact]
        public void ValidarCif_ConEspacios_FuncionaCorrectamente()
        {
            var doc = _validador.ValidarCif("  A00000000  ");
            doc.ValorNormalizado.Should().Be("A00000000");
        }
    }
}
