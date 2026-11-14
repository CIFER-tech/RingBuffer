using System;

namespace Cifer.Collections
{
    /// <summary>
    /// 固定サイズの輪になった待ち行列 (リングバッファ)。先に入れた要素から順に取り出す。
    /// 要素の配列を作るときに 1 度だけ領域を確保し、入れる・取り出すでは GC が起きない。
    /// 満杯のときに入れると、いちばん古い要素を捨てる。取り出す側が遅れても、メモリは増えない。
    /// スレッドに対して安全 (中で lock する)。1 つのスレッドが入れ、別のスレッドが取り出す使い方を想定している。
    /// </summary>
    /// <typeparam name="T">要素の型。構造体にすると、要素ごとの確保も起きない。</typeparam>
    public sealed class RingBuffer<T>
    {
        private readonly T[] _items;
        private readonly object _lock = new object();

        // 最も古い要素の位置と、入っている要素の数。
        private int _head;
        private int _count;

        /// <param name="capacity">入る要素の数の上限。1 以上。</param>
        /// <exception cref="ArgumentOutOfRangeException">capacity が 0 以下のとき。</exception>
        public RingBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "capacity は 1 以上にする。");
            }

            _items = new T[capacity];
        }

        /// <summary>入る要素の数の上限。</summary>
        public int Capacity
        {
            get
            {
                return _items.Length;
            }
        }

        /// <summary>いま入っている要素の数。</summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _count;
                }
            }
        }

        /// <summary>末尾に要素を入れる。満杯のときは、いちばん古い要素を捨てて入れる。</summary>
        /// <returns>古い要素を捨てたとき true。</returns>
        public bool Enqueue(in T item)
        {
            lock (_lock)
            {
                // _head + _count は、容量が int.MaxValue の半分を超えると、あふれる。足す前に、容量から引いて比べる。
                int tail = _head >= _items.Length - _count
                    ? _head - (_items.Length - _count)
                    : _head + _count;
                _items[tail] = item;

                if (_count < _items.Length)
                {
                    _count++;
                    return false;
                }

                // 満杯だったので、末尾に書いた場所は、いちばん古い要素の場所。次に古い要素が先頭になる。
                _head = (_head + 1) % _items.Length;
                return true;
            }
        }

        /// <summary>先頭 (いちばん古い) の要素を取り出す。</summary>
        /// <returns>要素があったとき true。空のときは false で、item は既定値。</returns>
        public bool TryDequeue(out T item)
        {
            lock (_lock)
            {
                if (_count == 0)
                {
                    item = default;
                    return false;
                }

                item = _items[_head];
                // 参照型の要素を、いつまでも持ち続けないようにする。
                _items[_head] = default;
                _head = (_head + 1) % _items.Length;
                _count--;
                return true;
            }
        }

        /// <summary>先頭 (いちばん古い) の要素を、取り出さずに読む。</summary>
        /// <returns>要素があったとき true。空のときは false で、item は既定値。</returns>
        public bool TryPeek(out T item)
        {
            lock (_lock)
            {
                if (_count == 0)
                {
                    item = default;
                    return false;
                }

                item = _items[_head];
                return true;
            }
        }

        /// <summary>要素をすべて捨てる。</summary>
        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_items, 0, _items.Length);
                _head = 0;
                _count = 0;
            }
        }
    }
}
