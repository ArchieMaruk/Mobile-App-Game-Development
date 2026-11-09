# Week 8, Lab A (2h): Peer distribution: APK, QR code and feedback

**Module:** Mobile Game Development (A12581) · **Engine:** Unity 6.6 · **Platform:** Android (sideload) · **Date:** Mon 2 Nov 2026
**Feeds:** CA3 Performance & Quality Dossier + Final Build (due Fri 4 Dec 2026) via Async Pack 3; CA2 Playable Vertical Slice (due Sun 15 Nov 2026) · **LOs:** LO2, LO4

> **Goal:** Distribute your release APK to classmates by QR code the way a studio does a closed test without a store, stamp every build so feedback maps to one binary, and launch a structured feedback form. Async Pack 3 (Peer APK Distribution & Survey) starts today and runs to Sun 29 Nov 2026.

## What you will complete today
1. A release-signed APK hosted on OneDrive or Google Drive with a QR code and a SHA-256 hash.
2. `BuildStamp.cs` on your About screen showing version, build ID and device details.
3. An anonymous feedback form with the Part C fields, tested on your own phone.
4. Your build installed on at least two peer phones, with their first responses submitted.
5. A device matrix started in `/docs/CA3/qa/device-matrix.md` and Pack 3 launched.

## Pre-flight
- Welcome back from Reading Week. Async Pack 1 was due yesterday (Sun 1 Nov); submit today if you missed it.
- A working release build of your slice on your phone (Development Build **off**, IL2CPP, ARM64, release keystore).
- A telemetry logger from Week 6 with a `session_start` event.
- Your SETU Microsoft 365 account signed in (OneDrive and Forms), or a Google account.
- A USB cable and `adb` on the path; charged phone.

## Part A: Build, host, QR (~15 min)
1. **Edit > Project Settings > Player > Android > Other Settings**: bump **Version** (for example 0.8.0) and **Bundle Version Code** by one. Confirm the release keystore is selected under **Publishing Settings**.
2. **File > Build Profiles > Android**: Development Build off, **Build** to `Builds/MyGame-0.8.0-arm64.apk`.
3. Compute the hash so testers (and you) can prove which file they got:

```
certutil -hashfile Builds\MyGame-0.8.0-arm64.apk SHA256
```

   On macOS or Linux: `shasum -a 256 Builds/MyGame-0.8.0-arm64.apk`.
