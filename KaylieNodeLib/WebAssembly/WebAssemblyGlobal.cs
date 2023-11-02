using FrooxEngine;
using KaylieNodeLib.WebAssembly.Fields;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

public sealed class WebAssemblyGlobal<T> : WebAssemblyExportBase<GlobalExport>
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(T));

    public readonly WasmGlobalField<T> WasmGlobal = default!;

    protected override void OnFieldStateChange()
    {
        if (!TryGetInstance(out var instance))
        {
            WasmGlobal.Global = null;
            return;
        }
        var global = instance?.GetGlobal(ExportName)?.Wrap<T>();
        WasmGlobal.Global = global;
    }
}