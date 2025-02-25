﻿using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using NewLife.Collections;
using NewLife.Data;

namespace NewLife.Buffers;

/// <summary>Span写入器</summary>
/// <param name="buffer"></param>
public ref struct SpanWriter(Span<Byte> buffer)
{
    #region 属性
    private readonly Span<Byte> _span = buffer;
    /// <summary>数据片段</summary>
    public Span<Byte> Span => _span;

    private Int32 _index;
    /// <summary>已写入字节数</summary>
    public Int32 Position { get => _index; set => _index = value; }

    /// <summary>总容量</summary>
    public readonly Int32 Capacity => _span.Length;

    /// <summary>空闲容量</summary>
    public readonly Int32 FreeCapacity => _span.Length - _index;

    /// <summary>是否小端字节序。默认true</summary>
    public Boolean IsLittleEndian { get; set; } = true;
    #endregion

    #region 构造
    /// <summary>实例化Span读取器</summary>
    /// <param name="data"></param>
    public SpanWriter(IPacket data) : this(data.GetSpan()) { }
    #endregion

    #region 基础方法
    /// <summary>告知有多少数据已写入缓冲区</summary>
    /// <param name="count"></param>
    public void Advance(Int32 count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (_index + count > _span.Length) throw new ArgumentOutOfRangeException(nameof(count));

        _index += count;
    }

    /// <summary>返回要写入到的Span，其大小按 sizeHint 参数指定至少为所请求的大小</summary>
    /// <param name="sizeHint"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public readonly Span<Byte> GetSpan(Int32 sizeHint = 0)
    {
        if (sizeHint > FreeCapacity) throw new ArgumentOutOfRangeException(nameof(sizeHint));

        return _span[_index..];
    }
    #endregion

    #region 写入方法
    /// <summary>确保缓冲区中有足够的空间。</summary>
    /// <param name="size">需要的字节数。</param>
    private readonly void EnsureSpace(Int32 size)
    {
        if (_index + size > _span.Length)
            throw new InvalidOperationException("Not enough data to write.");
    }

    /// <summary>写入字节</summary>
    public Int32 WriteByte(Int32 value) => Write((Byte)value);

    /// <summary>写入字节</summary>
    /// <param name="value">要写入的字节值。</param>
    public Int32 Write(Byte value)
    {
        var size = sizeof(Byte);
        EnsureSpace(size);
        _span[_index] = value;
        _index += size;
        return size;
    }

    /// <summary>写入 16 位整数。</summary>
    /// <param name="value">要写入的整数值。</param>
    public Int32 Write(Int16 value)
    {
        var size = sizeof(Int16);
        EnsureSpace(size);
        if (IsLittleEndian)
            BinaryPrimitives.WriteInt16LittleEndian(_span[_index..], value);
        else
            BinaryPrimitives.WriteInt16BigEndian(_span[_index..], value);
        _index += size;
        return size;
    }

    /// <summary>写入无符号 16 位整数。</summary>
    /// <param name="value">要写入的无符号整数值。</param>
    public Int32 Write(UInt16 value)
    {
        var size = sizeof(UInt16);
        EnsureSpace(size);
        if (IsLittleEndian)
            BinaryPrimitives.WriteUInt16LittleEndian(_span[_index..], value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(_span[_index..], value);
        _index += size;
        return size;
    }

    /// <summary>写入 32 位整数。</summary>
    /// <param name="value">要写入的整数值。</param>
    public Int32 Write(Int32 value)
    {
        var size = sizeof(Int32);
        EnsureSpace(size);
        if (IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(_span[_index..], value);
        else
            BinaryPrimitives.WriteInt32BigEndian(_span[_index..], value);
        _index += size;
        return size;
    }

    /// <summary>写入无符号 32 位整数。</summary>
    /// <param name="value">要写入的无符号整数值。</param>
    public Int32 Write(UInt32 value)
    {
        var size = sizeof(UInt32);
        EnsureSpace(size);
        if (IsLittleEndian)
            BinaryPrimitives.WriteUInt32LittleEndian(_span[_index..], value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(_span[_index..], value);
        _index += size;
        return size;
    }

    /// <summary>写入 64 位整数。</summary>
    /// <param name="value">要写入的整数值。</param>
    public Int32 Write(Int64 value)
    {
        var size = sizeof(Int64);
        EnsureSpace(size);
        if (IsLittleEndian)
            BinaryPrimitives.WriteInt64LittleEndian(_span[_index..], value);
        else
            BinaryPrimitives.WriteInt64BigEndian(_span[_index..], value);
        _index += size;
        return size;
    }

    /// <summary>写入无符号 64 位整数。</summary>
    /// <param name="value">要写入的无符号整数值。</param>
    public Int32 Write(UInt64 value)
    {
        var size = sizeof(UInt64);
        EnsureSpace(size);
        if (IsLittleEndian)
            BinaryPrimitives.WriteUInt64LittleEndian(_span[_index..], value);
        else
            BinaryPrimitives.WriteUInt64BigEndian(_span[_index..], value);
        _index += size;
        return size;
    }

    /// <summary>写入单精度浮点数。</summary>
    /// <param name="value">要写入的浮点值。</param>
    public unsafe Int32 Write(Single value)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return Write(BitConverter.SingleToInt32Bits(value));
#else
        return Write(*(Int32*)&value);
#endif
    }

    /// <summary>写入双精度浮点数。</summary>
    /// <param name="value">要写入的浮点值。</param>
    public unsafe Int32 Write(Double value)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return Write(BitConverter.DoubleToInt64Bits(value));
#else
        return Write(*(Int64*)&value);
#endif
    }

    /// <summary>写入字符串。支持定长、全部和长度前缀</summary>
    /// <param name="value">要写入的字符串</param>
    /// <param name="length">最大长度。字节数，-1表示写入全部，默认0表示写入7位压缩编码整数长度。不足时填充字节0，超长时截取</param>
    /// <param name="encoding">字符串编码，默认UTF8</param>
    /// <returns>返回写入字节数，包括头部长度和字符串部分</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Int32 Write(String value, Int32 length = 0, Encoding? encoding = null)
    {
        var p = _index;
        encoding ??= Encoding.UTF8;
        if (length < 0)
        {
            // 写入字符串全部内容
            var count = encoding.GetBytes(value.AsSpan(), _span[_index..]);
            _index += count;

            return _index - p;
        }
        else if (length == 0)
        {
            // 先写入长度，再写入内容
            if (value.IsNullOrEmpty())
            {
                WriteEncodedInt(0);
                return _index - p;
            }

            length = encoding.GetByteCount(value);
            WriteEncodedInt(length);
            EnsureSpace(length);

            var count = encoding.GetBytes(value.AsSpan(), _span[_index..]);
            _index += count;

            return _index - p;
        }
        else
        {
            // 写入指定长度，不足是填充字节0，超长时截取
            var span = GetSpan(length);
            if (span.Length > length) span = span[..length];

            // 输出缓冲区不能过小，否则报错。大小足够时，直接把字符串写入到目标
            var source = value.AsSpan();
            var max = encoding.GetMaxByteCount(source.Length);
            if (max <= length)
                encoding.GetBytes(source, span);
            else
            {
                // 目标大小可能不足，申请临时缓冲区，输出后做局部拷贝
                var buf = Pool.Shared.Rent(max);
                var count = encoding.GetBytes(source, buf);

                // 局部拷贝，仅拷贝需要部分，抛弃超长部分
                new Span<Byte>(buf, 0, length).CopyTo(span);

                Pool.Shared.Return(buf, true);
            }

            _index += length;

            return length;
        }
    }

    /// <summary>写入字节数组</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Int32 Write(Byte[] value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        value.CopyTo(_span[_index..]);
        _index += value.Length;

        return value.Length;
    }

    /// <summary>写入Span</summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public Int32 Write(ReadOnlySpan<Byte> span)
    {
        span.CopyTo(_span[_index..]);
        _index += span.Length;

        return span.Length;
    }

    /// <summary>写入Span</summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public Int32 Write(Span<Byte> span)
    {
        span.CopyTo(_span[_index..]);
        _index += span.Length;

        return span.Length;
    }

    /// <summary>写入结构体</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public Int32 Write<T>(T value) where T : struct
    {
        var size = Unsafe.SizeOf<T>();
        EnsureSpace(size);
#if NET8_0_OR_GREATER
        MemoryMarshal.Write(_span.Slice(_index, size), in value);
#else
        MemoryMarshal.Write(_span.Slice(_index, size), ref value);
#endif
        _index += size;
        return size;
    }
    #endregion

    #region 扩展写入
    /// <summary>写入7位压缩编码整数</summary>
    /// <remarks>
    /// 以7位压缩格式写入32位整数，小于7位用1个字节，小于14位用2个字节。
    /// 由每次写入的一个字节的第一位标记后面的字节是否还是当前数据，所以每个字节实际可利用存储空间只有后7位。
    /// </remarks>
    /// <param name="value">数值</param>
    /// <returns>实际写入字节数</returns>
    public Int32 WriteEncodedInt(Int64 value)
    {
        var span = _span[_index..];

        var count = 0;
        var num = (UInt32)value;
        while (num >= 0x80)
        {
            span[count++] = (Byte)(num | 0x80);
            num >>= 7;
        }
        span[count++] = (Byte)num;

        _index += count;

        return count;
    }
    #endregion
}
