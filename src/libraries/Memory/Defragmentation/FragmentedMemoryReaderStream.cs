namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Provides a generic view of a <see cref="FragmentedMemory{T:Byte}"/>.
/// </summary>
public class FragmentedMemoryReaderStream : Stream
{
    #region Fields & Properties

    private readonly FragmentedMemory<byte> _fragmentedMemory; // The FragmentedMemory<byte> to provide a generic view of.
    private FragmentedMemoryEnumerator _enumerator; // The position inside _fragmentedMemory.
    private long _position; // The position inside this stream.

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a FragmentedMemoryStream from the provided <see cref="FragmentedMemory{T}"/>.
    /// </summary>
    /// <param name="fragmentedMemory">The <see cref="FragmentedMemory{T}"/> to create a Stream from. The stream won't become the owner of this instance, hence no disposing of it will take place.</param>
    public FragmentedMemoryReaderStream(in FragmentedMemory<byte> fragmentedMemory)
    {
        _fragmentedMemory = fragmentedMemory;
        _enumerator = FragmentedMemoryEnumerator.Beginning;
        _position = 0;
    }

    #endregion

    #region Stream Interface

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false; // Not yet supported.

    public override long Length => _fragmentedMemory.Length;

    public override long Position
    {
        get => _position;
        set
        {
            _position = value;
            _enumerator = FragmentedMemoryEnumerator.None; // Setting position manually resets the FragmentedPosition.
        }
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _fragmentedMemory.CopyToAsync(destination).AsTask().GetAwaiter().GetResult();
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return _fragmentedMemory.CopyToAsync(destination, cancellationToken).AsTask();
    }

    public override void Flush() => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (Position >= Length) // Unfavor this branch when BPU has not enough information.
        {
            goto EndOfStream;
        }

        int bytesToCopy;

        if (Position + count <= Length) // Favor this branch when BPU has not enough information.
        {
            bytesToCopy = count;
        }
        else
        {
            bytesToCopy = (int)(Length - Position);
        }

        var fragmentedMemoryToCopy = _fragmentedMemory.Slice((int)Position, bytesToCopy);

        var enumerator = fragmentedMemoryToCopy.CopyTo(buffer.AsMemory()[offset..], _enumerator);

        Position += fragmentedMemoryToCopy.Length; // Update position first, because it resets the _fragmentedPosition.
        _enumerator = enumerator;

        return fragmentedMemoryToCopy.Length;

    EndOfStream:
        return 0;
    }

    public override int ReadByte()
    {
        if (Position >= Length)
        {
            goto EndOfStream; // Unfavor this branch when BPU has not enough information.
        }

        var fragmentedMemoryToCopy = _fragmentedMemory.Slice((int)Position, 1);
        Span<byte> buffer = stackalloc byte[1];

        var enumerator = fragmentedMemoryToCopy.CopyTo(buffer, _enumerator);
        ++Position;
        _enumerator = enumerator;

        return buffer[0];

    EndOfStream:
        return -1;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length - offset;
                break;
        }

        return Position;
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    #endregion
}
