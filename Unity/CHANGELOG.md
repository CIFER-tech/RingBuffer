# Changelog

## [2.0.0] - 2026-11-14

互換性のない変更。

- 名前空間を `Cifer.Collections` から `CIFER.tech.RingBuffer` に変更 (組織名 CIFER.tech に合わせる)。使う側の `using` を直す必要がある。
- アセンブリ (Unity のアセンブリ定義と .NET のアセンブリ) の名前を `RingBuffer` から `CIFER.tech.RingBuffer` に変更。アセンブリ定義から参照している側は、`references` を直す必要がある。
- パッケージの作者名を `CIFER.tech` に変更。

## [1.0.0] - 2026-11-14

- 固定サイズのリングバッファ `RingBuffer<T>` を追加。
  - `Enqueue` / `TryDequeue` / `TryPeek` / `Clear`、`Capacity` / `Count`。
  - 作るときに 1 度だけ領域を確保し、入れる・取り出すでは GC が起きない。
  - 満杯のときに入れると、いちばん古い要素を捨てる。
  - スレッドに対して安全 (中で lock する)。
