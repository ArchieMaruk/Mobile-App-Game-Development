# Performance Baseline – Whopper

Device: Samsung SM-A025F (Galaxy A02s), Android 12, Adreno 506, OpenGL ES 3, 60 Hz
Target: 60 fps
Unity: 6000.6.0f1

---

## 28 Sep 2026 – Week 3

Build: 0.1.1, Release, commit 7e84603

Frame time (avg / p99)
- Menu: 16.71 ms / 16.88 ms
- Steady gameplay: 16.71 ms / 16.90 ms
- Worst case: 16.71 ms / 16.90 ms (200 stress cubes, always active)

Memory
- GC allocated per frame: 59 B, all Unity internal (FinishFrameRendering 32 B, NewInputFixedUpdate 9 B, PlayerSendFrameStarted 9 B, NewInputUpdate 9 B)
- Peak Total Reserved (Dev): 317.0 MB
- TOTAL PSS (Release): 194.3 MB

Rendering (worst case)
- SetPass calls: 11
- Batches: 0 (not counted with URP)
- Triangles: 3.6k

Startup and size
- Cold start (median of 3): 716 ms
- First interactive: 2.46 s
- APK size: 71.0 MB

---

## 7 Oct 2026 – Week 5 Lab A

Build: 0.2.0 (code 4), Dev, commit a116980

Frame time (avg / p99)
- Steady gameplay: 16.71 ms / 16.80 ms (pooled hazards spawning every 1.5 s)

Memory
- GC allocated per frame: 18 B, all Unity internal (PlayerSendFrameStarted 9 B, NewInputUpdate 9 B)
- GC from spawner, pool and hazards: 0 B

What changed
- Hazards come from a prewarmed pool of 16 and the spawner uses Awaitable, so nothing is instantiated during play.
- Renderer switched from 2D to Universal (3D).

Capture: `docs/CA2/baseline/w05-spawner.data`

---

## 12 Oct 2026 – Week 5 Lab B

Build: 0.3.1 (code 6), Release, commit 86feb90

Startup (budget in brackets)
- Cold start: 423 / 453 / 455 ms, median 453 ms (budget: under 1000 ms; Android flags 5 s as slow)
- Time to interactive: 3.17 s (budget: under 4 s)

Scene loads (budget: under 1000 ms)
- Menu: 717 ms
- Game: 145 ms

Size
- APK: 54.0 MB (ARMv7 + ARM64)

What changed
- Scenes now load through a persistent SceneLoader with a loading bar.
- Cold start dropped from 716 ms (Week 3) to 453 ms.
- Time to interactive is now measured with a `[perf]` log on the Menu's first frame, which is a different method from Week 3's "Fully drawn", so the two aren't directly comparable.

---

## Still to measure

- Thermal delta and throttling (Week 7)