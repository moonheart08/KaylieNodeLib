using System;
using Elements.Core;

namespace KaylieNodeLib.WebAssembly;

public sealed class VectorExtensions<T>
    where T: unmanaged
{
    public static void IntoSpan<TV>(TV v, ref Span<T> s)
        where TV : IVector<T>
    {
        for (var i = 0; i < s.Length; i++)
        {
            s[i] = v[i];
        }
    }
}