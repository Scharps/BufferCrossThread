using System.Runtime.CompilerServices;

namespace BufferCrossThread;

public class DummyClass
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AppendToChannel<T>(int channel, IEnumerable<T> append)
    {
    }
}