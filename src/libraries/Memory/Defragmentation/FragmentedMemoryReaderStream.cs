namespace Ofella.Utilities.Memory.Defragmentation;

public class FragmentedMemoryReaderStream : Stream
{
    private readonly FragmentedMemory<byte> _fragmentedMemory;

    public FragmentedMemoryReaderStream(FragmentedMemory<byte> fragmentedMemory)
    {
        _fragmentedMemory = fragmentedMemory;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _fragmentedMemory.Length;

    public override long Position { get; set; }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _fragmentedMemory.CopyTo(destination);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return _fragmentedMemory.CopyToAsync(destination, cancellationToken).AsTask();
    }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var fragmentedMemorySlice = _fragmentedMemory[(int)Position..(int)(Position + count)];

        fragmentedMemorySlice.CopyTo(buffer.AsMemory()[offset..]);

        Position += fragmentedMemorySlice.Length;

        return fragmentedMemorySlice.Length;
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
