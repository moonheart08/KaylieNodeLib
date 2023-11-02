using System;
using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

[Category("WASM")]
public sealed class WebAssemblyWorker : WebAssemblyFuncBase
{
    protected override Delegate WasmCallDelegate => null!; // Don't expose an update delegate.
    protected override bool IsFunction => false;
    public const int WorkerGas = 10000000;

    protected override void OnCommonUpdate()
    {
        base.OnCommonUpdate();

        if (Function is not {} func)
            return;

        SetGas(WorkerGas);
        try
        {
            func.Invoke();
        }
        catch (WasmtimeException)
        {
            Enabled = false;
        }
    }
}