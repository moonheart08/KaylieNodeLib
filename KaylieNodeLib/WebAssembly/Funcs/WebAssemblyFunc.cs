using System;
using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly.Funcs;

public class WebAssemblyFunc<TR> : WebAssemblyFuncBase
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(TR));

    protected override Delegate WasmCallDelegate => Call;
    protected override bool IsFunction => true;

    [SyncMethod(typeof(Delegate))]
    private TR? Call()
    {
        if (Function is not {} func)
            return default;
        
        SetGas(FUNC_GAS);
        try
        {
            return (TR?) func.Invoke();
        }
        catch (WasmtimeException)
        {
            return default!;
        }
    }
}

public class WebAssemblyFunc<T1, TR> : WebAssemblyFuncBase
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(TR)) 
                                             && WasmHelpers.ValidWasmAbiType(typeof(T1));
    
    protected override Delegate WasmCallDelegate => Call;
    protected override bool IsFunction => true;

    [SyncMethod(typeof(Delegate))]
    private TR? Call(T1 a)
    {
        if (Function is not {} func)
            return default;
        
        SetGas(FUNC_GAS);

        try
        {
            return (TR?) func.Invoke(
                WasmAbi.BoxArgument(a, Module.Target.Abi)
            );
        } 
        catch (WasmtimeException)
        {
            return default!;
        }
    }
}

public class WebAssemblyFunc<T1, T2, TR> : WebAssemblyFuncBase
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(TR)) 
                                             && WasmHelpers.ValidWasmAbiType(typeof(T1))
                                             && WasmHelpers.ValidWasmAbiType(typeof(T2));
    
    protected override Delegate WasmCallDelegate => Call;
    protected override bool IsFunction => true;

    [SyncMethod(typeof(Delegate))]
    private TR? Call(T1 a, T2 b)
    {
        if (Function is not {} func)
            return default;
        
        SetGas(FUNC_GAS);
        try
        {
            return (TR?) func.Invoke(
                WasmAbi.BoxArgument(a, Module.Target.Abi),
                WasmAbi.BoxArgument(b, Module.Target.Abi)
            );
        }
        catch (WasmtimeException)
        {
            return default!;
        }
    }
}

public class WebAssemblyFunc<T1, T2, T3, TR> : WebAssemblyFuncBase
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(TR)) 
                                             && WasmHelpers.ValidWasmAbiType(typeof(T1))
                                             && WasmHelpers.ValidWasmAbiType(typeof(T2))
                                             && WasmHelpers.ValidWasmAbiType(typeof(T3));
    
    protected override Delegate WasmCallDelegate => Call;
    protected override bool IsFunction => true;

    [SyncMethod(typeof(Delegate))]
    private TR? Call(T1 a, T2 b, T3 c)
    {
        if (Function is not {} func)
            return default;
        
        SetGas(FUNC_GAS);
        try
        {
            return (TR?) func.Invoke(
                WasmAbi.BoxArgument(a, Module.Target.Abi),
                WasmAbi.BoxArgument(b, Module.Target.Abi),
                WasmAbi.BoxArgument(c, Module.Target.Abi)
            );
        }
        catch (WasmtimeException)
        {
            return default!;
        }
    }
}
public class WebAssemblyFunc<T1, T2, T3, T4, TR> : WebAssemblyFuncBase
{
    public static bool IsValidGenericType => WasmHelpers.ValidWasmType(typeof(TR)) 
                                             && WasmHelpers.ValidWasmAbiType(typeof(T1))
                                             && WasmHelpers.ValidWasmAbiType(typeof(T2))
                                             && WasmHelpers.ValidWasmAbiType(typeof(T3))
                                             && WasmHelpers.ValidWasmAbiType(typeof(T4));
    
    protected override Delegate WasmCallDelegate => Call;
    protected override bool IsFunction => true;

    [SyncMethod(typeof(Delegate))]
    private TR? Call(T1 a, T2 b, T3 c, T4 d)
    {
        if (Function is not {} func)
            return default;
        
        SetGas(FUNC_GAS);
        try
        {
            return (TR?) func.Invoke(
                WasmAbi.BoxArgument(a, Module.Target.Abi),
                WasmAbi.BoxArgument(b, Module.Target.Abi),
                WasmAbi.BoxArgument(c, Module.Target.Abi),
                WasmAbi.BoxArgument(d, Module.Target.Abi)
            );
        }
        catch (WasmtimeException)
        {
            return default!;
        }
    }
}