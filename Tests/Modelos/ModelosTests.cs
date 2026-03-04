using FluentAssertions;
using NIF.DNI.NIE.CIF.Validation.Modelos;

namespace NIF.DNI.NIE.CIF.Validation.Tests.Modelos
{
    /// <summary>
    /// Tests para ResultadoValidacion, ErrorValidacion, DocumentoValidado, ResultadoValidacionLote.
    /// </summary>
    public class ModelosTests
    {
        // ═══════════════════════════════════════════════════════════════════
        //  ResultadoValidacion
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void ResultadoValidacion_Valido_PropiedadesCorrectas()
        {
            var resultado = ResultadoValidacion.Valido("12345678z", "12345678Z", TipoDocumento.DNI, "Z");

            resultado.EsValido.Should().BeTrue();
            resultado.ValorOriginal.Should().Be("12345678z");
            resultado.ValorNormalizado.Should().Be("12345678Z");
            resultado.TipoDocumento.Should().Be(TipoDocumento.DNI);
            resultado.CaracterControl.Should().Be("Z");
            resultado.Mensaje.Should().Contain("válido");
        }

        [Fact]
        public void ResultadoValidacion_Invalido_PropiedadesCorrectas()
        {
            var resultado = ResultadoValidacion.Invalido("abc", "ABC", TipoDocumento.DNI, "Formato incorrecto");

            resultado.EsValido.Should().BeFalse();
            resultado.ValorOriginal.Should().Be("abc");
            resultado.Mensaje.Should().Be("Formato incorrecto");
        }

        [Fact]
        public void ResultadoValidacion_ToString_Valido_ContieneValido()
        {
            var resultado = ResultadoValidacion.Valido("12345678Z", "12345678Z", TipoDocumento.DNI);
            resultado.ToString().Should().Contain("VÁLIDO");
        }

