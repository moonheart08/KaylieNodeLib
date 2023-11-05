using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

public class WebAssemblyMemory : WebAssemblyExportBase<MemoryExport>
{
    public readonly RawOutput<long> Length;
    public readonly RawOutput<long> MaximumSize;
    public readonly RawOutput<long> MinimumSize;
    internal Memory? Memory;
    
    protected override void OnFieldStateChange()
    {
        if (!TryGetInstance(out var instance))
        {
            Memory = null;
            return;
        }
        
        Memory ??= instance?.GetMemory(ExportName);
        
        Length.Value = Memory?.GetLength() ?? 0;
        MinimumSize.Value = Memory?.Minimum * Memory.PageSize ?? 0;
        MaximumSize.Value = Memory?.Maximum * Memory.PageSize ?? 0;
    }
}