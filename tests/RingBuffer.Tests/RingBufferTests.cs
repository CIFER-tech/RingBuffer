using CIFER.tech.RingBuffer;
using Xunit;

namespace CIFER.tech.RingBuffer.Tests;

public class RingBufferTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_容量が1未満なら例外(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(capacity));
    }

    [Fact]
    public void TryDequeue_空のとき_falseと既定値を返す()
    {
        var buffer = new RingBuffer<int>(3);

        bool isDequeued = buffer.TryDequeue(out int item);

        Assert.False(isDequeued);
        Assert.Equal(0, item);
    }

    [Fact]
    public void TryDequeue_入れた順に取り出す()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Enqueue(1);
        buffer.Enqueue(2);
        buffer.Enqueue(3);

        Assert.True(buffer.TryDequeue(out int first));
        Assert.True(buffer.TryDequeue(out int second));
        Assert.True(buffer.TryDequeue(out int third));

        Assert.Equal(new[] { 1, 2, 3 }, new[] { first, second, third });
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Enqueue_満杯のとき_いちばん古い要素を捨ててtrueを返す()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Enqueue(1);
        buffer.Enqueue(2);
        buffer.Enqueue(3);

        bool isDropped = buffer.Enqueue(4);

        Assert.True(isDropped);
        Assert.Equal(3, buffer.Count);
        buffer.TryDequeue(out int first);
        buffer.TryDequeue(out int second);
        buffer.TryDequeue(out int third);
        Assert.Equal(new[] { 2, 3, 4 }, new[] { first, second, third });
    }

    [Fact]
    public void Enqueue_満杯でないとき_falseを返す()
    {
        var buffer = new RingBuffer<int>(3);

        Assert.False(buffer.Enqueue(1));
        Assert.False(buffer.Enqueue(2));
        Assert.False(buffer.Enqueue(3));
    }

    [Fact]
    public void 入れる取り出すを繰り返して一周以上しても_順序が保たれる()
    {
        // 容量 3 に対して、2 つ入れて 2 つ取り出すのを繰り返す。あふれずに、位置だけが何周もする。
        var buffer = new RingBuffer<int>(3);
        var dequeued = new List<int>();

        for (int value = 0; value < 20; value += 2)
        {
            buffer.Enqueue(value);
            buffer.Enqueue(value + 1);
            buffer.TryDequeue(out int first);
            buffer.TryDequeue(out int second);
            dequeued.Add(first);
            dequeued.Add(second);
        }

        Assert.Equal(Enumerable.Range(0, 20), dequeued);
    }

    [Fact]
    public void TryPeek_取り出さずに先頭を読む()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Enqueue(7);
        buffer.Enqueue(8);

        Assert.True(buffer.TryPeek(out int item));

        Assert.Equal(7, item);
        Assert.Equal(2, buffer.Count);
    }

    [Fact]
    public void TryPeek_空のとき_falseを返す()
    {
        var buffer = new RingBuffer<int>(3);

        Assert.False(buffer.TryPeek(out _));
    }

    [Fact]
    public void Clear_すべて捨てる()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Enqueue(1);
        buffer.Enqueue(2);

        buffer.Clear();

        Assert.Equal(0, buffer.Count);
        Assert.False(buffer.TryDequeue(out _));
        buffer.Enqueue(9);
        buffer.TryDequeue(out int item);
        Assert.Equal(9, item);
    }

    [Fact]
    public void Capacity_作ったときの容量を返す()
    {
        Assert.Equal(5, new RingBuffer<string>(5).Capacity);
    }

    [Fact]
    public void 入れる取り出すで_確保が起きない()
    {
        var buffer = new RingBuffer<long>(8);
        // JIT などの初回の確保を、計測の前に済ませる。
        buffer.Enqueue(0);
        buffer.TryDequeue(out _);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (long value = 0; value < 1000; value++)
        {
            buffer.Enqueue(value);
            buffer.TryDequeue(out _);
        }

        long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocatedBytes);
    }

    [Fact]
    public void 別スレッドが入れて_こちらが取り出しても_順序が保たれる()
    {
        const int itemCount = 10000;
        var buffer = new RingBuffer<int>(itemCount);
        var producer = new Thread(() =>
        {
            for (int value = 0; value < itemCount; value++)
            {
                buffer.Enqueue(value);
            }
        });

        producer.Start();
        var received = new List<int>();
        while (received.Count < itemCount)
        {
            if (buffer.TryDequeue(out int item))
            {
                received.Add(item);
            }
        }

        producer.Join();
        Assert.Equal(Enumerable.Range(0, itemCount), received);
    }
}