4. Upload the APK to a `Builds` folder on your SETU OneDrive (or Google Drive). Share with **Anyone with the link, view only**. Do not post it on public forums or Discord servers: this is a closed test.
5. Paste the share link into any QR generator (the one built into Chrome's share menu works), download the PNG, and put it on your laptop screen. Save it as `/docs/CA3/qa/qr-0.8.0.png`.

*Expected result: scanning the QR on a phone opens the file with a Download button. Play Protect and "install unknown apps" prompts come later, in Part D.*

## Part B: Build stamp (~15 min)
1. If you do not have an About screen yet, add a panel to your settings menu with a **Text - TextMeshPro** label named `BuildLabel`. Android back closes the panel; there is no Quit button.
2. Create `Assets/Scripts/UI/BuildStamp.cs` with the deck code (Part 3 slide) and drag `BuildLabel` into `label`.
3. Add the same stamp to your telemetry: in your `session_start` event, include `version`, `build` (the eight-character GUID prefix) and `SystemInfo.deviceModel` as parameters.
4. Rebuild (repeat Part A steps 2 to 5; the GUID changes with every build, so re-upload and re-hash). Install locally and check the About screen against the installed package:

```
adb shell dumpsys package com.setu.mygame | findstr "versionName versionCode"
```

*Expected result: the About screen shows `v0.8.0 (a1b2c3d4)`, the device model, Android version, RAM, resolution and panel Hz; `dumpsys` shows the same versionName.*

## Part C: Create the feedback form (~20 min)
1. Open **Microsoft Forms** (forms.office.com) with your SETU account, or Google Forms. Create *MyGame closed test, build 0.8.0*. Turn on anonymous responses; do not collect names or emails.
2. Add these questions in this order (type in brackets):
   - **Device model and Android version** (choice, from your device matrix, plus *Other* with text).
   - **Build ID** shown on the About screen (short text).
   - **Install** (choice): worked first time / needed help / failed. If failed, what did the phone say? (text).
   - **Minutes played** (number).
   - **Crash, freeze or "not responding" seen?** (choice: none / freeze / crash / not responding). When, and what were you doing? (text).
   - **Feel, 1 to 5** (rating grid): smoothness, heat, battery drain, control responsiveness.
   - **Loop, 1 to 5** (rating grid): I understood the goal; I wanted one more go; difficulty felt fair.
   - **One thing to fix** (text) and **One thing to keep** (text).
   - **Consent** (required checkbox): anonymous responses used for a university project; no personal data collected.
3. Test it on your own phone, then delete your test response. Copy the short share link.
4. Put the form link on the About screen text as well (`Feedback: <short link>`) so testers can find it after the lab.

*Expected result: a form that takes under three minutes and exports to CSV (Responses > Open in Excel / Download).*

## Part D: Swap phones (~30 min)
1. Pair up, then rotate so each of you tests two other builds. Testers follow the six steps on the Part 2 slide:
   1. Scan the QR, download the APK.
   2. Open it from the notification or the Files app.
   3. Android 8 and later ask per app: **Settings > Apps > Special app access > Install unknown apps > Chrome (or Files) > Allow**. Chrome and Files are separate switches.
   4. Play Protect may warn about an unknown developer: **More details > Install anyway**.
   5. Install, open, go to the About screen, note the build ID.
   6. Play for five minutes, then fill in the form.
2. While your build is being tested, test theirs. Watch for the tester's first thirty seconds: what they tap, where they hesitate. Do not explain the game; write the hesitation down.
3. Common install failures and what to do are in Troubleshooting. Note every failure in the form: a failed install on a peer device is data.

*Expected result: two responses in your form from two different phones, and two test sessions logged on other people's builds.*

## Part E: Device matrix and Pack 3 launch (~10 min)
1. Create `/docs/CA3/qa/device-matrix.md`:

| Phone | SoC | RAM | Android | Panel Hz | Tier (Low/Mid/High) | Owner (initials) | Builds tested |
|-------|-----|-----|---------|----------|---------------------|------------------|---------------|
| Your phone | | | | | | you | 0.8.0 |
| Peer 1 | | | | | | | 0.8.0 |
| Peer 2 | | | | | | | 0.8.0 |

   Get SoC and RAM from the About screen stamp or from **Settings > About phone**. Aim to add a low-tier phone this week if both peers are mid or high.
2. **Async Pack 3: Peer APK Distribution & Survey** is now live. It runs to **Sun 29 Nov 2026** and feeds the CA3 QA evidence pack:
   - Share the QR and form with at least **10 testers** (classmates, other years, friends; avoid a family-only sample).
   - Export the responses to **CSV** and commit it to `/docs/CA3/qa/survey/`.
   - Write **5 to 8 insights** with an immediate fix for each in `/docs/CA3/qa/survey/insights.md`.
   - Re-share a new QR every time you ship a build; keep the build ID column in the CSV.
3. Post your QR image, the SHA-256 and the form link in the Moodle forum thread for this week.

## Troubleshooting
- **`INSTALL_FAILED_UPDATE_INCOMPATIBLE` or "App not installed"** -> the tester has an older build signed with a different keystore (from Week 1 debug builds, usually). Uninstall the old app first.
- **"App not installed" with the same signature** -> versionCode is lower than the installed one. Bump **Bundle Version Code** and rebuild.
- **Play Protect blocks the install with no "Install anyway" option** -> tap *More details* first; on some phones the option is behind it. If still blocked, sideload with `adb install -r` for today and note it.
- **QR opens a preview instead of downloading** -> OneDrive shows a preview for unknown types; use the Download button or share the *Download* link variant.
- **Tester's phone says "parse error"** -> a 32-bit-only or Android version below your Minimum API level. Record the phone in the matrix as *unsupported* and move on.
- **Form responses show a build ID you never shipped** -> the tester typed it from memory. Ask them to copy from the About screen; add a hint to the question.

## Checkpoints to commit
1. `Assets/Scripts/UI/BuildStamp.cs` and the About screen prefab or scene.
2. `/docs/CA3/qa/qr-0.8.0.png` and `/docs/CA3/qa/builds.md` listing version, build ID, SHA-256 and share link per build.
3. `/docs/CA3/qa/device-matrix.md` with at least three rows.
4. `/docs/CA3/qa/survey/form-link.md` with the form link and the field list.
5. Telemetry `session_start` now carries `version`, `build` and `device`.

## Exit ticket
Post your APK link, its SHA-256 and your form link in the Moodle forum; your device matrix must list at least two peer phones with their tier.

## Reference links
- Alternative distribution on Android (installing outside Google Play): https://developer.android.com/distribute/marketing-tools/alternative-distribution
- `adb` reference (install, shell): https://developer.android.com/tools/adb
- `dumpsys` reference: https://developer.android.com/tools/dumpsys
- `Application.buildGUID`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-buildGUID.html
- `Application.version`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-version.html
- `SystemInfo`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SystemInfo.html
- Android Player settings (versionCode, keystore): https://docs.unity3d.com/6000.6/Documentation/Manual/class-PlayerSettingsAndroid.html
- Microsoft Forms: https://forms.office.com
- Unity 6.6 manual index: https://docs.unity3d.com/6000.6/Documentation/Manual/index.html
