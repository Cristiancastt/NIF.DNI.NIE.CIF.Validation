using FluentAssertions;
using NIF.DNI.NIE.CIF.Validation.Excepciones;
using NIF.DNI.NIE.CIF.Validation.Modelos;

namespace NIF.DNI.NIE.CIF.Validation.Tests.Excepciones
{
    /// <summary>
    /// Tests para DocumentoNoValidoException y sus factories.
    /// </summary>
    public class DocumentoNoValidoExceptionTests
    {
        [Fact]
        public void DniInvalido_PropiedadesCorrectas()
        {
            var ex = DocumentoNoValidoException.DniInvalido("12345678A", "Letra incorrecta");

            ex.Valor.Should().Be("12345678A");
            ex.TipoDocumento.Should().Be(TipoDocumento.DNI);
            ex.CodigoError.Should().Be("DNI_NO_VALIDO");
            ex.Message.Should().Be("Letra incorrecta");
        }

        [Fact]
        public void NieInvalido_PropiedadesCorrectas()
        {
            var ex = DocumentoNoValidoException.NieInvalido("X0000000A", "Letra incorrecta");

            ex.TipoDocumento.Should().Be(TipoDocumento.NIE);
            ex.CodigoError.Should().Be("NIE_NO_VALIDO");
        }

        [Fact]
        public void NifInvalido_PropiedadesCorrectas()
        {
            var ex = DocumentoNoValidoException.NifInvalido("K0000000A", "Letra incorrecta");

            ex.TipoDocumento.Should().Be(TipoDocumento.NIF);
            ex.CodigoError.Should().Be("NIF_NO_VALIDO");
        }

        [Fact]
        public void CifInvalido_PropiedadesCorrectas()
        {
            var ex = DocumentoNoValidoException.CifInvalido("B00000001", "Control incorrecto");

            ex.TipoDocumento.Should().Be(TipoDocumento.CIF);
            ex.CodigoError.Should().Be("CIF_NO_VALIDO");
        }

        [Fact]
        public void TipoDesconocido_PropiedadesCorrectas()
        {
            var ex = DocumentoNoValidoException.TipoDesconocido("XXXXX");

            ex.TipoDocumento.Should().Be(TipoDocumento.Desconocido);
            ex.CodigoError.Should().Be("TIPO_DESCONOCIDO");
            ex.Message.Should().Contain("XXXXX");
        }

        [Fact]
        public void ValorVacio_PropiedadesCorrectas()
        {
            var ex = DocumentoNoValidoException.ValorVacio();

            ex.Valor.Should().BeEmpty();
            ex.TipoDocumento.Should().Be(TipoDocumento.Desconocido);
            ex.CodigoError.Should().Be("VALOR_VACIO");
        }

        [Fact]
        public void Constructor_ConInnerException_Funciona()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new DocumentoNoValidoException("val", TipoDocumento.DNI, "msg", inner, "COD");

            ex.InnerException.Should().BeSameAs(inner);
            ex.Valor.Should().Be("val");
            ex.CodigoError.Should().Be("COD");
        }

        [Fact]
        public void Exception_EsSerializable_HeredaDeException()
        {
            var ex = DocumentoNoValidoException.DniInvalido("val", "msg");
            ex.Should().BeAssignableTo<Exception>();
        }
    }
}
