using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using JetBrains.Annotations;
using KaylieNodeLib.WebAssembly.Funcs;
using Wasmtime;
using Engine = Wasmtime.Engine;

namespace KaylieNodeLib.WebAssembly;

[Category("WASM")]
[UsedImplicitly]
public partial class WebAssemblyProcess : Component, ICustomInspector
{
    private static readonly ulong MemSize = (ulong) Math.Pow(2, 24);
    
    /// <summary>
    ///     The webassembly module to expose.
    /// </summary>
    public readonly AssetRef<Binary> Module;

    public readonly RawOutput<string> BuildError;

    public Module? BuiltModule;
    public Instance? Instance;
    private Store? _store;
    private Engine? _wasmEngine;
    private Linker? _linker;
    public WasmAbi? Abi;

    public Engine WasmEngine
    {
        get
        {
            if (_wasmEngine is not null)
                return _wasmEngine;

            _wasmEngine = new Engine(
                new Config()
                    .WithFuelConsumption(true)
                    .WithMaximumStackSize(65565)
                    .WithWasmThreads(false)
                    .WithStaticMemoryMaximumSize(MemSize)
                    .WithSIMD(true)
                    .WithDebugInfo(true));

            return _wasmEngine;
        }
    }

    protected override void OnStart()
    {
        Module.OnObjectAvailable += OnModuleAvailable;

        _linker = BuildLinker();
        _stopwatch.Start();
    }

    private void OnModuleAvailable(SyncRef<IAssetProvider<Binary>> reference)
    {
        UniLog.Log("Webassembly module asset changed. Rebuilding!");
        
        StartTask(UpdateModule);
    }

    private async Task UpdateModule()
    {
        if (!(Module.Target is StaticBinary target))
            return;

        await new ToBackground();
        var file = await Engine.AssetManager.GatherAssetFile(target.URL.Value, 100f).ConfigureAwait(false);

        try
        {
            BuiltModule = Wasmtime.Module.FromFile(WasmEngine, file);
            _store = new Store(WasmEngine);
            _store.SetData(this);
            _store.SetLimits((long)MemSize, 1024, null, null, null);

            Instance = _linker!.Instantiate(_store, BuiltModule);
            WasmAbi.TryGetAllocator(Instance, Instance.GetMemory("memory"), out Abi);
        }
        catch (Exception e)
        {
            // WASMTime very politely throws an exception instead of having good error handling.   
            // Throw it at whichever poor soul has to figure it out in-game.
            BuiltModule = null;
            _store = null;
            BuildError.Value = e.ToString();
        }
    }
    public void SetGas(int amount)
    {
        if (_store is null)
            return;
        _store.AddFuel(1);
        var remaining = _store.ConsumeFuel(1); // God this API sucks. Wasmtime pls fix.
        _store.ConsumeFuel(remaining);
        _store.AddFuel((ulong) amount);
    }

    public void BuildInspectorUI(UIBuilder ui)
    {
        ui.PushStyle();
        ui.Style.MinHeight = 300f;
        //LOCALE: This should be localized! Scrunkly.
        const string warning = "<color=red><size=150%>WARNING!</size></color><br>While WASM does indeed have up to 16MiB of memory available to it, this memory is <b>not persistent across save/load</b> and <b>not networked</b>. Do not rely on it in your creations! It is perfectly fine to use as a cache, but be aware the game is free to erase the memory at any time.";
        ui.Text(warning, true, Alignment.TopLeft);
        ui.PopStyle();
        WorkerInspector.BuildInspectorUI(this, ui);
        ui.Button("Find and create functions.", Action);
    }

    [SyncMethod(typeof(Delegate))]
    private void Action(IButton button, ButtonEventData eventdata)
        => FindFunctions();

    private static readonly Type[] WasmFunctionComponents = 
    {
        typeof(WebAssemblyFunc<>),
        typeof(WebAssemblyFunc<,>),
        typeof(WebAssemblyFunc<,,>),
        typeof(WebAssemblyFunc<,,,>),
        typeof(WebAssemblyFunc<,,,,>),
    };

    private static readonly Type[] WasmActionComponents =
    {
        typeof(WebAssemblyAction),
    };
    
    [SyncMethod(typeof(Action))]
    public void FindFunctions()
    {
        if (BuiltModule is null)
            return;

        UniLog.Log("Finding and building webassembly exports!");
        foreach (var export in BuiltModule.Exports)
        {
            UniLog.Log($"Building {export.Name}, type {export.GetType()}");

            switch (export)
            {
                case FunctionExport func:
                {

                    if (func.Results.Count > 1)
                    {
                        UniLog.Log($"Bailing due to too many results! Got {func.Results.Count}.");
                        continue;
                    }

                    var table = func.Results.Count == 0 ? WasmActionComponents : WasmFunctionComponents;

                    if (func.Parameters.Count > table.Length)
                    {
                        UniLog.Log($"Bailing due to too many parameters! Got {func.Parameters.Count} for a {func.Results.Count} result function.");
                        continue;
                    }

                    var t = table[func.Parameters.Count];
                    UniLog.Log($"Using base {t} for {func.Parameters.Count} parameters and {func.Results.Count} results.");

                    if (!func.Parameters.All(KnownKind) || !func.Results.All(KnownKind))
                    {
                        UniLog.Log($"Bailing due to unknown types! Parameters: {string.Join(", ", func.Parameters)}, Results: {string.Join(", ", func.Results)}");
                        continue;
                    }

                    var types = func.Parameters.Select(KindAsType).Concat(func.Results.Select(KindAsType)).ToArray();

                    UniLog.Log($"With arguments {string.Join(", ", types.AsEnumerable())}");
                    Type gt;
                    if (types.Length == 0)
                    {
                        gt = t;
                    }
                    else
                    {
                        gt = t.MakeGenericType(types);
                    }
                    var component = (WebAssemblyFuncBase) Slot.AttachComponent(gt);
                    component.Module.Target = this;
                    component.ExportName.Value = export.Name;
                    break;
                }
                case GlobalExport global:
                {
                    if (!KnownKind(global.Kind))
                    {
                        UniLog.Log($"Bailing due to unknown type! {global.Kind}");
                        continue;
                    }

                    var globaltype = typeof(WebAssemblyGlobal<>).MakeGenericType(KindAsType(global.Kind));
                    var component = (WebAssemblyExportBase<GlobalExport>) Slot.AttachComponent(globaltype);
                    component.Module.Target = this;
                    component.ExportName.Value = global.Name;
                    break;
                }
                case MemoryExport memory:
                {
                    var component = Slot.AttachComponent<WebAssemblyMemory>();
                    component.Module.Target = this;
                    component.ExportName.Value = memory.Name;
                    break;
                }
                default:
                {
                    UniLog.Log($"Refusing to handle {export.GetType()}, NYI.");
                    break;
                }
            }
        }
    }

    private Type KindAsType(ValueKind k)
    {
        return k switch
        {
            ValueKind.Int32 => typeof(int),
            ValueKind.Int64 => typeof(long),
            ValueKind.Float32 => typeof(float),
            ValueKind.Float64 => typeof(double),
            _ => throw new ArgumentOutOfRangeException(nameof(k), k, null)
        };
    }

    private bool KnownKind(ValueKind k)
    {
        return k is ValueKind.Float32 or ValueKind.Float64 or ValueKind.Int32 or ValueKind.Int64;
    }
}