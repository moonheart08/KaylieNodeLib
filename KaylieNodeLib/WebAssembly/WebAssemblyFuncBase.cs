using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elements.Core;
using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

[Category("WASM/Funcs")]
public abstract class WebAssemblyFuncBase : WebAssemblyExportBase<FunctionExport>
{
    public readonly SyncDelegate<Delegate> Delegate;

    public const int FUNC_GAS = 100000;

    protected abstract Delegate WasmCallDelegate { get; }
    protected abstract bool IsFunction { get; }
    protected Function? Function { get; private set; }

    protected virtual Type[] Arguments
    {
        get
        {
            var args = GetType().GenericTypeArguments.Select(WasmAbi.AsWasmArgument).ToList();
            return IsFunction ? args.GetRange(0, args.Count-1).ToArray() : args.ToArray();
        }
    }

    protected virtual Type? ReturnType=> IsFunction ? GetType().GenericTypeArguments.GetLast() : null;

    protected override void OnStart()
    {
        base.OnStart();
        
    }

    protected Function? FindFunction()
    {
        if (!TryGetInstance(out var instance) || ExportName.Value is not { } name)
            return null;

        UniLog.Log($"Searching for impl of function with sig [{string.Join(", ", (IEnumerable<Type>) Arguments)} => {ReturnType}.");
        return instance!.GetFunction(name, ReturnType, Arguments);
    }

    protected void SetGas(int amount)
    {
        Module.Target!.SetGas(amount);
    }

    protected override void OnChanges()
    {
        base.OnChanges();
        Function = FindFunction();
        if (Function is not null)
            Delegate.Target = WasmCallDelegate;
        else
            Delegate.Target = null;
    }
}