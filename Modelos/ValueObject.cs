namespace nif_dni_nie_cif_validation.Modelos
{
    /// <summary>
    /// Clase base abstracta para objetos de valor (Value Objects) de documentos de identificación.
    /// Los objetos de valor son inmutables y se comparan por su contenido, no por referencia.
    /// Solo se pueden crear instancias con valores válidos.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Valor normalizado del documento (sin espacios, guiones, en mayúsculas).
        /// </summary>
        public string Valor { get; }

        /// <summary>
        /// Tipo de documento que representa este objeto de valor.
        /// </summary>
        public abstract TipoDocumento Tipo { get; }

        /// <summary>
        /// Parte numérica del documento.
        /// </summary>
        public abstract string ParteNumerica { get; }

        /// <summary>
        /// Carácter de control (letra o dígito) del documento.
        /// </summary>
        public abstract string CaracterControl { get; }

        /// <summary>
        /// Constructor protegido. Solo las clases derivadas pueden crear instancias.
        /// </summary>
        /// <param name="valor">Valor normalizado del documento.</param>
        protected ValueObject(string valor)
        {
            Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        }

        /// <inheritdoc />
        public bool Equals(ValueObject? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Valor == other.Valor && Tipo == other.Tipo;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as ValueObject);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Valor, Tipo);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Valor;
        }

        /// <summary>
        /// Conversión implícita a string.
        /// </summary>
        /// <param name="valueObject">Objeto de valor a convertir.</param>
        public static implicit operator string(ValueObject valueObject)
        {
            return valueObject.Valor;
        }

        /// <summary>
        /// Operador de igualdad.
        /// </summary>
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }
}
