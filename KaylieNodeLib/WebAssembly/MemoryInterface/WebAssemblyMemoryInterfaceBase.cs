using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly.MemoryInterface;

[Category("WASM/Memory")]
public abstract class WebAssemblyMemoryInterfaceBase : Component
{
    public readonly SyncRef<WebAssemblyMemory> WebAssemblyMemory;
    public readonly Sync<int> TargetAddress;

    protected WebAssemblyModule? ModuleComponent => WebAssemblyMemory.Target?.Module.Target;
    protected Memory? Memory => WebAssemblyMemory.Target?.Memory;

    protected override void OnStart()
    {
        base.OnStart();
        TargetAddress.OnValueChange += _ => OnTargetChanged();
        WebAssemblyMemory.OnTargetChange += _ => OnTargetChanged();
    }

    protected abstract void OnTargetChanged();
}