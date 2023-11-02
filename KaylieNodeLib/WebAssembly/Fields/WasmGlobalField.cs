using System;
using System.IO;
using Elements.Core;
using FrooxEngine;
using JetBrains.Annotations;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly.Fields;

[UsedImplicitly]
public sealed class WasmGlobalField<T> : EmptySyncElement, IField<T>
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(T));
    
    public object BoxedValue
    {
        get => Value!;
        set => Value = (T)value;
    }

    Type IField.ValueType => typeof(T);
    Type IValue.ValueType => typeof(T);

    public T Value
    {
        get
        {
            if (Global is null)
                return default!;
            
            return Global.GetValue();
        }
        set
        {
            if (Global is null || Global.Mutability == Mutability.Immutable)
                return;
            
            Global.SetValue(value);
        }
    }

    public Global.Accessor<T>? Global { get; set; }
}