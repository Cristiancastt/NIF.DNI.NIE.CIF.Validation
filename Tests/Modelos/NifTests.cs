using FluentAssertions;
using NIF.DNI.NIE.CIF.Validation.Excepciones;
using NIF.DNI.NIE.CIF.Validation.Modelos;

namespace NIF.DNI.NIE.CIF.Validation.Tests.Modelos
{
    /// <summary>
    /// Tests completos para el Value Object Nif.
    /// Cubre: DNI como NIF, NIE como NIF, NIF especial (K/L/M), errores.
    /// </summary>
    public class NifTests
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Crear — DNI como NIF
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_DniValido_DevuelveNifComoDni()
        {
            var nif = Nif.Crear("12345678Z");

            nif.Valor.Should().Be("12345678Z");
            nif.SubTipo.Should().Be(TipoDocumento.DNI);
            nif.Tipo.Should().Be(TipoDocumento.NIF);
            nif.DescripcionTipo.Should().Contain("Identidad");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Crear — NIE como NIF
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_NieValido_DevuelveNifComoNie()
        {
            var nif = Nif.Crear("X1234567L");

            nif.Valor.Should().Be("X1234567L");
            nif.SubTipo.Should().Be(TipoDocumento.NIE);
            nif.DescripcionTipo.Should().Contain("Extranjero");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Crear — NIF especial (K, L, M)
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_NifEspecialK_DevuelveNifCorrecto()
        {
            // K debe usar el mismo algoritmo: número % 23 → letra
            // K0000000T → parteNum=0000000, 0 % 23 = 0 → T
            var nif = Nif.Crear("K0000000T");

            nif.Valor.Should().Be("K0000000T");
            nif.SubTipo.Should().Be(TipoDocumento.NIF);
            nif.DescripcionTipo.Should().Contain("menor de 14");
        }

        [Fact]
        public void Crear_NifEspecialL_DevuelveNifCorrecto()
        {
            var nif = Nif.Crear("L0000000T");

            nif.SubTipo.Should().Be(TipoDocumento.NIF);
            nif.DescripcionTipo.Should().Contain("extranjero");
        }

        [Fact]
        public void Crear_NifEspecialM_DevuelveNifCorrecto()
        {
            var nif = Nif.Crear("M0000000T");

            nif.SubTipo.Should().Be(TipoDocumento.NIF);
            nif.DescripcionTipo.Should().Contain("sin NIE");
        }

        [Theory]
        [InlineData("k0000000t", "K0000000T")]  // minúsculas
        [InlineData("K-0000000-T", "K0000000T")] // guiones
        public void Crear_NifEspecialConFormatoSucio_NormalizaCorrectamente(string entrada, string esperado)
        {
            var nif = Nif.Crear(entrada);
            nif.Valor.Should().Be(esperado);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Crear — Casos inválidos
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Crear_ValorNuloOVacio_LanzaExcepcion(string? entrada)
        {
            var act = () => Nif.Crear(entrada!);
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.CodigoError.Should().Be("NIF_NO_VALIDO");
        }

        [Fact]
        public void Crear_LongitudIncorrecta_LanzaExcepcion()
        {
            var act = () => Nif.Crear("12345");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Fact]
        public void Crear_PrimerCaracterInvalido_LanzaExcepcion()
        {
            // W es letra de CIF, no NIF
            var act = () => Nif.Crear("W1234567A");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("empezar por");
        }

        [Fact]
        public void Crear_NifEspecialConParteNumericaInvalida_LanzaExcepcion()
        {
            var act = () => Nif.Crear("K12345A7T");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("dígitos");
        }

        [Fact]
        public void Crear_NifEspecialConLetraControlIncorrecta_LanzaExcepcion()
        {
            // K0000000T es válido → K0000000A es inválido
            var act = () => Nif.Crear("K0000000A");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("letra de control");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Intentar
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("12345678Z")]
        [InlineData("X1234567L")]
        [InlineData("K0000000T")]
        public void Intentar_NifValido_DevuelveTrue(string entrada)
        {
            bool resultado = Nif.Intentar(entrada, out var nif);

            resultado.Should().BeTrue();
            nif.Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("INVALIDO")]
        [InlineData("12345678A")]
        public void Intentar_NifInvalido_DevuelveFalse(string? entrada)
        {
            bool resultado = Nif.Intentar(entrada!, out var nif);

            resultado.Should().BeFalse();
            nif.Should().BeNull();
        }
    }
}
