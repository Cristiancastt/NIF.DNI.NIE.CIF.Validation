using FluentAssertions;
using nif_dni_nie_cif_validation.Modelos;
using nif_dni_nie_cif_validation.Utiles;

namespace nif_dni_nie_cif_validation.Tests.Utiles
{
    /// <summary>
    /// Tests completos para UtilesValidacion:
    /// Normalizar, DetectarTipoDocumento, CalcularLetraDni, CalcularLetraNie,
    /// CalcularLetraNifEspecial, CalcularControlCif, ObtenerTipoControlCif,
    /// ObtenerDescripcionTipoCif, ObtenerDescripcionNifEspecial, ExtraerParteNumerica.
    /// </summary>
    public class UtilesValidacionTests
    {
        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                        NORMALIZAR                                  ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("\t", "")]
        [InlineData("\r\n", "")]
        public void Normalizar_ValoresVacios_DevuelveCadenaVacia(string? entrada, string esperado)
        {
            UtilesValidacion.Normalizar(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData("12345678Z", "12345678Z")]             // sin cambios
        [InlineData("12345678z", "12345678Z")]             // a mayúsculas
        [InlineData("12.345.678-Z", "12345678Z")]          // puntos y guiones
        [InlineData("12 345 678 Z", "12345678Z")]          // espacios
        [InlineData("  12345678Z  ", "12345678Z")]         // espacios inicio/fin
        [InlineData("12,345,678/Z", "12345678Z")]          // comas y barras
        [InlineData("12\\345\\678Z", "12345678Z")]         // barras invertidas
        [InlineData("x1234567l", "X1234567L")]             // todo en minúsculas
        [InlineData("\t12345678Z\r\n", "12345678Z")]       // tabs y newlines
        public void Normalizar_EliminaCaracteresNoDeseados(string entrada, string esperado)
        {
            UtilesValidacion.Normalizar(entrada).Should().Be(esperado);
        }

        [Fact]
        public void Normalizar_CadenaLarga_NormalizaCorrectamente()
        {
            // Probar con cadena > 64 caracteres (triggerea path no-stackalloc)
            string entrada = new string(' ', 30) + "12345678z" + new string(' ', 30);
            UtilesValidacion.Normalizar(entrada).Should().Be("12345678Z");
        }

        [Fact]
        public void Normalizar_SinCaracteresNoDeseados_SoloMayusculas()
        {
            UtilesValidacion.Normalizar("abcdef").Should().Be("ABCDEF");
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    DETECTAR TIPO DOCUMENTO                         ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData("12345678Z", TipoDocumento.DNI)]
        [InlineData("00000000T", TipoDocumento.DNI)]
        [InlineData("99999999R", TipoDocumento.DNI)]
        public void DetectarTipoDocumento_FormatoDni_DevuelveDni(string entrada, TipoDocumento esperado)
        {
            UtilesValidacion.DetectarTipoDocumento(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData("X1234567L", TipoDocumento.NIE)]
        [InlineData("Y0000000Z", TipoDocumento.NIE)]
        [InlineData("Z0000000M", TipoDocumento.NIE)]
        public void DetectarTipoDocumento_FormatoNie_DevuelveNie(string entrada, TipoDocumento esperado)
        {
            UtilesValidacion.DetectarTipoDocumento(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData("K0000000T", TipoDocumento.NIF)]
        [InlineData("L0000000T", TipoDocumento.NIF)]
        [InlineData("M0000000T", TipoDocumento.NIF)]
        public void DetectarTipoDocumento_FormatoNifEspecial_DevuelveNif(string entrada, TipoDocumento esperado)
        {
            UtilesValidacion.DetectarTipoDocumento(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData("A00000000", TipoDocumento.CIF)]
        [InlineData("B00000000", TipoDocumento.CIF)]
        [InlineData("P0000000J", TipoDocumento.CIF)]
        public void DetectarTipoDocumento_FormatoCif_DevuelveCif(string entrada, TipoDocumento esperado)
        {
            UtilesValidacion.DetectarTipoDocumento(entrada).Should().Be(esperado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("123")]             // muy corto
        [InlineData("1234567890")]      // muy largo
        [InlineData("ABCDEFGHI")]       // no reconocido
        public void DetectarTipoDocumento_FormatoInvalido_DevuelveDesconocido(string? entrada)
        {
            UtilesValidacion.DetectarTipoDocumento(entrada).Should().Be(TipoDocumento.Desconocido);
        }

        [Fact]
        public void DetectarTipoDocumento_DniConLetraEnParteNumerica_DevuelveDesconocido()
        {
            // 1234567AB → tiene letra en posición no-última de la parte numérica
            UtilesValidacion.DetectarTipoDocumento("1234567AB").Should().Be(TipoDocumento.Desconocido);
        }

        [Fact]
        public void DetectarTipoDocumento_NormalizaAntesDeDetectar()
        {
            // Con espacios y minúsculas
            UtilesValidacion.DetectarTipoDocumento("12.345.678-z").Should().Be(TipoDocumento.DNI);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    CALCULAR LETRA DNI                              ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData(0, 'T')]
        [InlineData(1, 'R')]
        [InlineData(2, 'W')]
        [InlineData(12345678, 'Z')]    // 12345678 % 23 = 14 → Z
        [InlineData(99999999, 'R')]    // 99999999 % 23 = 1 → R
        public void CalcularLetraDni_Int_DevuelveLetraCorrecta(int numero, char esperada)
        {
            UtilesValidacion.CalcularLetraDni(numero).Should().Be(esperada);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100000000)]
        public void CalcularLetraDni_Int_FueraDeRango_LanzaExcepcion(int numero)
        {
            var act = () => UtilesValidacion.CalcularLetraDni(numero);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData("12345678", 'Z')]
        [InlineData("00000000", 'T')]
        public void CalcularLetraDni_String_DevuelveLetraCorrecta(string numero, char esperada)
        {
            UtilesValidacion.CalcularLetraDni(numero).Should().Be(esperada);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData("12.345")]
        public void CalcularLetraDni_String_Invalido_LanzaExcepcion(string numero)
        {
            var act = () => UtilesValidacion.CalcularLetraDni(numero);
            act.Should().Throw<ArgumentException>();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    CALCULAR LETRA NIE                              ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public void CalcularLetraNie_X_CalculaCorrecto()
        {
            // X → prefijo 0: 0*10000000 + 1234567 = 1234567 → 1234567 % 23 = 14 → Z... verifiquemos
            char letra = UtilesValidacion.CalcularLetraNie("X1234567");
            // El resultado ya está verificado por Nie.Crear("X1234567L")
            letra.Should().Be('L');
        }

        [Fact]
        public void CalcularLetraNie_Y_CalculaCorrecto()
        {
            char letra = UtilesValidacion.CalcularLetraNie("Y0000000");
            // Y→1, 10000000 % 23 = 10000000 % 23 → lets calc: 23*434782=9999986, 10000000-9999986=14 → Z
            letra.Should().Be('Z');
        }

        [Fact]
        public void CalcularLetraNie_Z_CalculaCorrecto()
        {
            char letra = UtilesValidacion.CalcularLetraNie("Z0000000");
            // Z→2, 20000000 % 23 → 23*869565=19999995, 20000000-19999995=5 → M
            letra.Should().Be('M');
        }

        [Theory]
        [InlineData("A1234567")]     // letra no válida
        [InlineData("B1234567")]
        public void CalcularLetraNie_LetraInvalida_LanzaExcepcion(string nie)
        {
            var act = () => UtilesValidacion.CalcularLetraNie(nie);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CalcularLetraNie_MuyCorto_LanzaExcepcion()
        {
            var act = () => UtilesValidacion.CalcularLetraNie("X123");
            act.Should().Throw<ArgumentException>();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                CALCULAR LETRA NIF ESPECIAL                         ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData("K0000000", 'T')]   // 0 % 23 = 0 → T
        [InlineData("L0000000", 'T')]   // mismo algoritmo, misma letra
        [InlineData("M0000000", 'T')]   // mismo algoritmo, misma letra
        public void CalcularLetraNifEspecial_DevuelveLetraCorrecta(string nif, char esperada)
        {
            UtilesValidacion.CalcularLetraNifEspecial(nif).Should().Be(esperada);
        }

        [Theory]
        [InlineData("A0000000")]     // no es K/L/M
        [InlineData("X0000000")]     // no es K/L/M
        public void CalcularLetraNifEspecial_LetraInvalida_LanzaExcepcion(string nif)
        {
            var act = () => UtilesValidacion.CalcularLetraNifEspecial(nif);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CalcularLetraNifEspecial_MuyCorto_LanzaExcepcion()
        {
            var act = () => UtilesValidacion.CalcularLetraNifEspecial("K12");
            act.Should().Throw<ArgumentException>();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    CALCULAR CONTROL CIF                            ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public void CalcularControlCif_CerosCompletos_DevuelveControlCero()
        {
            var (digito, letra) = UtilesValidacion.CalcularControlCif("A0000000");

            digito.Should().Be('0');
            letra.Should().Be('J');   // JABCDEFGHI[0] = J
        }

        [Fact]
        public void CalcularControlCif_ConDigitosVariados_CalculaCorrecto()
        {
            // B1200000 → ya comprobamos antes: control=6
            var (digito, letra) = UtilesValidacion.CalcularControlCif("B1200000");

            digito.Should().Be('6');
            letra.Should().Be('F');   // JABCDEFGHI[6] = F
        }

        [Fact]
        public void CalcularControlCif_MuyCorto_LanzaExcepcion()
        {
            var act = () => UtilesValidacion.CalcularControlCif("A12");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CalcularControlCif_DigitosNoNumericos_LanzaExcepcion()
        {
            var act = () => UtilesValidacion.CalcularControlCif("A123456A");
            act.Should().Throw<ArgumentException>();
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    OBTENER TIPO CONTROL CIF                        ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData('A', "digito")]
        [InlineData('B', "digito")]
        [InlineData('E', "digito")]
        [InlineData('H', "digito")]
        public void ObtenerTipoControlCif_TipoDigito_DevuelveDigito(char letra, string esperado)
        {
            UtilesValidacion.ObtenerTipoControlCif(letra).Should().Be(esperado);
        }

        [Theory]
        [InlineData('K', "letra")]
        [InlineData('P', "letra")]
        [InlineData('Q', "letra")]
        [InlineData('S', "letra")]
        public void ObtenerTipoControlCif_TipoLetra_DevuelveLetra(char letra, string esperado)
        {
            UtilesValidacion.ObtenerTipoControlCif(letra).Should().Be(esperado);
        }

        [Theory]
        [InlineData('C', "ambos")]
        [InlineData('D', "ambos")]
        [InlineData('F', "ambos")]
        [InlineData('G', "ambos")]
        [InlineData('J', "ambos")]
        [InlineData('N', "ambos")]
        [InlineData('R', "ambos")]
        [InlineData('U', "ambos")]
        [InlineData('V', "ambos")]
        [InlineData('W', "ambos")]
        public void ObtenerTipoControlCif_TipoAmbos_DevuelveAmbos(char letra, string esperado)
        {
            UtilesValidacion.ObtenerTipoControlCif(letra).Should().Be(esperado);
        }

        [Fact]
        public void ObtenerTipoControlCif_Minuscula_FuncionaIgual()
        {
            UtilesValidacion.ObtenerTipoControlCif('a').Should().Be("digito");
            UtilesValidacion.ObtenerTipoControlCif('p').Should().Be("letra");
            UtilesValidacion.ObtenerTipoControlCif('g').Should().Be("ambos");
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                DESCRIPCIONES TIPO CIF                              ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData('A', "Sociedad Anónima")]
        [InlineData('B', "Sociedad de Responsabilidad Limitada")]
        [InlineData('C', "Sociedad Colectiva")]
        [InlineData('G', "Asociación")]
        [InlineData('H', "Comunidad de Propietarios")]
        [InlineData('P', "Corporación Local")]
        [InlineData('W', "Establecimiento permanente")]
        public void ObtenerDescripcionTipoCif_DevuelveDescripcionCorrecta(char letra, string parciaEsperada)
        {
            UtilesValidacion.ObtenerDescripcionTipoCif(letra).Should().Contain(parciaEsperada);
        }

        [Fact]
        public void ObtenerDescripcionTipoCif_LetraInvalida_DevuelveDesconocido()
        {
            UtilesValidacion.ObtenerDescripcionTipoCif('Z').Should().Contain("desconocido");
        }

        [Fact]
        public void ObtenerDescripcionTipoCif_Minuscula_FuncionaIgual()
        {
            UtilesValidacion.ObtenerDescripcionTipoCif('a').Should().Contain("Anónima");
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                DESCRIPCIONES NIF ESPECIAL                          ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData('K', "menor de 14")]
        [InlineData('L', "extranjero")]
        [InlineData('M', "sin NIE")]
        public void ObtenerDescripcionNifEspecial_DevuelveCorrecta(char letra, string parcialEsperada)
        {
            UtilesValidacion.ObtenerDescripcionNifEspecial(letra).Should().Contain(parcialEsperada);
        }

        [Fact]
        public void ObtenerDescripcionNifEspecial_LetraInvalida_DevuelveDesconocido()
        {
            UtilesValidacion.ObtenerDescripcionNifEspecial('A').Should().Contain("desconocido");
        }

        [Fact]
        public void ObtenerDescripcionNifEspecial_Minuscula_FuncionaIgual()
        {
            UtilesValidacion.ObtenerDescripcionNifEspecial('k').Should().Contain("menor de 14");
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    EXTRAER PARTE NUMÈRICA                          ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Theory]
        [InlineData("12345678Z", "12345678")]
        [InlineData("X1234567L", "1234567")]
        [InlineData("A00000000", "00000000")]
        [InlineData("ABCDEFG", "")]
        [InlineData("", "")]
        [InlineData("K1234567T", "1234567")]
        public void ExtraerParteNumerica_DevuelveCorrectamente(string entrada, string esperado)
        {
            UtilesValidacion.ExtraerParteNumerica(entrada).Should().Be(esperado);
        }

        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║                    CONSTANTES                                      ║
        // ╚══════════════════════════════════════════════════════════════════════╝

        [Fact]
        public void Constantes_LetrasDni_Tiene23Caracteres()
        {
            Constantes.LetrasDni.Should().HaveLength(23);
            Constantes.LetrasDni.Should().Be("TRWAGMYFPDXBNJZSQVHLCKE");
        }

        [Fact]
        public void Constantes_ModuloDni_Es23()
        {
            Constantes.ModuloDni.Should().Be(23);
        }

        [Fact]
        public void Constantes_LongitudDocumento_Es9()
        {
            Constantes.LongitudDocumento.Should().Be(9);
        }

        [Fact]
        public void Constantes_LetrasControlCif_Tiene10Caracteres()
        {
            Constantes.LetrasControlCif.Should().HaveLength(10);
            Constantes.LetrasControlCif.Should().Be("JABCDEFGHI");
        }

        [Theory]
        [InlineData('X')]
        [InlineData('Y')]
        [InlineData('Z')]
        public void Constantes_LetrasNie_ContieneXYZ(char letra)
        {
            Constantes.LetrasNie.Contains(letra).Should().BeTrue();
        }

        [Theory]
        [InlineData('A')]
        [InlineData('B')]
        [InlineData('1')]
        public void Constantes_LetrasNie_NoContieneOtros(char letra)
        {
            Constantes.LetrasNie.Contains(letra).Should().BeFalse();
        }

        [Theory]
        [InlineData('K')]
        [InlineData('L')]
        [InlineData('M')]
        public void Constantes_LetrasNifEspecial_ContieneKLM(char letra)
        {
            Constantes.LetrasNifEspecial.Contains(letra).Should().BeTrue();
        }

        [Theory]
        [InlineData('A')]
        [InlineData('B')]
        [InlineData('C')]
        [InlineData('D')]
        [InlineData('E')]
        [InlineData('F')]
        [InlineData('G')]
        [InlineData('H')]
        [InlineData('J')]
        [InlineData('N')]
        [InlineData('P')]
        [InlineData('Q')]
        [InlineData('R')]
        [InlineData('S')]
        [InlineData('U')]
        [InlineData('V')]
        [InlineData('W')]
        public void Constantes_LetrasCif_ContieneTodasLasValidas(char letra)
        {
            Constantes.LetrasCif.Contains(letra).Should().BeTrue();
        }

        [Theory]
        [InlineData('I')]      // excluida
        [InlineData('O')]      // excluida
        [InlineData('X')]      // NIE, no CIF
        [InlineData('K')]      // NIF especial, no CIF
        public void Constantes_LetrasCif_NoContieneExcluidas(char letra)
        {
            Constantes.LetrasCif.Contains(letra).Should().BeFalse();
        }

        [Theory]
        [InlineData('A')]
        [InlineData('B')]
        [InlineData('E')]
        [InlineData('H')]
        public void Constantes_CifControlNumerico_ContieneCorrectos(char letra)
        {
            Constantes.CifControlNumerico.Contains(letra).Should().BeTrue();
        }

        [Theory]
        [InlineData('K')]
        [InlineData('P')]
        [InlineData('Q')]
        [InlineData('S')]
        public void Constantes_CifControlLetra_ContieneCorrectos(char letra)
        {
            Constantes.CifControlLetra.Contains(letra).Should().BeTrue();
        }

        [Theory]
        [InlineData('0')]
        [InlineData('5')]
        [InlineData('9')]
        public void Constantes_Digitos_ContieneDigitos(char c)
        {
            Constantes.Digitos.Contains(c).Should().BeTrue();
        }

        [Theory]
        [InlineData('A')]
        [InlineData(' ')]
        public void Constantes_Digitos_NoContieneNoDigitos(char c)
        {
            Constantes.Digitos.Contains(c).Should().BeFalse();
        }

        [Fact]
        public void Constantes_DescripcionesTipoCif_Tiene17Entradas()
        {
            Constantes.DescripcionesTipoCif.Count.Should().Be(17);
        }

        [Fact]
        public void Constantes_DescripcionesNifEspecial_Tiene3Entradas()
        {
            Constantes.DescripcionesNifEspecial.Count.Should().Be(3);
        }

        [Theory]
        [InlineData(' ')]
        [InlineData('\t')]
        [InlineData('\r')]
        [InlineData('\n')]
        [InlineData('-')]
        [InlineData('.')]
        [InlineData(',')]
        [InlineData('/')]
        [InlineData('\\')]
        public void Constantes_CaracteresNoDeseados_ContieneCorrectos(char c)
        {
            Constantes.CaracteresNoDeseados.Contains(c).Should().BeTrue();
        }
    }
}
