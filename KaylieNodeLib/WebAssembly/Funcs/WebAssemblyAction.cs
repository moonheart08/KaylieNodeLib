using System;
using FrooxEngine;

namespace KaylieNodeLib.WebAssembly.Funcs;

public sealed class WebAssemblyAction : WebAssemblyFuncBase
{
    protected override Delegate WasmCallDelegate => Call;
    protected override bool IsFunction => false;

    [SyncMethod(typeof(Delegate))]
    private void Call()
    {
        if (Function is not {} func)
            return;
        
        SetGas(FUNC_GAS);
        func.Invoke();
    }
}