namespace Ofella.Utilities.Memory.Defragmentation;

public class FragmentedMemoryReaderStream : Stream
{
    private readonly FragmentedMemory<byte> _fragmentedMemory;
    private FragmentedPosition? _fragmentedPosition;
    private long _position;

    public FragmentedMemoryReaderStream(FragmentedMemory<byte> fragmentedMemory)
    {
        _fragmentedMemory = fragmentedMemory;
        _fragmentedPosition = null;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _fragmentedMemory.Length;

    public override long Position
    {
        get => _position;
        set
        {
            _position = value;
            _fragmentedPosition = null;
        }
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _fragmentedMemory.CopyToAsync(destination, null).AsTask().GetAwaiter().GetResult();
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return _fragmentedMemory.CopyToAsync(destination, null, cancellationToken).AsTask();
    }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

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

        var fragmentedMemorySlice = _fragmentedMemory.Slice((int)Position, bytesToCopy);

        var fragmentedPosition = fragmentedMemorySlice.CopyTo(buffer.AsMemory()[offset..], _fragmentedPosition);
        Position += fragmentedMemorySlice.Length;
        _fragmentedPosition = fragmentedPosition;

        return fragmentedMemorySlice.Length;

    EndOfStream:
        return 0;
    }

    public override int ReadByte()
    {
        if (Position >= Length)
        {
            goto EndOfStream; // Unfavor this branch when BPU has not enough information.
        }

        var fragmentedMemorySlice = _fragmentedMemory.Slice((int)Position, 1);
        Span<byte> buffer = stackalloc byte[1];

        var fragmentedPosition = fragmentedMemorySlice.CopyTo(buffer, _fragmentedPosition);
        ++Position;
        _fragmentedPosition = fragmentedPosition;

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
                Position = Length - offset - 1;
                break;
        }

        return Position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
