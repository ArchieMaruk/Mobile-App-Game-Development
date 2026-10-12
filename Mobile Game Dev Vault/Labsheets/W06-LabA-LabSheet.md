# Week 6, Lab A (2h): Economy loop: earn, spend, upgrade and IAP awareness

**Module:** Mobile Game Development (A12581) · **Engine:** Unity 6.6 · **Platform:** Android (sideload) · **Date:** Mon 12 Oct 2026
**Feeds:** CA2 Playable Vertical Slice (due Sun 15 Nov 2026, 23:59) · **LOs:** LO2, LO3

> **Goal:** Give your slice a soft-currency loop (earn on the core verb, spend on upgrades that change play), pace it so the first upgrade lands inside two minutes, and stub the store behind `IPurchaseProvider` so a real SDK could be dropped in later without touching gameplay code.

## What you will complete today
1. `IPurchaseProvider` and `Wallet` (Coins, Level, `Earn`, `TryUpgrade`, `Restore`, `Changed` event) bound to a HUD.
2. `Earn` called from your core verb and an upgrade that changes a real gameplay value.
3. `FakePurchaseProvider` and `CoinShop` with a Buy Pack button that logs `[iap-stub]`.
4. `WalletSave` persisting coins and level through relaunch.
5. A pacing pass with numbers in the journal.

## Pre-flight
- Week 5 checkpoints: scene flow, greybox lane, `SceneLoader`, tag `v0.3.1-lane`.
- TextMeshPro in the project (it is, in Unity 6.6, via the UI packages).
- Phone plugged in; `adb logcat -s Unity` works.
- Read the deck's IAP models and dark-patterns slides: no IAP SDK is installed this semester, and nothing you build today talks to a store.

## Part A: Wallet and HUD (~20 min)
1. `Assets/Scripts/Economy/IPurchaseProvider.cs` and `Wallet.cs`:
   ```csharp
   using UnityEngine;

   public interface IPurchaseProvider     // real store SDK sits behind
   { Awaitable<bool> PurchaseAsync(string productId); }

   public class Wallet : MonoBehaviour
   {
       public int Coins { get; private set; }
       public int Level { get; private set; }
       public event System.Action Changed;
       public int NextCost => (int)(100 * Mathf.Pow(1.5f, Level));

       public void Earn(int n) { Coins += n; Changed?.Invoke(); }
       public void Restore(int c, int l)
       { Coins = c; Level = l; Changed?.Invoke(); }

       public bool TryUpgrade()
       {
           if (Coins < NextCost) return false;
           Coins -= NextCost; Level++; Changed?.Invoke(); return true;
       }
   }
   ```
2. Put a `Wallet` on the persistent `App` object in `Boot` so it survives scene changes and is reachable from `Game` and `Result`.
3. HUD in `Game`: a TextMeshPro coin label and an **Upgrade** button whose label reads `Upgrade (150)`. A `WalletHud` component subscribes in `OnEnable` and unsubscribes in `OnDisable`:
   ```csharp
   void OnEnable()  { wallet.Changed += Refresh; Refresh(); }
   void OnDisable() { wallet.Changed -= Refresh; }
   void Refresh()
   {
       coinsLabel.SetText("{0}", wallet.Coins);            // no string concat
       upgradeLabel.SetText("Upgrade ({0})", wallet.NextCost);
       upgradeButton.interactable = wallet.Coins >= wallet.NextCost;
   }
   ```
   `SetText` with format arguments avoids per-call string garbage; `Refresh` runs only on change, never in `Update`.

*Expected result: the label shows 0, the button is greyed out.*

## Part B: Earn and upgrade in the loop (~20 min)
1. Call `wallet.Earn(amount)` from your core verb: enemy despawned by the player, distance milestone, item crafted, unit captured. Start with a small amount (5 to 10) and tune in Part E.
2. Make `TryUpgrade` change a real gameplay value. Pick one that players feel: move speed, damage, capacity, craft speed. Read `wallet.Level` where that value is used, or subscribe to `Changed` and cache it.
3. Wire the Upgrade button to `wallet.TryUpgrade()`. When it returns false the button should already be disabled by `Refresh`; keep the check anyway.
4. Play one attempt on the phone. Does the upgrade feel different? If not, the multiplier is too small; the economy loop only works when spending is noticeable.

*Expected result: coins go up on the core verb, the button lights up at 100, pressing it changes play and the next cost reads 150.*

## Part C: Stub the store (~20 min)
1. `FakePurchaseProvider.cs` and `CoinShop.cs` as on the deck:
   ```csharp
   using UnityEngine;

   public class FakePurchaseProvider : IPurchaseProvider
   {
       public async Awaitable<bool> PurchaseAsync(string productId)
       {
           Debug.Log($"[iap-stub] purchase {productId}");
           await Awaitable.WaitForSecondsAsync(0.5f);  // fake dialog
           return true;                // real SDK result goes here later
       }
   }

   public class CoinShop : MonoBehaviour
   {
       [SerializeField] Wallet wallet;
       IPurchaseProvider store = new FakePurchaseProvider();

       public async void OnBuyPack()   // Button handler: async void ok
       {
           if (await store.PurchaseAsync("coins_500")) wallet.Earn(500);
       }
   }
   ```
