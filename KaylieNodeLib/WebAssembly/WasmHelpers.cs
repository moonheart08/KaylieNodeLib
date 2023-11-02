using System;
using System.Text;
using Elements.Core;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

public static class WasmHelpers
{
    public static bool ValidWasmType(Type T)
    {
        if (T == typeof(int) || T == typeof(long) || T == typeof(float) || T == typeof(double))
            return true;

        return false;
    }
    
    public static bool ValidWasmFuncType(Type T)
    {
        if (ValidWasmType(T))
            return true;

        if (T.IsEnum)
            return true;

        if (T == typeof(string))
            return true;

        return false;
    }
    
    public static bool ValidWasmMemoryType(Type T)
    {
        if (ValidWasmType(T))
            return true;

        if (T.IsEnum)
            return true;

        return false;
    }

    public static bool ValidWasmAbiType(Type T)
    {
        if (ValidWasmMemoryType(T))
            return true;
        if (T.IsCastableTo(typeof(IVector)))
            return true;
        if (T == typeof(colorX))
            return true;
        if (T == typeof(string))
            return true;
        
        return false;
    }
}

public enum StringFormat : int
{
    Unspecified = 0,
    Utf16 = 1,
    Utf32 = 2,
    Utf8 = 3,
    End,
}