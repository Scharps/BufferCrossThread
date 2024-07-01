using System.Buffers;
using R3;

namespace BufferCrossThread;

public sealed class ExampleBufferWriter
{
    private const int BlockCapacity = 30;
    private readonly ChannelBuffer<float> _buffer;
    private readonly int _sampleRate;
    private readonly Random _random = new ();
    private readonly ArrayPool<List<float>> _blockPool;
    
    private readonly Subject<IReadOnlyList<IReadOnlyList<float>>> _bufferWritten = new();
    public Observable<IReadOnlyList<IReadOnlyList<float>>> BufferWritten => _bufferWritten.AsObservable();

    public ExampleBufferWriter(ChannelBuffer<float> buffer, int sampleRate)
    {
        _buffer = buffer;
        _sampleRate = sampleRate;
        _blockPool = ArrayPool<List<float>>.Create(_buffer.ChannelCount, _buffer.ChannelCount * 2);
    }
    
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() => WriteLoopAsync(cancellationToken), cancellationToken);
    }

    private async Task WriteLoopAsync(CancellationToken cancellationToken)
    {
        var period = (double)BlockCapacity / _sampleRate;
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(period));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var blockBuffer = _blockPool.Rent(_buffer.ChannelCount);
    
            var count = ClearAndPopulateBlockBuffer(blockBuffer);
            _buffer.WriteChannelData(blockBuffer, count);
            _bufferWritten.OnNext(blockBuffer.AsReadOnly());
    
            _blockPool.Return(blockBuffer);
        }
    }

    private int ClearAndPopulateBlockBuffer(List<float>?[] blockBuffer)
    {
        var numOfSamples = _random.Next(20, 40);
        
        for (var channel = 0; channel < _buffer.ChannelCount; channel++)
        {
            var channelBuffer = blockBuffer[channel] ??= [];
            channelBuffer.Clear();
            for (var sample = 0; sample < numOfSamples; sample++)
            {
                channelBuffer.Add(_random.NextSingle());
            }
        }

        return numOfSamples;
    }
}