using FluentAssertions;
using NIF.DNI.NIE.CIF.Validation.Excepciones;
using NIF.DNI.NIE.CIF.Validation.Modelos;
using NIF.DNI.NIE.CIF.Validation.Utiles;

namespace NIF.DNI.NIE.CIF.Validation.Tests.Modelos
{
    /// <summary>
    /// Tests completos para el Value Object Cif.
    /// Cubre: tipos de entidad, control numérico, control letra, control ambos, errores de formato.
    /// </summary>
    public class CifTests
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Crear — CIF con control numérico (A, B, E, H)
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_CifTipoA_SociedadAnonima()
        {
            // A58818501 — Sociedad Anónima real/conocida
            // Calculemos: A + 5881850 + control
            // Para validar necesitamos un CIF real o calcular uno
            // A00000000 → control para 0000000
            // Pares: pos 2,4,6 = 0+0+0 = 0
            // Impares: pos 1,3,5,7 → cada uno: 0*2=0, 0+0=0 → sumaImpares=0
            // control = (10 - (0+0)%10) % 10 = 0
            // A00000000 → dígito control = '0'
            var cif = Cif.Crear("A00000000");

            cif.LetraTipoEntidad.Should().Be('A');
            cif.TipoEntidad.Should().Contain("Anónima");
            cif.Tipo.Should().Be(TipoDocumento.CIF);
            cif.CodigoProvincia.Should().Be("00");
        }

        [Fact]
        public void Crear_CifTipoB_SociedadLimitada()
        {
            var cif = Cif.Crear("B00000000");
            cif.TipoEntidad.Should().Contain("Responsabilidad Limitada");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Crear — CIF con control letra (K, P, Q, S)
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_CifTipoP_CorporacionLocal()
        {
            // P → control letra. control=0 → letra='J'
            var cif = Cif.Crear("P0000000J");

            cif.LetraTipoEntidad.Should().Be('P');
            cif.TipoEntidad.Should().Contain("Corporación Local");
        }

        [Fact]
        public void Crear_CifTipoQ_OrganismoPublico()
        {
            var cif = Cif.Crear("Q0000000J");
            cif.TipoEntidad.Should().Contain("Organismo Público");
        }

        [Fact]
        public void Crear_CifTipoS_Administracion()
        {
            var cif = Cif.Crear("S0000000J");
            cif.TipoEntidad.Should().Contain("Administración");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Crear — CIF con control ambos (C, D, F, G, ...)
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_CifTipoG_Asociacion_ControlDigito()
        {
            // G → ambos. control=0 → '0' (dígito) o 'J' (letra)
            var cif = Cif.Crear("G00000000");
            cif.LetraTipoEntidad.Should().Be('G');
            cif.TipoEntidad.Should().Contain("Asociación");
        }

        [Fact]
        public void Crear_CifTipoG_Asociacion_ControlLetra()
        {
            // G → ambos, acepta letra también
            var cif = Cif.Crear("G0000000J");
            cif.LetraTipoEntidad.Should().Be('G');
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Normalización
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("b00000000", "B00000000")]
        [InlineData("B-00000000", "B00000000")]
        [InlineData("  B00000000  ", "B00000000")]
        public void Crear_CifConFormatoSucio_NormalizaCorrectamente(string entrada, string esperado)
        {
            var cif = Cif.Crear(entrada);
            cif.Valor.Should().Be(esperado);
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
            var act = () => Cif.Crear(entrada!);
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.CodigoError.Should().Be("CIF_NO_VALIDO");
        }

        [Theory]
        [InlineData("B0000000")]       // 8 chars
        [InlineData("B000000000")]     // 10 chars
        public void Crear_LongitudIncorrecta_LanzaExcepcion(string entrada)
        {
            var act = () => Cif.Crear(entrada);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Theory]
        [InlineData("I00000000")]      // I no es letra CIF válida
        [InlineData("O00000000")]      // O no es letra CIF válida
        [InlineData("100000000")]      // empieza por dígito
        public void Crear_LetraInicialInvalida_LanzaExcepcion(string entrada)
        {
            var act = () => Cif.Crear(entrada);
            act.Should().Throw<DocumentoNoValidoException>();
        }

        [Fact]
        public void Crear_ParteNumericaConLetras_LanzaExcepcion()
        {
            var act = () => Cif.Crear("B00000A00");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("dígitos");
        }

        [Fact]
        public void Crear_ControlIncorrecto_LanzaExcepcion()
        {
            // A00000000 es válido (control '0'), A00000001 debería ser inválido
            var act = () => Cif.Crear("A00000001");
            act.Should().Throw<DocumentoNoValidoException>()
               .Which.Message.Should().Contain("control");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Intentar
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Intentar_CifValido_DevuelveTrue()
        {
            bool resultado = Cif.Intentar("B00000000", out var cif);

            resultado.Should().BeTrue();
            cif.Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("INVALIDO")]
        [InlineData("B00000001")]
        public void Intentar_CifInvalido_DevuelveFalse(string? entrada)
        {
            bool resultado = Cif.Intentar(entrada!, out var cif);

            resultado.Should().BeFalse();
            cif.Should().BeNull();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Propiedades específicas del CIF
        // ═══════════════════════════════════════════════════════════════════

        [Fact]
        public void Crear_CifValido_CodigoProvinciaExtraido()
        {
            // B12 = código provincia "12"
            // Necesitamos un CIF con provincia 12 que sea válido
            // B1200000 → calcular control
            // Impares (i=1,3,5,7): d1=1*2=2, d3=0*2=0, d5=0*2=0, d7=0*2=0 → sumaImpares=2
            // Pares (i=2,4,6): d2=2, d4=0, d6=0 → sumaPares=2  
            // total=4, control=(10-4%10)%10 = 6
            var cif = Cif.Crear("B12000006");
            cif.CodigoProvincia.Should().Be("12");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Todos los tipos de entidad CIF
        // ═══════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData('A', "Sociedad Anónima")]
        [InlineData('B', "Sociedad de Responsabilidad Limitada")]
        [InlineData('C', "Sociedad Colectiva")]
        [InlineData('D', "Sociedad Comanditaria")]
        [InlineData('E', "Comunidad de Bienes")]
        [InlineData('F', "Sociedad Cooperativa")]
        [InlineData('G', "Asociación")]
        [InlineData('H', "Comunidad de Propietarios")]
        [InlineData('J', "Sociedad Civil")]
        [InlineData('N', "Entidad Extranjera")]
        [InlineData('R', "Congregación Religiosa")]
        [InlineData('U', "Unión Temporal de Empresas")]
        [InlineData('V', "Otro tipo no definido")]
        [InlineData('W', "Establecimiento permanente")]
        public void Crear_CadaTipoEntidad_DescripcionCorrecta(char letra, string descripcionParcial)
        {
            // Todos con 0000000 y control calculado para cada letra
            string parteNum = "0000000";
            var (digito, letraControl) = UtilesValidacion.CalcularControlCif($"{letra}{parteNum}0");

            // Determinar qué control usar
            string tipoControl = UtilesValidacion.ObtenerTipoControlCif(letra);
            char control = tipoControl == "letra" ? letraControl : digito;

            string cifCompleto = $"{letra}{parteNum}{control}";
            var cif = Cif.Crear(cifCompleto);

            cif.TipoEntidad.Should().Contain(descripcionParcial);
        }
    }
}
