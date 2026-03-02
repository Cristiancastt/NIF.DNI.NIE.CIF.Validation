using FluentAssertions;
using nif_dni_nie_cif_validation.Excepciones;
using nif_dni_nie_cif_validation.Modelos;

namespace nif_dni_nie_cif_validation.Tests.Modelos
{
    /// <summary>
    /// Tests completos para el Value Object Nie.
    /// Cubre: creación válida con X/Y/Z, normalización, errores de formato, letra de control, Intentar.
    /// </summary>
    public class NieTests
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Crear — Casos válidos
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("X0000000T", 'X', "0000000", 'T')]
        [InlineData("Y0000000Z", 'Y', "0000000", 'Z')]
        [InlineData("Z0000000M", 'Z', "0000000", 'M')]
        [InlineData("X1234567L", 'X', "1234567", 'L')]
        public void Crear_NieValido_DevuelveNieCorrecto(
            string entrada, char letraInicialEsperada, string parteNumEsperada, char letraControlEsperada)
        {
            var nie = Nie.Crear(entrada);

            nie.LetraInicial.Should().Be(letraInicialEsperada);
            nie.ParteNumerica.Should().Be(parteNumEsperada);
            nie.LetraControl.Should().Be(letraControlEsperada);
            nie.CaracterControl.Should().Be(letraControlEsperada.ToString());
            nie.Tipo.Should().Be(TipoDocumento.NIE);
        }

        [Theory]
        [InlineData("x1234567l", "X1234567L")]      // minúsculas
        [InlineData("X-1234567-L", "X1234567L")]     // guiones
        [InlineData("  X1234567L  ", "X1234567L")]   // espacios
        [InlineData("X 1234567 L", "X1234567L")]     // espacios internos
        public void Crear_NieConFormatoSucio_NormalizaCorrectamente(string entrada, string esperado)
        {
            var nie = Nie.Crear(entrada);
            nie.Valor.Should().Be(esperado);
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
            var act = () => Nie.Crear(entrada!);
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.CodigoError.Should().Be("NIE_NO_VALIDO");
        }

        [Theory]
        [InlineData("X123456L")]       // 8 chars (corto)
        [InlineData("X12345678L")]     // 10 chars (largo)
        public void Crear_LongitudIncorrecta_LanzaExcepcion(string entrada)
        {
            var act = () => Nie.Crear(entrada);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Theory]
        [InlineData("A1234567L")]      // letra inicial no válida
        [InlineData("K1234567L")]      // K es NIF especial, no NIE
        [InlineData("B1234567L")]      // B es CIF
        public void Crear_LetraInicialInvalida_LanzaExcepcion(string entrada)
        {
            var act = () => Nie.Crear(entrada);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Fact]
        public void Crear_ParteNumericaConLetras_LanzaExcepcion()
        {
            var act = () => Nie.Crear("X12345A7L");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("dígitos");
        }

        [Fact]
        public void Crear_LetraControlIncorrecta_LanzaExcepcion()
        {
            // X1234567L es válido → X1234567A debería ser inválido
            var act = () => Nie.Crear("X1234567A");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("letra de control");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Intentar
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Intentar_NieValido_DevuelveTrue()
        {
            bool resultado = Nie.Intentar("X1234567L", out var nie);

            resultado.Should().BeTrue();
            nie.Should().NotBeNull();
            nie!.Valor.Should().Be("X1234567L");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("INVALIDO")]
        [InlineData("X1234567A")]
        public void Intentar_NieInvalido_DevuelveFalse(string? entrada)
        {
            bool resultado = Nie.Intentar(entrada!, out var nie);

            resultado.Should().BeFalse();
            nie.Should().BeNull();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Algoritmo NIE — X=0, Y=1, Z=2
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("X0000000T")]  // X→0, num=0000000 → 0 % 23 = 0 → T
        [InlineData("Y0000000Z")]  // Y→1, num=0000000 → 10000000 % 23 = 3 → A... Verifiquemos
        [InlineData("Z0000000M")]  // Z→2, num=0000000 → 20000000 % 23 → ?
        public void Crear_AlgoritmoNie_CalculaLetraCorrecta(string nie)
        {
            // Si la creación no lanza excepción, el algoritmo es correcto
            var resultado = Nie.Crear(nie);
            resultado.Valor.Should().Be(nie);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ValueObject — Igualdad
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Equals_MismoNie_SonIguales()
        {
            var nie1 = Nie.Crear("X1234567L");
            var nie2 = Nie.Crear("X1234567L");

            nie1.Should().Be(nie2);
            nie1.GetHashCode().Should().Be(nie2.GetHashCode());
        }

        [Fact]
        public void ConversionImplicita_AString_DevuelveValor()
        {
            var nie = Nie.Crear("X1234567L");
            string texto = nie;
            texto.Should().Be("X1234567L");
        }
    }
}
