using System;
using System.Linq;
using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

[Category("WASM")]
public abstract class WebAssemblyExportBase<T> : Component
    where T: Export
{
    public readonly RelayRef<WebAssemblyModule> Module;
    public readonly Sync<string> ExportName;
    public readonly Sync<bool> Found;
    private WebAssemblyModule? _lastModule;

    protected override void OnStart()
    {
        base.OnStart();
        ExportName.OnValueChange += ExportNameOnOnValueChange;
    }

    internal void ExportNameOnOnValueChange(SyncField<string> _)
    {
        if (!TryGetInstance(out var _))
        {
            Found.Value = false;
            OnFieldStateChange();
            return;
        }

        Found.Value = Module.Target.BuiltModule!.Exports.Any(x => x is T && x.Name == ExportName.Value);
        OnFieldStateChange();
    }

    protected virtual void OnFieldStateChange()
    {
    }

    protected bool TryGetInstance(out Instance? instance)
    {
        instance = null;
        if (Module.Target is not {} moduleComponent)
            return false;

        if (moduleComponent.Instance is null)
            return false;

        instance = moduleComponent.Instance;
        return true;
    }
}