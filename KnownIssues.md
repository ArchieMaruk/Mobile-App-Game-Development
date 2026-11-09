# Known Issues – Whopper

## 1. Swipe only triggers when the finger lifts
- Scenario: normal play
- Steps: start a run, swipe up and keep the finger on the screen
- Expected: jump happens as soon as the swipe is long enough
- Actual: jump only happens when the finger is lifted, which feels laggy
- Found in: Week 2 accessibility playtest
- Build: 0.6.0 (28fbc052), Galaxy S26+, Android 17

## 2. Memory over budget on the new phone
- Scenario: 5 minutes of play
- Steps: play for 5 min, then `adb shell dumpsys meminfo com.archil.whopper`
- Expected: TOTAL PSS under 300 MB (MDA budget)
- Actual: TOTAL PSS 374 MB
- Log line: `TOTAL PSS: 382937`
- Build: 0.6.0 (28fbc052), Galaxy S26+, Android 17