# Unity Slot Game Assignment

A modular 2D/UI slot machine prototype built in **Unity (C#)** featuring weighted random symbol selection, all-symbol-match payout evaluation, staggered reel animations, sound effects, a balance/win reset flow, and a runtime Win Chance testing slider.

---

## Table of Contents
1. [Game Overview & Gameplay Loop](#game-overview--gameplay-loop)
2. [How to Play & Run the WebGL Build](#how-to-play--run-the-webgl-build)
3. [Project Structure](#project-structure)
4. [Architecture & Key Components](#architecture--key-components)
5. [Bonus Features & Creative Additions](#bonus-features--creative-additions)
6. [Implementation Approach & Design Notes](#implementation-approach--design-notes)

---

## Game Overview & Gameplay Loop
- **Starting Balance & Bet Tiers**: Players begin with a configurable balance (`500` by default) and can switch between bet tiers (`10`, `50`, and `100`) while the machine is in the `Idle` state.
- **Spinning & Staggered Stops**: Clicking **SPIN** validates balance and state, deducts the current bet, triggers the spin start/loop audio, and scrolls the reels vertically with staggered stop timings (`baseSpinDuration` + per-reel `reelStopStagger`) followed by a smooth ease-out settle snap.
- **Win Rule & Payouts**: A win is awarded when all reels land on the same symbol on the active evaluation line. Payouts are calculated as `currentBet * multiplier`, supporting per-symbol multipliers, `PayoutConfig` overrides, and an optional bonus multiplier (`isBonusActive`).
- **Win Celebration & Reset**: Winning spins pulse the matching reel symbols, play the win audio stinger, and add the payout to the balance. Players can click **RESET** while idle to restore the starting balance and clear the last win readout.

---

## How to Play & Run the WebGL Build

### Option 1: Play the Prebuilt WebGL Build Locally
Because browsers restrict loading WebGL `.wasm` / data files over `file://` URLs, serve the WebGL build folder through a local static web server:

1. Locate your exported WebGL build directory (e.g., `Build/WebGL/` or `WebGL/`).
2. Start a local HTTP server from that build directory:
   - **Python**:
     ```bash
     cd Build/WebGL
     python -m http.server 8080
     ```
   - **Node.js (`http-server` or `npx serve`)**:
     ```bash
     cd Build/WebGL
     npx http-server -p 8080
     ```
3. Open `http://localhost:8080` in a modern desktop browser (Chrome, Edge, or Firefox).

### Option 2: Open & Build in the Unity Editor
1. Open the project in the Unity Editor.
2. Open the main slot machine scene and verify that `SlotGameManager`, `SlotUIManager`, and `SlotAudioManager` have their Inspector references assigned (symbols, reels, `PayoutConfig`, UI text/buttons/slider, and audio clips/sources).
3. Press **Play** to test in the Editor, or go to **File > Build Settings**, select **WebGL**, and click **Build** to export a fresh WebGL build.

---

## Project Structure

```text
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameState.cs
│   │   └── SlotGameManager.cs
│   ├── Data/
│   │   ├── PayoutConfig.cs
│   │   └── SymbolDefinition.cs
│   ├── Reels/
│   │   ├── ReelController.cs
│   │   ├── SlotEvaluator.cs
│   │   └── SlotRNG.cs
│   ├── UI/
│   │   └── SlotUIManager.cs
│   └── Audio/
│       ├── BtnClick.cs
│       └── SlotAudioManager.cs
├── Prefabs/
├── Animations/
├── UI/
│   └── Slot Machine/
└── Sounds/
```

---

## Architecture & Key Components

- **`GameState` (`Assets/Scripts/Core/GameState.cs`)**: Defines explicit lifecycle states (`Idle`, `Spinning`, `Evaluating`, `WinCelebration`) so input handling and state transitions stay predictable.
- **`SlotGameManager` (`Assets/Scripts/Core/SlotGameManager.cs`)**: Central gameplay coordinator managing balance, bet selection, spin eligibility (`CanSpin`), spin outcome generation, staggered reel coroutines, payout updates, and balance/win resets (`ResetBalanceAndWin`).
- **`SymbolDefinition` & `PayoutConfig` (`Assets/Scripts/Data/`)**: Data-driven `ScriptableObject` assets for defining symbol IDs, sprites, RNG weights, base all-match multipliers, per-symbol payout overrides, and bonus multipliers without hardcoding values into scripts.
- **`SlotRNG` (`Assets/Scripts/Reels/SlotRNG.cs`)**: Weighted random selector using cumulative weight rolls over valid `SymbolDefinition` entries (with optional seed support for deterministic testing).
- **`ReelController` (`Assets/Scripts/Reels/ReelController.cs`)**: Handles vertical symbol scrolling, wrap-around symbol recycling, cubic ease-out landing alignment, reel-stop audio triggers, and scale-pulse win highlights.
- **`SlotEvaluator` (`Assets/Scripts/Reels/SlotEvaluator.cs`)**: Pure evaluation helper (`EvaluateAllMatch`) that checks whether all landed symbols match and computes the payout multiplier.
- **`SlotUIManager` (`Assets/Scripts/UI/SlotUIManager.cs`)**: Updates `Balance`, `Bet`, `Win`, and `Status` TextMeshPro readouts, manages button/slider interactivity based on `GameState.Idle`, and drives the pull-lever visual state (`PinFull` / `PinHalf`).
- **`SlotAudioManager` & `BtnClick` (`Assets/Scripts/Audio/`)**: Centralizes one-shot SFX and looping reel-spin audio with delayed win/loss stingers (`0.4s` delay) and button click audio hooks.

---

## Bonus Features & Creative Additions

- **Runtime Win Chance Testing Slider**: Added a `Win Chance` UI slider (`Loss` to `Win`) with a live numeric readout (`SlotUIManager` updating `SlotGameManager.winChance`), making it easy to verify win celebrations, payouts, and loss states quickly during testing.
- **Data-Driven `PayoutConfig` with Bonus Multiplier & Overrides**: Supports configurable `bonusMultiplier`, `defaultAllMatchMultiplier`, and `SymbolPayoutOverride` rules via `ScriptableObject` assets.
- **Lever Pull Animation Feedback (`PinFull` / `PinHalf`)**: Toggles between full and pulled lever sprites briefly when a spin starts (`PinRoutine` in `SlotUIManager`) for tactile slot-machine feedback.
- **Balance & Win Reset Button**: Allows instant reset of balance, last win, and default bet tier once a spin cycle returns to `Idle`.

---

## Implementation Approach & Design Notes

- **Separation of Concerns**: Kept data definitions (`ScriptableObject`s), probability/evaluation logic (`SlotRNG`, `SlotEvaluator`), reel presentation (`ReelController`), UI binding (`SlotUIManager`), audio (`SlotAudioManager`), and game loop orchestration (`SlotGameManager`) separated across dedicated folders under `Assets/Scripts/`.
- **Controllable Win Ratio vs. Per-Reel Independence**: `GenerateSpinOutcome` rolls against `winChance` first—on a win, a weighted symbol is selected and applied across all reels so all-match wins occur at the configured rate; on a loss, weighted symbols are rolled per reel and adjusted if they happen to accidentally match all slots.
- **UI & Audio Safeguards**: Button and bet controls are locked while spinning/evaluating so players cannot place conflicting bets mid-spin, and `SlotAudioManager` checks for null `AudioSource` and `AudioClip` references before playing audio.
