# RingBuffer

固定サイズのリングバッファ (輪になった待ち行列) `RingBuffer<T>`。C#, netstandard2.1。
.NET のアプリからも、Unity (2021.2+) からも使える。Unity のパッケージには、ビルドした DLL を同梱する。

- 作るときに 1 度だけ領域を確保する。入れる・取り出すでは GC が起きない (`T` を構造体にすると、要素ごとの確保も起きない)。
- 満杯のときに入れると、いちばん古い要素を捨てる。取り出す側が遅れても、メモリは増えない。
- スレッドに対して安全 (中で `lock` する)。1 つのスレッドが入れ、別のスレッドが取り出す使い方を想定している。

## 使い方

```csharp
using CIFER.tech.RingBuffer;

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

## 注意点

- 容量は、作ったあとで変えられない。足りなくなるのではなく、**古い要素が捨てられる**。捨てられると困る用途では、容量を十分に大きくするか、`Enqueue` の戻り値 (`true` = 捨てた) を見る。
- 入れる・取り出す・読む・数える、どれも要素の数によらず一定の時間 (`O(1)`)。`Clear` だけは、容量に比例する (参照型を持ち続けないよう、配列を空にするため)。
- `T` を構造体にすると、要素ごとの確保も起きない。`Enqueue(in T)` は参照渡しなので、コピーは配列に書くときの 1 回だけ。
- `lock` は 1 つ。入れる側と取り出す側が同時に動いても壊れないが、同時に処理を進めることはできない (どちらかが待つ)。

## Unity から使う

`Packages/manifest.json` の `dependencies` に、このリポジトリの `Unity` フォルダを足す。

```json
"tech.cifer.ringbuffer": "file:../../../RingBuffer/Unity"
```

パスは `Packages/manifest.json` からの相対パス。Git から入れるときは、次のように書く。

```json
"tech.cifer.ringbuffer": "https://github.com/CIFER-tech/RingBuffer.git?path=/Unity"
```

DLL は `Unity/Runtime/CIFER.tech.RingBuffer.dll` (アセンブリ名 `CIFER.tech.RingBuffer`)。自分のアセンブリ定義の `references` に `CIFER.tech.RingBuffer` を書いていた場合は、それを削除する (DLL は自動で参照される)。`overrideReferences` を有効にしているアセンブリ定義だけ、`precompiledReferences` に `CIFER.tech.RingBuffer.dll` を足す。

## リポジトリの構成

| パス | 内容 |
| --- | --- |
| `Unity/` | UPM パッケージ (`tech.cifer.ringbuffer`)。`Unity/Runtime/` にビルドした DLL を同梱する。`main` に `src/RingBuffer` の変更が入ると、GitHub Actions が DLL を作り直してコミットする。 |
| `src/RingBuffer/` | .NET のライブラリ。ソースは `src/RingBuffer/RingBuffer.cs` の 1 つだけ。 |
| `tests/RingBuffer.Tests/` | xUnit のテスト。 |

## .NET から使う

タグ (`v*`) を付けると、GitHub Actions が `.nupkg` を作って、GitHub の Releases に添付する。nuget.org には公開しない。取り出した `.nupkg` を置いたフォルダを、NuGet のローカルのソースにして使う。

## ビルドとテスト

```
dotnet build
dotnet test
```