2. A **Buy 500 coins** button on the Menu or Result screen (not during play: a shop that interrupts the core loop is a dark pattern). Wire `OnClick` to `CoinShop.OnBuyPack`.
3. Confirm the log line:
   ```bash
   adb logcat -s Unity | grep iap-stub
   ```
4. Why this shape: gameplay code depends on `IPurchaseProvider`, never on a store. Swapping in Google Play Billing later means one new class and one line in `CoinShop`. Tests can inject a provider that returns false. This is the "stub IAP flow behind an interface" the module brief asks for.
5. `async void` is acceptable **only** for UI event handlers, because the Button cannot await. Everywhere else, return `Awaitable`.

## Part D: Persist the wallet (~15 min)
1. `WalletSave.cs`:
   ```csharp
   using UnityEngine;

   public static class WalletSave
   {
       const string CoinsKey = "wallet.coins", LevelKey = "wallet.level";

       public static void Save(Wallet w)
       {
           PlayerPrefs.SetInt(CoinsKey, w.Coins);
           PlayerPrefs.SetInt(LevelKey, w.Level);
           PlayerPrefs.Save();                     // flush to disk now
       }

       public static void Load(Wallet w) =>
           w.Restore(PlayerPrefs.GetInt(CoinsKey, 0), PlayerPrefs.GetInt(LevelKey, 0));
   }
   ```
2. On the `App` object, a `WalletPersistence` component: in `Awake`, `WalletSave.Load(wallet)`; subscribe `wallet.Changed += () => WalletSave.Save(wallet)`; and:
   ```csharp
   void OnApplicationPause(bool paused) { if (paused) WalletSave.Save(wallet); }
   ```
   `OnApplicationQuit` is not reliable on Android (the process is often killed without it); saving on change and on pause is what survives a force-stop. Lab B tests exactly this.
3. Earn some coins, kill the app from the recent-apps screen, relaunch. The coin count must be back.
4. Add the PlayerPrefs keys to `docs/CA1/privacy-statement.md` under "Data stored on the device" (you promised the statement would match the build).

## Part E: Pacing pass and journal (~15 min)
1. Reset the wallet (`PlayerPrefs.DeleteAll()` from a debug button or the editor menu **Edit > Clear All PlayerPrefs**), then play on the phone for three minutes with a stopwatch.
2. Note: time of the first upgrade, coins at three minutes, upgrades bought.
3. Target: first upgrade inside **2 minutes**, roughly the fifth by the end of a 10-minute session on the `100 * 1.5^level` curve (100, 150, 225, 337, 506). Tune the earn amount, not the curve, until the first target holds.
4. Journal: earn amount before and after, the numbers above, and one sentence on why the upgrade you chose is felt by the player. Lab B replaces the stopwatch with telemetry.
5. Commit.

## Troubleshooting
- **`Awaitable<bool>` cannot be used as a return type** -> the method must be `async`; check `using UnityEngine;`.
- **Button does nothing** -> `OnBuyPack` is not assigned in the Button's OnClick, or the `Wallet` reference is empty because the HUD is in `Game` and `Wallet` is in `Boot`; assign it at runtime with a static `Wallet.Instance` set in `Awake`.
- **Coins reset on relaunch** -> `Load` runs before `Restore` exists or after the HUD subscribed; make `WalletPersistence` execute before the HUD (Script Execution Order) and call `Refresh()` in `OnEnable`.
- **Upgrade button flickers or never enables** -> `Refresh` is subscribed twice; check that `OnDisable` unsubscribes.
- **`PlayerPrefs` values look stale after a force-stop** -> `PlayerPrefs.Save()` was never called; keep it in `WalletSave.Save`.

## Checkpoints to commit
1. `Assets/Scripts/Economy/IPurchaseProvider.cs`, `Wallet.cs`, `WalletHud.cs`.
2. `FakePurchaseProvider.cs`, `CoinShop.cs`, `WalletSave.cs`, `WalletPersistence.cs`.
3. `docs/CA1/privacy-statement.md` updated with the PlayerPrefs keys.
4. `docs/dev-journal.md`: pacing numbers before and after.
5. Tag `v0.4.0-economy` after Lab B (this lab commits without a tag).

## Exit ticket
Show an upgrade bought with earned coins on your phone, then the [iap-stub] log line from the fake provider.

## Reference links
- Unity 6.6: `Awaitable<T>`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Awaitable_1.html
- Unity 6.6: `Awaitable.WaitForSecondsAsync`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Awaitable.WaitForSecondsAsync.html
- Unity 6.6: `PlayerPrefs`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PlayerPrefs.html
- Unity 6.6: `MonoBehaviour.OnApplicationPause`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html
- Unity 6.6: TextMeshPro `SetText` (allocation-free label updates): https://docs.unity3d.com/Packages/com.unity.ugui@latest
- Google Play developer policy centre (Monetisation and Ads, including odds disclosure for randomised items; check the current text): https://play.google.com/about/developer-content-policy/
- Google Play Billing overview (awareness only, not used this semester): https://developer.android.com/google/play/billing
- Microsoft: async return types (`async void` only for event handlers): https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-return-types
