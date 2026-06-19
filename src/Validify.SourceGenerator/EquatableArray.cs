using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Validify.SourceGenerator;

/// <summary>
/// A value-equality wrapper over <see cref="ImmutableArray{T}"/> so incremental
/// generator pipeline state caches correctly (ImmutableArray uses reference equality).
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
  where T : IEquatable<T>
{
  public static readonly EquatableArray<T> Empty = new(ImmutableArray<T>.Empty);

  private readonly ImmutableArray<T> _array;

  public EquatableArray(ImmutableArray<T> array) => _array = array;

  public int Count => _array.IsDefault ? 0 : _array.Length;

  public ImmutableArray<T> AsImmutableArray() => _array.IsDefault ? ImmutableArray<T>.Empty : _array;

  public bool Equals(EquatableArray<T> other) => AsImmutableArray().SequenceEqual(other.AsImmutableArray());

  public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

  public override int GetHashCode()
  {
    var hash = 0;
    foreach (var item in AsImmutableArray())
      hash = unchecked((hash * 397) ^ item.GetHashCode());
    return hash;
  }

  public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}