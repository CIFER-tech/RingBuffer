# Changelog

## [1.0.0] - 2026-11-14

- 固定サイズのリングバッファ `RingBuffer<T>` を追加。
  - `Enqueue` / `TryDequeue` / `TryPeek` / `Clear`、`Capacity` / `Count`。
  - 作るときに 1 度だけ領域を確保し、入れる・取り出すでは GC が起きない。
  - 満杯のときに入れると、いちばん古い要素を捨てる。
  - スレッドに対して安全 (中で lock する)。
