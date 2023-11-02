using System;
using System.Runtime.InteropServices;
using System.Text;
using Elements.Core;
using ProtoFlux.Runtimes.Execution.Nodes;
using Wasmtime;

namespace KaylieNodeLib.WebAssembly;

public record struct WasmPtr(int Value)
{
    public static implicit operator int(WasmPtr p) => p.Value;
    public static implicit operator long(WasmPtr p) => p.Value;
    public static implicit operator ValueBox(WasmPtr p) => p.Value;
    public static implicit operator ValueBox(WasmPtr? p) => p?.Value ?? -1;

    public static WasmPtr operator +(in WasmPtr left, in int right)
    {
        return new WasmPtr(left.Value + right);
    }
}

// Can be made abstract in the future to support versioning.
public sealed class WasmAbi
{
    private Func<int, int> _alloc;
    private Action<int> _dealloc;
    private Memory _memory;
    private StringFormat _stringFormat;
    
    public static bool TryGetAllocator(in Instance? instance, in Memory? memory, out WasmAbi? allocator)
    {
        if (instance is null || memory is null)
        {
            allocator = null;
            return false;
        }
        
        var alloc = instance.GetFunction<int, int>("__resonite_v1_alloc");
        var dealloc = instance.GetAction<int>("__resonite_v1_dealloc");
        var form = instance.GetGlobal("__resonite_v1_str_format")?.Wrap<int>();

        if (alloc is null || form is null || dealloc is null)
        {
            allocator = null;
            return false;
        }

        var format = (StringFormat) form.GetValue();
        if (format is >= StringFormat.End or <= StringFormat.Unspecified)
        {
            allocator = null;
            return false;
        }

        allocator = new(alloc, dealloc, format, memory);
        return true;
    }

    private WasmAbi(Func<int, int> alloc, Action<int> dealloc, StringFormat format, Memory memory)
    {
        _alloc = alloc;
        _dealloc = dealloc;
        _stringFormat = format;
        _memory = memory;
    }

    public bool TryAllocBlock(int length, out WasmPtr? ptr)
    {
        var res = _alloc(length);
        
        if (res <= 0)
        {
            ptr = null;
            return false;
        }

        ptr = new WasmPtr(res);
        return true;
    }

    public bool TryAllocBlock<T>(in Span<T> blk, out WasmPtr? ptr)
        where T: unmanaged
    {
        if (!TryAllocBlock(blk.Length, out ptr))
            return false;
        try
        {
            var span = _memory.GetSpan<T>(ptr!.Value, blk.Length);
            blk.CopyTo(span);
        }
        catch (Exception)
        {
            _dealloc(ptr!.Value);
            return false; // WasmTime moment.
        }

        return true;
    }

    public bool TryAllocString(in string str, out WasmPtr? ptr)
    {
        byte[] bytes;
        switch (_stringFormat)
        {
            case StringFormat.Utf8:
            {
                bytes = Encoding.UTF8.GetBytes(str);
                break;
            }
            case StringFormat.Utf16:
            {
                bytes = Encoding.Unicode.GetBytes(str);
                break;
            }
            case StringFormat.Utf32:
            {
                bytes = Encoding.UTF32.GetBytes(str);
                break;
            }
            case StringFormat.Unspecified:
            case StringFormat.End:
            default:
            {
                throw new NotImplementedException($"Unimplemented format {_stringFormat}.");
            }
        }

        if (!TryAllocBlock(bytes.Length + 4, out ptr))
            return false;

        try
        {
            _memory.WriteInt32(ptr!.Value, bytes.Length);
            var span = _memory.GetSpan<byte>(ptr.Value + 4, bytes.Length);
            bytes.CopyTo(span);
            return true;
        }
        catch (Exception)
        {
            _dealloc(ptr!.Value);
            return false; // WasmTime moment.
        }
    }

