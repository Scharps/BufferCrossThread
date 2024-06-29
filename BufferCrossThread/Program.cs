using BufferCrossThread;

const int channels = 256;
const int sampleRate = 400;
const int bufferRetentionDuration = 60;
const int capacity = sampleRate * bufferRetentionDuration;


var buffer = new ChannelBuffer<float>(channels, capacity);
var writer = new ExampleBufferWriter(buffer, sampleRate);

var samplesWritten = 0;
writer.BufferWritten += (ref SampleHandle<float> handle) =>
{
    Console.WriteLine(++samplesWritten);
};

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
var t = writer.RunAsync(cts.Token);
await t;