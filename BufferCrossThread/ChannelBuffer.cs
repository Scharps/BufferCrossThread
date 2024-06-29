namespace BufferCrossThread;

public sealed class ChannelBuffer<T>
    where T : unmanaged
{
    private int _cursor;
    private readonly T[][] _buffer;
    
    public int Capacity { get; }
    public int ChannelCount { get; }

    public ChannelBuffer(int channels, int capacity)
    {
        Capacity = capacity;
        ChannelCount = channels;
        _buffer = new T[channels][];
        
        // Initialise channel buffers
        for (var i = 0; i < channels; i++)
        {
            _buffer[i] = new T[capacity];
        }
    }
    public Range WriteChannelData(T[][] dataBatch, int length)
    {
        if (_buffer.Length != dataBatch.Length)
        {
            throw new ArgumentException($"{nameof(dataBatch)} does not have the same length as the number of channels.");
        }

        for (var channel = 0; channel < _buffer.Length; channel++)
        {
            CopyData(channel, dataBatch[channel], length);
        }
        
        var cursorBefore = _cursor;
        AdvanceCursor(length);
        var cursorAfter = _cursor;
        
        return cursorBefore..cursorAfter;
    }

    private void CopyData(int channel, T[] dataBatch, int length)
    {
        var isOverflow = _cursor + length > Capacity;
        if (!isOverflow) 
        {
            Array.Copy(dataBatch, 0, _buffer[channel], _cursor, length);
        }
        else
        {
            var firstCopyLength = Capacity - _cursor;
            var secondCopyLength = length - firstCopyLength;
            Array.Copy(dataBatch, 0, _buffer[channel], _cursor, firstCopyLength);
            Array.Copy(dataBatch, firstCopyLength, _buffer[channel], 0, secondCopyLength);
        }
    }

    private void AdvanceCursor(int length)
    {
        _cursor = (_cursor + length) % Capacity;
    }

    internal bool TryCopyTo(int channel, Span<T> destination, Range range)
    {
        throw new NotImplementedException();
    }
}