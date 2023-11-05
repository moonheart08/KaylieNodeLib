using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly.MemoryInterface;

[Category("WASM/Memory")]
public abstract class WebAssemblyMemoryInterfaceBase : Component
{
    public readonly RelayRef<WebAssemblyMemory> WebAssemblyMemory;
    public readonly Sync<int> TargetAddress;

    protected WebAssemblyProcess? ModuleComponent => WebAssemblyMemory.Target?.Module.Target;
    protected Memory? Memory => WebAssemblyMemory.Target?.Memory;

    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnChanges()
    {
        base.OnChanges();
        OnTargetChanged();
    }

    protected abstract void OnTargetChanged();
}