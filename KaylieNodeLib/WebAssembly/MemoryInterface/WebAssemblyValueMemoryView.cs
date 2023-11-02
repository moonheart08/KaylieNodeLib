using FrooxEngine;
using KaylieNodeLib.WebAssembly.Fields;

namespace KaylieNodeLib.WebAssembly.MemoryInterface;

public sealed class WebAssemblyValueMemoryView<T> : WebAssemblyMemoryInterfaceBase
    where T: unmanaged
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmMemoryType(typeof(T));
    public readonly WasmMemoryField<T> Field;
    
    protected override void OnTargetChanged()
    {
        Field.Memory = Memory;
        Field.Address = TargetAddress.Value;
    }
}