    public bool TryWriteSpan<T>(in Span<T> data, in WasmPtr ptr)
        where T : unmanaged
    {
        try
        {
            var span = _memory.GetSpan<T>(ptr.Value, data.Length);
            data.CopyTo(span);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TryAlloc<T>(in T value, out WasmPtr? ptr)
    {
        switch (value)
        {
            case IVector v:
            {
                var size = Marshal.SizeOf(v.ElementType);
                if (!TryAllocBlock(size * v.Dimensions, out ptr))
                    return false;
                return TryWrite(value, ptr!.Value);
            }
            case colorX color:
            {
                const int size = sizeof(float) * 4 + sizeof(int);
                if (!TryAllocBlock(size, out ptr))
                    return false;
                return TryWrite(color, ptr!.Value);
            }
            default:
            {
                throw new NotSupportedException();
            }
        }
    }

    public bool TryWrite<T>(in T value, in WasmPtr ptr)
    {
        switch (value)
        {
            case IVector<bool> f:
            {
                Span<bool> data = stackalloc bool[f.Dimensions];
                VectorExtensions<bool>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case IVector<ulong> f:
            {
                Span<ulong> data = stackalloc ulong[f.Dimensions];
                VectorExtensions<ulong>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case IVector<long> f:
            {
                Span<long> data = stackalloc long[f.Dimensions];
                VectorExtensions<long>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case IVector<uint> f:
            {
                Span<uint> data = stackalloc uint[f.Dimensions];
                VectorExtensions<uint>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case IVector<int> f:
            {
                Span<int> data = stackalloc int[f.Dimensions];
                VectorExtensions<int>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case IVector<float> f:
            {
                Span<float> data = stackalloc float[f.Dimensions];
                VectorExtensions<float>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case IVector<double> f:
            {
                Span<double> data = stackalloc double[f.Dimensions];
                VectorExtensions<double>.IntoSpan(f, ref data);
                return TryWriteSpan(data, ptr);
            }
            case colorX color:
            {
                Span<float> data = stackalloc float[4];
                VectorExtensions<float>.IntoSpan((float4)color, ref data);
                try
                {
                    _memory.WriteInt32(ptr + (sizeof(float) * 4), (int) color.profile);
                    return TryWriteSpan(data, ptr);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            default:
            {
                throw new NotSupportedException();
            }
        }
    }
    
    public static ValueBox BoxArgument<T>(T arg, WasmAbi? abi)
    {
        switch (arg)
        {
            case short s:
                return s;
            case ushort s:
                return s;
            case sbyte b:
                return b;
            case byte b:
                return b;
            case int i:
                return i;
            case uint i:
                return i;
            case long l:
                return l;
            case ulong l:
                return l;
            case float f:
                return f;
            case double d:
                return d;
            case char c:
                return c;
            case string str:
            {
                if (abi is null)
                    return (int) AllocatorErrorCode.AllocFailed;
                if (!abi.TryAllocString(str, out var blk))
                {
                    return (int) AllocatorErrorCode.AllocFailed;
                }

                return blk;
            }
            case IVector:
            {
                if (abi is null)
                    return (int) AllocatorErrorCode.AllocFailed;
                if (!abi.TryAlloc(arg, out var ptr))
                    return (int) AllocatorErrorCode.AllocFailed;

                return ptr!.Value;
            }
            default:
            {
                if (!typeof(T).IsEnum || !WasmHelpers.ValidWasmType(Enum.GetUnderlyingType(typeof(T))))
                    throw new NotImplementedException();
                
                var eTy = Enum.GetUnderlyingType(typeof(T));
                // i hate this --kaylie
                if (eTy == typeof(int))
                {
                    return (int) (object) arg!;
                }
                else if (eTy == typeof(long))
                {
                    return (long) (object) arg!;
                }
                else if (eTy == typeof(sbyte))
                {
                    return (sbyte) (object) arg!;
                }
                else if (eTy == typeof(short))
                {
                    return (short) (object) arg!;
                }
                else if (eTy == typeof(uint))
                {
                    return (uint) (object) arg!;
                }
                else if (eTy == typeof(ulong))
                {
                    return (ulong) (object) arg!;
                }
                else if (eTy == typeof(byte))
                {
                    return (byte) (object) arg!;
                }
                else if (eTy == typeof(ushort))
                {
                    return (ushort) (object) arg!;
                }
                else
                {
                    throw new NotImplementedException(
                        $"Couldn't convert enum with base type {eTy}, type was {typeof(T)}");
                }
            }
        }
    }

    public static Type AsWasmArgument(Type T)
    {
        if (T.IsEnum)
            return Enum.GetUnderlyingType(T);
        
        if (T.IsPrimitive)
            return T;

        return typeof(int);
    }

    private enum AllocatorErrorCode
    {
        AllocFailed = -1,
    }
}