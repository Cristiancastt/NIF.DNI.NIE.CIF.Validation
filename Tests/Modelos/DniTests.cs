using FluentAssertions;
using nif_dni_nie_cif_validation.Excepciones;
using nif_dni_nie_cif_validation.Modelos;

namespace nif_dni_nie_cif_validation.Tests.Modelos
{
    /// <summary>
    /// Tests completos para el Value Object Dni.
    /// Cubre: creación válida, normalización, errores de formato, letra de control, DesdeNumero, Intentar.
    /// </summary>
    public class DniTests
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Crear — Casos válidos
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("12345678Z", "12345678Z", "12345678", 12345678, 'Z')]
        [InlineData("00000000T", "00000000T", "00000000", 0, 'T')]
        [InlineData("99999999R", "99999999R", "99999999", 99999999, 'R')]
        [InlineData("00000001R", "00000001R", "00000001", 1, 'R')]
        [InlineData("11111111H", "11111111H", "11111111", 11111111, 'H')]
        public void Crear_DniValido_DevuelveDniCorrecto(
            string entrada, string valorEsperado, string parteNumEsperada, int numeroEsperado, char letraEsperada)
        {
            var dni = Dni.Crear(entrada);

            dni.Valor.Should().Be(valorEsperado);
            dni.ParteNumerica.Should().Be(parteNumEsperada);
            dni.Numero.Should().Be(numeroEsperado);
            dni.Letra.Should().Be(letraEsperada);
            dni.CaracterControl.Should().Be(letraEsperada.ToString());
            dni.Tipo.Should().Be(TipoDocumento.DNI);
        }

        [Theory]
        [InlineData("12345678z", "12345678Z")]      // minúscula → mayúscula
        [InlineData("12.345.678-Z", "12345678Z")]    // puntos y guiones
        [InlineData("  12345678Z  ", "12345678Z")]   // espacios
        [InlineData("12 345 678 Z", "12345678Z")]    // espacios internos
        public void Crear_DniConFormatoSucio_NormalizaCorrectamente(string entrada, string esperado)
        {
            var dni = Dni.Crear(entrada);
            dni.Valor.Should().Be(esperado);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Crear — Casos inválidos (excepción)
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Crear_ValorNuloOVacio_LanzaExcepcion(string? entrada)
        {
            var act = () => Dni.Crear(entrada!);
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.CodigoError.Should().Be("DNI_NO_VALIDO");
        }

        [Theory]
        [InlineData("1234567Z")]       // 8 chars (corto)
        [InlineData("123456789Z")]     // 10 chars (largo)
        [InlineData("12345")]          // muy corto
        public void Crear_LongitudIncorrecta_LanzaExcepcion(string entrada)
        {
            var act = () => Dni.Crear(entrada);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Theory]
        [InlineData("1234567AB")]      // letra en parte numérica
        [InlineData("ABCDEFGHZ")]      // todo letras
        public void Crear_ParteNumericaInvalida_LanzaExcepcion(string entrada)
        {
            var act = () => Dni.Crear(entrada);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Fact]
        public void Crear_LetraControlIncorrecta_LanzaExcepcion()
        {
            // 12345678Z es válido, por tanto 12345678A es inválido
            var act = () => Dni.Crear("12345678A");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("letra de control");
        }

        [Fact]
        public void Crear_UltimoCaracterNoEsLetra_LanzaExcepcion()
        {
            var act = () => Dni.Crear("123456789");
            act.Should().Throw<DocumentoNoValidoException>();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Intentar — Try pattern
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Intentar_DniValido_DevuelveTrue()
        {
            bool resultado = Dni.Intentar("12345678Z", out var dni);

            resultado.Should().BeTrue();
            dni.Should().NotBeNull();
            dni!.Valor.Should().Be("12345678Z");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("INVALIDO")]
        [InlineData("12345678A")]
        public void Intentar_DniInvalido_DevuelveFalse(string? entrada)
        {
            bool resultado = Dni.Intentar(entrada!, out var dni);

            resultado.Should().BeFalse();
            dni.Should().BeNull();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  DesdeNumero — Factory desde entero
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(0, "00000000T")]
        [InlineData(1, "00000001R")]
        [InlineData(12345678, "12345678Z")]
        [InlineData(99999999, "99999999R")]
        public void DesdeNumero_NumeroValido_GeneraDniCorrecto(int numero, string valorEsperado)
        {
            var dni = Dni.DesdeNumero(numero);

            dni.Valor.Should().Be(valorEsperado);
            dni.Numero.Should().Be(numero);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100000000)]
        public void DesdeNumero_FueraDeRango_LanzaArgumentException(int numero)
        {
            var act = () => Dni.DesdeNumero(numero);
            act.Should().Throw<ArgumentException>();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ValueObject — Igualdad y conversión implícita
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Equals_MismoValor_SonIguales()
        {
            var dni1 = Dni.Crear("12345678Z");
            var dni2 = Dni.Crear("12345678Z");

            dni1.Should().Be(dni2);
            (dni1 == dni2).Should().BeTrue();
            (dni1 != dni2).Should().BeFalse();
            dni1.GetHashCode().Should().Be(dni2.GetHashCode());
        }

        [Fact]
        public void Equals_DiferenteValor_NoSonIguales()
        {
            var dni1 = Dni.Crear("12345678Z");
            var dni2 = Dni.Crear("00000000T");

            (dni1 == dni2).Should().BeFalse();
            (dni1 != dni2).Should().BeTrue();
        }

        [Fact]
        public void ConversionImplicita_AString_DevuelveValor()
        {
            var dni = Dni.Crear("12345678Z");
            string texto = dni;
            texto.Should().Be("12345678Z");
        }

        [Fact]
        public void ToString_DevuelveValor()
        {
            var dni = Dni.Crear("12345678Z");
            dni.ToString().Should().Be("12345678Z");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Tabla completa de letras DNI (módulo 23)
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(0, 'T')]
        [InlineData(1, 'R')]
        [InlineData(2, 'W')]
        [InlineData(3, 'A')]
        [InlineData(4, 'G')]
        [InlineData(5, 'M')]
        [InlineData(6, 'Y')]
        [InlineData(7, 'F')]
        [InlineData(8, 'P')]
        [InlineData(9, 'D')]
        [InlineData(10, 'X')]
        [InlineData(11, 'B')]
        [InlineData(12, 'N')]
        [InlineData(13, 'J')]
        [InlineData(14, 'Z')]
        [InlineData(15, 'S')]
        [InlineData(16, 'Q')]
        [InlineData(17, 'V')]
        [InlineData(18, 'H')]
        [InlineData(19, 'L')]
        [InlineData(20, 'C')]
        [InlineData(21, 'K')]
        [InlineData(22, 'E')]
        public void DesdeNumero_CadaRestoMod23_GeneraLetraCorrecta(int resto, char letraEsperada)
        {
            var dni = Dni.DesdeNumero(resto);
            dni.Letra.Should().Be(letraEsperada);
        }
    }
}
