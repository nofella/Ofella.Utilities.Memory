namespace Ofella.Utilities.Memory.Defragmentation;

public static class FragmentedMemory
{
    private readonly record struct Test(int Index, int Offset);

    #region Defragmentation of Memory<T>[]

    public static void Copy<T>(Memory<T>[] source, Memory<T> target)
    {
        int offset = 0;

        for (var i = 0; i < source.Length; ++i)
        {
            source[i].CopyTo(target[offset..]);
            offset += source[i].Length;
        }
    }

    public static void Copy(Memory<byte>[] source, Stream target)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            target.Write(source[i].Span);
        }
    }

    public static async ValueTask CopyAsync(Memory<byte>[] source, Stream target, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            await target.WriteAsync(source[i], cancellationToken);
        }
    }

    #endregion

    #region Defragmentation of object[] where object is T[]

    public static void Copy<T>(object[] source, Memory<T> target)
    {
        int offset = 0;

        for (var i = 0; i < source.Length; ++i)
        {
            var fragment = (T[])source[i];
            fragment.CopyTo(target[offset..]);
            offset += fragment.Length;
        }
    }

    public static void Copy(object[] source, Stream target)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            var buffer = (byte[])source[i];
            target.Write(buffer);
        }
    }

    public static async ValueTask CopyAsync(object[] source, Stream target, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            var buffer = (byte[])source[i];
            await target.WriteAsync(buffer, cancellationToken);
        }
    }

    #endregion

    #region Fragmentation to Memory<T>[]

    public static void Copy<T>(Memory<T> source, Memory<T>[] target)
    {
        int offset = 0;

        for (var i = 0; i < target.Length; ++i)
        {
            source[offset..(offset + target[i].Length)].CopyTo(target[i]);
            offset += target[i].Length;
        }
    }

    public static void Copy(Stream source, Memory<byte>[] target)
    {
        for (var i = 0; i < target.Length; ++i)
        {
            source.Read(target[i].Span);
        }
    }

    public static async ValueTask CopyAsync(Stream source, Memory<byte>[] target)
    {
        for (var i = 0; i < target.Length; ++i)
        {
            await source.ReadAsync(target[i]);
        }
    }

    #endregion

    #region Fragmentation to object[] where object is T[]

    public static void Copy<T>(Memory<T> source, object[] target)
    {
        int offset = 0;

        for (var i = 0; i < target.Length; ++i)
        {
            var fragment = (T[])target[i];
            source[offset..(offset + fragment.Length)].CopyTo(fragment);
            offset += fragment.Length;
        }
    }

    public static void Copy(Stream source, object[] target)
    {
        for (var i = 0; i < target.Length; ++i)
        {
            var buffer = (byte[])target[i];
            source.Read(buffer);
        }
    }

    public static async ValueTask CopyAsync(Stream source, object[] target)
    {
        for (var i = 0; i < target.Length; ++i)
        {
            var buffer = (byte[])target[i];
            await source.ReadAsync(buffer);
        }
    }

    #endregion
}
