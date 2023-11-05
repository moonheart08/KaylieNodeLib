using System;
using System.Diagnostics;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

public partial class WebAssemblyProcess
{
    private readonly Stopwatch _stopwatch = new();

    private Linker BuildLinker()
    {
        var linker = new Linker(WasmEngine);
        linker.DefineFunction("env", "abort", (CallerAction<int>)Wasm_Env_Abort);
        linker.DefineFunction("env", "curtime", Wasm_Env_CurTime);
        linker.DefineFunction("env", "fuel_left", Wasm_Env_FuelLeft);
        return linker;
    }
    
    private static void Wasm_Env_Abort(Caller caller, int a)
    {
        throw new Exception("WASM environment aborted execution.");
    }

    private static long Wasm_Env_CurTime(Caller caller)
    {
        var self = (WebAssemblyProcess) caller.GetData()!;
        return self._stopwatch.ElapsedTicks;
    }

    private static long Wasm_Env_FuelLeft(Caller caller)
    {
        return (long)caller.ConsumeFuel(1);
    }
}