        [Fact]
        public void ResultadoValidacion_ToString_Invalido_ContieneInvalido()
        {
            var resultado = ResultadoValidacion.Invalido("abc", "ABC", TipoDocumento.DNI, "error");
            resultado.ToString().Should().Contain("INVÁLIDO");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ErrorValidacion
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void ErrorValidacion_Constructor_PropiedadesCorrectas()
        {
            var error = new ErrorValidacion("abc", "ABC", "Error de formato", TipoDocumento.DNI, "FORMATO_INVALIDO");

            error.Valor.Should().Be("abc");
            error.ValorNormalizado.Should().Be("ABC");
            error.Mensaje.Should().Be("Error de formato");
            error.TipoDocumentoEsperado.Should().Be(TipoDocumento.DNI);
            error.CodigoError.Should().Be("FORMATO_INVALIDO");
        }

        [Fact]
        public void ErrorValidacion_DesdeResultado_CreaCorrectamente()
        {
            var resultado = ResultadoValidacion.Invalido("abc", "ABC", TipoDocumento.NIE, "Formato incorrecto");
            var error = ErrorValidacion.DesdeResultado(resultado);

            error.Valor.Should().Be("abc");
            error.Mensaje.Should().Be("Formato incorrecto");
            error.TipoDocumentoEsperado.Should().Be(TipoDocumento.NIE);
        }

        [Fact]
        public void ErrorValidacion_ToString_ContieneCodigoYValor()
        {
            var error = new ErrorValidacion("abc", "ABC", "Error", TipoDocumento.DNI, "COD_ERR");
            error.ToString().Should().Contain("COD_ERR").And.Contain("abc");
        }

        [Fact]
        public void ErrorValidacion_CodigoErrorPorDefecto_EsValidacionFallida()
        {
            var error = new ErrorValidacion("abc", "ABC", "Error");
            error.CodigoError.Should().Be("VALIDACION_FALLIDA");
            error.TipoDocumentoEsperado.Should().Be(TipoDocumento.Desconocido);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  DocumentoValidado
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void DocumentoValidado_Constructor_PropiedadesCorrectas()
        {
            var doc = new DocumentoValidado("12345678z", "12345678Z", TipoDocumento.DNI, "Z", "12345678");

            doc.ValorOriginal.Should().Be("12345678z");
            doc.ValorNormalizado.Should().Be("12345678Z");
            doc.TipoDocumento.Should().Be(TipoDocumento.DNI);
            doc.CaracterControl.Should().Be("Z");
            doc.ParteNumerica.Should().Be("12345678");
        }

        [Fact]
        public void DocumentoValidado_DesdeResultado_CreaCorrectamente()
        {
            var resultado = ResultadoValidacion.Valido("12345678Z", "12345678Z", TipoDocumento.DNI, "Z");
            var doc = DocumentoValidado.DesdeResultado(resultado, "12345678");

            doc.ValorOriginal.Should().Be("12345678Z");
            doc.CaracterControl.Should().Be("Z");
            doc.ParteNumerica.Should().Be("12345678");
        }

        [Fact]
        public void DocumentoValidado_ToString_ContieneInfo()
        {
            var doc = new DocumentoValidado("orig", "12345678Z", TipoDocumento.DNI, "Z", "12345678");
            doc.ToString().Should().Contain("DNI").And.Contain("12345678Z");
        }

        [Fact]
        public void DocumentoValidado_Equals_MismoValor_SonIguales()
        {
            var doc1 = new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678");
            var doc2 = new DocumentoValidado("b", "12345678Z", TipoDocumento.DNI, "Z", "12345678");

            doc1.Equals(doc2).Should().BeTrue();
            doc1.GetHashCode().Should().Be(doc2.GetHashCode());
        }

        [Fact]
        public void DocumentoValidado_Equals_DiferenteValor_NoSonIguales()
        {
            var doc1 = new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678");
            var doc2 = new DocumentoValidado("b", "00000000T", TipoDocumento.DNI, "T", "00000000");

            doc1.Equals(doc2).Should().BeFalse();
        }

        [Fact]
        public void DocumentoValidado_Equals_ConNull_DevuelveFalse()
        {
            var doc = new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678");
            doc.Equals(null).Should().BeFalse();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  TipoDocumento enum
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void TipoDocumento_ValoresCorrectos()
        {
            ((int)TipoDocumento.DNI).Should().Be(0);
            ((int)TipoDocumento.NIE).Should().Be(1);
            ((int)TipoDocumento.NIF).Should().Be(2);
            ((int)TipoDocumento.CIF).Should().Be(3);
            ((int)TipoDocumento.Desconocido).Should().Be(-1);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ResultadoValidacionLote
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void ResultadoValidacionLote_Vacio_EsSingleton()
        {
            var lote1 = ResultadoValidacionLote.Vacio();
            var lote2 = ResultadoValidacionLote.Vacio();

            lote1.Should().BeSameAs(lote2);
            lote1.TotalProcesados.Should().Be(0);
            lote1.TotalValidos.Should().Be(0);
            lote1.TotalInvalidos.Should().Be(0);
            lote1.Validos.Should().BeEmpty();
            lote1.Invalidos.Should().BeEmpty();
            lote1.ResultadosIndividuales.Should().BeEmpty();
        }

        [Fact]
        public void ResultadoValidacionLote_ConDatos_EstadisticasCorrectas()
        {
            var validos = new[]
            {
                new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678"),
                new DocumentoValidado("b", "X1234567L", TipoDocumento.NIE, "L", "1234567")
            };
            var invalidos = new[]
            {
                new ErrorValidacion("c", "CCC", "error", TipoDocumento.DNI)
            };
            var resultados = new[]
            {
                ResultadoValidacion.Valido("a", "12345678Z", TipoDocumento.DNI, "Z"),
                ResultadoValidacion.Valido("b", "X1234567L", TipoDocumento.NIE, "L"),
                ResultadoValidacion.Invalido("c", "CCC", TipoDocumento.DNI, "error")
            };

            var lote = new ResultadoValidacionLote(validos, invalidos, resultados, 3);

            lote.TotalProcesados.Should().Be(3);
            lote.TotalValidos.Should().Be(2);
            lote.TotalInvalidos.Should().Be(1);
            lote.TodosValidos.Should().BeFalse();
            lote.TodosInvalidos.Should().BeFalse();
            lote.TieneValidos.Should().BeTrue();
            lote.TieneInvalidos.Should().BeTrue();
            lote.PorcentajeValidos.Should().BeApproximately(66.67, 0.01);
            lote.PorcentajeInvalidos.Should().BeApproximately(33.33, 0.01);
        }

        [Fact]
        public void ResultadoValidacionLote_TodosValidos_EstadisticaCorrecta()
        {
            var validos = new[]
            {
                new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678")
            };
            var resultados = new[]
            {
                ResultadoValidacion.Valido("a", "12345678Z", TipoDocumento.DNI, "Z")
            };

            var lote = new ResultadoValidacionLote(validos, Array.Empty<ErrorValidacion>(), resultados, 1);

            lote.TodosValidos.Should().BeTrue();
            lote.TodosInvalidos.Should().BeFalse();
            lote.PorcentajeValidos.Should().Be(100);
        }

        [Fact]
        public void ResultadoValidacionLote_TodosInvalidos_EstadisticaCorrecta()
        {
            var invalidos = new[]
            {
                new ErrorValidacion("a", "A", "error", TipoDocumento.DNI)
            };
            var resultados = new[]
            {
                ResultadoValidacion.Invalido("a", "A", TipoDocumento.DNI, "error")
            };

            var lote = new ResultadoValidacionLote(Array.Empty<DocumentoValidado>(), invalidos, resultados, 1);

            lote.TodosInvalidos.Should().BeTrue();
            lote.TodosValidos.Should().BeFalse();
            lote.PorcentajeInvalidos.Should().Be(100);
        }

        [Fact]
        public void ResultadoValidacionLote_ObtenerValidosPorTipo_FiltraCorrecto()
        {
            var validos = new[]
            {
                new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678"),
                new DocumentoValidado("b", "X1234567L", TipoDocumento.NIE, "L", "1234567"),
                new DocumentoValidado("c", "00000000T", TipoDocumento.DNI, "T", "00000000")
            };

            var lote = new ResultadoValidacionLote(validos, Array.Empty<ErrorValidacion>(), Array.Empty<ResultadoValidacion>(), 3);

            lote.ObtenerValidosPorTipo(TipoDocumento.DNI).Should().HaveCount(2);
            lote.ObtenerValidosPorTipo(TipoDocumento.NIE).Should().HaveCount(1);
            lote.ObtenerValidosPorTipo(TipoDocumento.CIF).Should().BeEmpty();
        }

        [Fact]
        public void ResultadoValidacionLote_ObtenerInvalidosPorTipo_FiltraCorrecto()
        {
            var invalidos = new[]
            {
                new ErrorValidacion("a", "A", "error", TipoDocumento.DNI),
                new ErrorValidacion("b", "B", "error", TipoDocumento.DNI),
                new ErrorValidacion("c", "C", "error", TipoDocumento.NIE)
            };

            var lote = new ResultadoValidacionLote(Array.Empty<DocumentoValidado>(), invalidos, Array.Empty<ResultadoValidacion>(), 3);

            lote.ObtenerInvalidosPorTipo(TipoDocumento.DNI).Should().HaveCount(2);
            lote.ObtenerInvalidosPorTipo(TipoDocumento.NIE).Should().HaveCount(1);
            lote.ObtenerInvalidosPorTipo(TipoDocumento.CIF).Should().BeEmpty();
        }

        [Fact]
        public void ResultadoValidacionLote_TieneValidosDeTipo_DevuelveCorrectos()
        {
            var validos = new[]
            {
                new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678")
            };

            var lote = new ResultadoValidacionLote(validos, Array.Empty<ErrorValidacion>(), Array.Empty<ResultadoValidacion>(), 1);

            lote.TieneValidosDeTipo(TipoDocumento.DNI).Should().BeTrue();
            lote.TieneValidosDeTipo(TipoDocumento.NIE).Should().BeFalse();
        }

        [Fact]
        public void ResultadoValidacionLote_ContarValidosPorTipo_DevuelveConteo()
        {
            var validos = new[]
            {
                new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678"),
                new DocumentoValidado("b", "00000000T", TipoDocumento.DNI, "T", "00000000")
            };

            var lote = new ResultadoValidacionLote(validos, Array.Empty<ErrorValidacion>(), Array.Empty<ResultadoValidacion>(), 2);

            lote.ContarValidosPorTipo(TipoDocumento.DNI).Should().Be(2);
            lote.ContarValidosPorTipo(TipoDocumento.CIF).Should().Be(0);
        }

        [Fact]
        public void ResultadoValidacionLote_ObtenerTiposValidos_DevuelveTiposPresentes()
        {
            var validos = new[]
            {
                new DocumentoValidado("a", "12345678Z", TipoDocumento.DNI, "Z", "12345678"),
                new DocumentoValidado("b", "X1234567L", TipoDocumento.NIE, "L", "1234567")
            };

            var lote = new ResultadoValidacionLote(validos, Array.Empty<ErrorValidacion>(), Array.Empty<ResultadoValidacion>(), 2);

            lote.ObtenerTiposValidos().Should().Contain(TipoDocumento.DNI).And.Contain(TipoDocumento.NIE);
        }

        [Fact]
        public void ResultadoValidacionLote_Resumen_ContieneEstadisticas()
        {
            var lote = new ResultadoValidacionLote(
                Array.Empty<DocumentoValidado>(),
                Array.Empty<ErrorValidacion>(),
                Array.Empty<ResultadoValidacion>(),
                5);

            lote.Resumen.Should().Contain("Procesados: 5");
        }

        [Fact]
        public void ResultadoValidacionLote_ToString_DevuelveResumen()
        {
            var lote = ResultadoValidacionLote.Vacio();
            lote.ToString().Should().Be(lote.Resumen);
        }

        [Fact]
        public void ResultadoValidacionLote_ConstructorConNulls_UsaArraysVacios()
        {
            var lote = new ResultadoValidacionLote(null!, null!, null!, 0);

            lote.Validos.Should().BeEmpty();
            lote.Invalidos.Should().BeEmpty();
            lote.ResultadosIndividuales.Should().BeEmpty();
        }
    }
}
