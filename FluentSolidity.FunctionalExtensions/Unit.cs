namespace FluentSolidity.FunctionalExtensions;

/// <summary>
/// A type with exactly one value, used as a void replacement in functional pipelines.
/// Enables <see cref="Result{T}"/> and <see cref="Either{TLeft,TRight}"/> to represent
/// succeed-or-fail operations that produce no meaningful value.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    /// <summary>
    /// The single value of the Unit type.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// A pre-allocated completed Task returning Unit, to avoid allocations.
    /// </summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "()";
    public int CompareTo(Unit other) => 0;

    public static bool operator ==(Unit left, Unit right) => true;
    public static bool operator !=(Unit left, Unit right) => false;

    public static implicit operator ValueTuple(Unit _) => default;
    public static implicit operator Unit(ValueTuple _) => default;
}
