using System;
using FrooxEngine;
using JetBrains.Annotations;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly.Fields;

[UsedImplicitly]
public sealed class WasmMemoryField<T> : EmptySyncElement, IField<T>
    where T: unmanaged
{
    /// <summary>
    /// Target memory address for this field.
    /// </summary>
    private int _address;
    /// <summary>
    /// The memory being read from.
    /// </summary>
    private Memory? _memory;
    
    public static bool IsValidGenericType => WasmHelpers.ValidWasmMemoryType(typeof(T));
    
    public object BoxedValue
    {
        get => Value;
        set => Value = (T)value;
    }

    Type IField.ValueType => typeof(T);
    Type IValue.ValueType => typeof(T);
    
    public unsafe T Value
    {
        get
        {
            if (Memory is null)
                return default;
            
            if (Memory.GetLength() <= sizeof(T) + Address)
                return default;

            return Memory.Read<T>(Address);
        }

        set
        {
            if (Memory is null)
                return;
            
            if (Memory.GetLength() <= sizeof(T) + Address)
                return;
            
            Memory.Write(Address, value);
        }
    }

    public int Address
    {
        get => _address;
        set
        {
            SyncElementChanged();
            _address = value;
        }
    }

    public Memory? Memory
    {
        get => _memory;
        set
        {
            SyncElementChanged();
            _memory = value;
        }
    }
}