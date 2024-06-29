namespace BufferCrossThread;

public sealed class ExampleBufferWriter
{
    private const int BlockCapacity = 100;
    private readonly ChannelBuffer<float> _buffer;
    private readonly int _sampleRate;
    private readonly Random _random = new ();
    private readonly float[][] _blockBuffer;
    private readonly TimeSpan _handleLifeSpan;

    public ExampleBufferWriter(ChannelBuffer<float> buffer, int sampleRate)
    {
        _buffer = buffer;
        _sampleRate = sampleRate;
        
        // sampleRate in Hz. This is a rough calculation, safety margins need to be implemented.
        _handleLifeSpan =  TimeSpan.FromSeconds((double)_buffer.Capacity / sampleRate);
        
        _blockBuffer = new float[_buffer.ChannelCount][];
        for (var i = 0; i < _buffer.ChannelCount; i++)
        {
            _blockBuffer[i] = new float[BlockCapacity];
        }
    }
    
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => WriteLoopAsync(cancellationToken), TaskCreationOptions.LongRunning);
    }

    private async Task WriteLoopAsync(CancellationToken cancellationToken)
    {
        var period = (double)BlockCapacity / _sampleRate;
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(period));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            PopulateBlockBuffer();
            var section = _buffer.WriteChannelData(_blockBuffer, BlockCapacity);
            var handle = new SampleHandle<float>(_buffer, DateTime.UtcNow + _handleLifeSpan, section);

            BufferWritten?.Invoke(ref handle);
        }
    }

    private void PopulateBlockBuffer()
    {
        for (var channel = 0; channel < _buffer.ChannelCount; channel++)
        {
            for (var sample = 0; sample < BlockCapacity; sample++)
            {
                _blockBuffer[channel][sample] = _random.NextSingle();
            }
        }
    }
    
    public delegate void BufferWrittenDelegate(ref SampleHandle<float> sampleHandle);

    public event BufferWrittenDelegate? BufferWritten;
}