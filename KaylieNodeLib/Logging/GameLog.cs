using Elements.Core;
using FrooxEngine;

namespace KaylieNodeLib.Logging;

public sealed class GameLog : Component
{
    public readonly SyncRef<LogCollector> Collector = default!;
    
    protected override void OnStart()
    {
        if (Slot.ReferenceID.User != World.LocalUser.AllocationID)
            return;
        UniLog.OnLog += LogHook;
    }

    protected override void OnDestroy()
    {
        if (Slot.ReferenceID.User != World.LocalUser.AllocationID)
            return;
        UniLog.OnLog -= LogHook;
    }

    private void LogHook(string obj)
    {
        Collector.Target?.Log(obj);
    }
}