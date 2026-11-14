# RingBuffer

固定サイズのリングバッファ (輪になった待ち行列) `RingBuffer<T>`。C#, netstandard2.1。
.NET のアプリからも、Unity (2021.2+) からも使える。

- 作るときに 1 度だけ領域を確保する。入れる・取り出すでは GC が起きない (`T` を構造体にすると、要素ごとの確保も起きない)。
- 満杯のときに入れると、いちばん古い要素を捨てる。取り出す側が遅れても、メモリは増えない。
- スレッドに対して安全 (中で `lock` する)。1 つのスレッドが入れ、別のスレッドが取り出す使い方を想定している。

## 使い方

```csharp
using Cifer.Collections;

var buffer = new RingBuffer<int>(capacity: 3);

buffer.Enqueue(1);
buffer.Enqueue(2);
buffer.Enqueue(3);
bool isDropped = buffer.Enqueue(4);   // 満杯なので、いちばん古い 1 を捨てて true

while (buffer.TryDequeue(out int item))
{
    // 2, 3, 4 の順に取り出す
}
```

| メンバー | 説明 |
| --- | --- |
| `RingBuffer<T>(int capacity)` | 容量を決めて作る。1 以上。 |
| `bool Enqueue(in T item)` | 末尾に入れる。古い要素を捨てたとき true。 |
| `bool TryDequeue(out T item)` | 先頭 (いちばん古い) を取り出す。空のとき false。 |
| `bool TryPeek(out T item)` | 先頭を取り出さずに読む。空のとき false。 |
| `void Clear()` | すべて捨てる。 |
| `int Capacity` / `int Count` | 容量 / いま入っている数。 |

## Unity から使う

`Packages/manifest.json` の `dependencies` に、このリポジトリの `Unity` フォルダを足す。

```json
"tech.cifer.ringbuffer": "file:../../../RingBuffer/Unity"
```

パスは `Packages/manifest.json` からの相対パス。Git から入れるときは、次のように書く。

```json
"tech.cifer.ringbuffer": "https://github.com/CIFER-tech/RingBuffer.git?path=/Unity"
```

アセンブリは `RingBuffer` (Unity のアセンブリ定義)。自分のアセンブリ定義から使うときは、`references` に `RingBuffer` を足す。

## リポジトリの構成

| パス | 内容 |
| --- | --- |
| `Unity/` | UPM パッケージ (`tech.cifer.ringbuffer`)。ソースは `Unity/Runtime/RingBuffer.cs` の 1 つだけ。 |
| `src/RingBuffer/` | .NET のライブラリ。`Unity/Runtime/RingBuffer.cs` を、そのままビルドする。 |
| `tests/RingBuffer.Tests/` | xUnit のテスト。 |

## ビルドとテスト

```
dotnet build
dotnet test
```
