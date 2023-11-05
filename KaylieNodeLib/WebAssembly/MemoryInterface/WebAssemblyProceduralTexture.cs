using System;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly.MemoryInterface;

[Category("WASM/Memory")]
public class WebAssemblyProceduralTexture : ProceduralTextureBase
{
    public readonly Sync<int2> Size;
    [DefaultValue(true)]
    public readonly Sync<bool> Mipmaps;
    [DefaultValue(Elements.Assets.TextureFormat.RGBA32)]
    public readonly Sync<TextureFormat> TextureFormat;
    
    public readonly SyncRef<WebAssemblyMemory> WebAssemblyMemory;
    public readonly Sync<int> TargetAddress;
    
    private Memory? Memory => WebAssemblyMemory.Target?.Memory;
    
    protected override void GenerateErrorIndication() => TextureDecoder.FillErrorTexture(tex2D);

    protected override void ClearTextureData()
    {
        tex2D.Clear(color.Clear);
    }

    protected override void OnCommonUpdate()
    {
        OnChanges(); // HACK: We really should have a smarter way of checking dirty than "just do it every frame 4head"
    }

    protected override void UpdateTextureData(Bitmap2D tex2D)
    {
        try
        {
            if (Memory is null)
                goto err;

            var arrSize = Size.Value.X * Size.Value.Y * TextureFormat.Value.GetBytesPerPixel();

            var span = Memory.GetSpan<byte>(TargetAddress.Value, arrSize);
            if (span.Length < arrSize)
                goto err;

            span.CopyTo(tex2D.RawData);
            return;
        }
        catch (Exception)
        {
            // ignored
        }

        err:
        GenerateErrorIndication();
        return;
    }

    protected override int2 GenerateSize => Size.Value;
    protected override bool GenerateMipmaps => Mipmaps.Value;
    protected override TextureFormat GenerateFormat => TextureFormat.Value;
}