using System.Collections.Generic;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;

namespace KaylieNodeLib.Logging;

public sealed class LogCollector : Component
{
    public readonly SyncRef<Slot> OutputContainer = default!;
    public readonly Sync<bool> Paused = default!;
    public readonly Sync<float> TextSize = default!;
    public readonly AssetRef<FontSet> Font = default!;
    private Queue<string> LogRoll = new();
    private const int LOG_MAX = 2048;

    protected override void OnCommonUpdate()
    {
        if (Paused)
            return;
        while (LogRoll.Count > 0)
        {
            var obj = LogRoll.Dequeue();
            if (OutputContainer.Target.ChildrenCount > LOG_MAX)
            {
                OutputContainer.Target[0].Destroy();
            }
            
            var ui = new UIBuilder(OutputContainer.Target);
            RadiantUI_Constants.SetupDefaultStyle(ui);
            var txt = ui.Text(obj, bestFit: false, parseRTF: false);
            txt.Align = Alignment.TopLeft;
            txt.Size.Value = TextSize;
            txt.Font.Value = Font;
        }
    }

    public void Log(string obj)
    {
        if (OutputContainer.Target == null)
            return;
        
        LogRoll.Enqueue(obj);
    }
}