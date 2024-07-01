using BufferCrossThread;
using R3;

const int channels = 256;
const int sampleRate = 16000;
const int bufferRetentionDuration = 60;
const int capacity = sampleRate * bufferRetentionDuration;

var buffer = new ChannelBuffer<float>(channels, capacity);
var writer = new ExampleBufferWriter(buffer, sampleRate);

ObservableSystem.DefaultFrameProvider = new TimerFrameProvider(TimeSpan.FromSeconds(1.0/60));

writer.BufferWritten
    .ChunkFrame(1)
    .Subscribe(chunk =>
    {
        foreach (var block in chunk)
        {
            for (var channel = 0; channel < block.Count; channel++)
            {
                var channelData = block[channel];
                DummyClass.AppendToChannel(channel, channelData);
            }
        }
    });

var t = writer.RunAsync();
await t;