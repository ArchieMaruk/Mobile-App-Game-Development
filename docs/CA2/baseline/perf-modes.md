# Performance modes – 22/10/2026

Device: Samsung Galaxy A02s (SM-A025F), Android 12, 60 Hz. Release build 0.5.0.

| Mode | targetFrameRate | Render scale | Frame time avg (ms) | 99th (ms) | Bottleneck | Panel Hz |
|------|-----------------|--------------|---------------------|-----------|------------|----------|
| Balanced (default) | 60 | 1.0 | 16.71 | 16.78 | TargetFrameRate | 60 |
| High FPS | 120 | 0.8 | 16.71 | 16.79 | TargetFrameRate | 60 |

- Both modes give the same frame time, because the phone's screen is 60 Hz, so it can't go above 60 fps even when 120 is asked for.
- High FPS only lowers the render scale on this phone.
- The bottleneck is TargetFrameRate in both modes, meaning the game is hitting its frame cap with time to spare. It flips briefly to GPU when a run starts.
- `[AP] active warning=NoWarning`: no thermal warnings during the test.
- A 90/120 Hz phone (e.g. the Galaxy S25) would be needed to see High FPS actually go above 60.