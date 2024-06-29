namespace BufferCrossThread;

/// <summary>
/// Represents a handle to a sample in a multi-channel circular buffer.
/// </summary>
/// <typeparam name="T">The type of elements in the circular buffer.</typeparam>
public readonly struct SampleHandle<T>
    where T: unmanaged
{
    private readonly ChannelBuffer<T> _buffer;
    private readonly DateTime _expiry;
    private readonly Range _range;

    public int SampleLength => _range.End.Value < _range.Start.Value
        ? _range.End.Value + _buffer.Capacity - _range.Start.Value
        : _range.End.Value - _range.Start.Value;
    
    internal SampleHandle(ChannelBuffer<T> buffer, DateTime expiry, Range range)
    {
        _buffer = buffer;
        _expiry = expiry;
        _range = range;
    }

    public bool HasExpired => DateTime.UtcNow >= _expiry;

    public bool TryCopyTo(int channel, Span<T> destination)
    {
        return !HasExpired && _buffer.TryCopyTo(channel, destination, _range);
    }
}