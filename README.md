# HapticsSample

A small Unity/C# haptics system sample created from production mobile game code.

This repository demonstrates how I structured haptic feedback for a mobile game using a centralized manager, event listener, request queue, deduplication window, and reusable gameplay-specific haptic helper methods.

## Purpose

This sample is meant to show code organization and system design for mobile haptics in Unity. It is not a full Unity project. Instead, it includes the core scripts used to manage and trigger haptic feedback across gameplay, UI, slot-style interactions, match events, bonus events, and animation-driven moments.

The goal of the system is to make haptics easy to call from gameplay code while keeping the actual device feedback logic centralized and controlled.

## What This Demonstrates

- Centralized haptic management through a persistent `HapticManager`
- Event-based communication between gameplay systems and the haptic playback layer
- Device support checks before attempting to play haptics
- Player preference support for enabling/disabling haptics
- Request batching through `HapticRequestManager`
- Duplicate haptic prevention using a configurable deduplication window
- Support for multiple haptic request types:
  - Preset haptics
  - Constant haptics
  - Emphasis haptics
  - Haptic clips
  - Multi-step haptic patterns
- Gameplay-specific convenience methods through `GameHaptics`
- Pattern-based haptics for events such as matches, bombs, rockets, dice rolls, slot reels, card flips, bonus reels, and build animations

## File Overview

### `HapticManager.cs`

The central singleton responsible for managing haptic state, player preferences, and request processing.

Key responsibilities:

- Stores whether haptics are enabled
- Stores whether the current device supports haptics
- Persists haptic settings with `PlayerPrefs`
- Receives haptic requests from gameplay systems
- Sends valid requests to the active listener
- Uses `HapticRequestManager` to queue and deduplicate requests

### `HapticListener.cs`

The playback layer that listens for haptic requests and sends them to the device through the haptics plugin.

Key responsibilities:

- Checks device haptic support
- Subscribes to `HapticManager` events
- Plays preset, constant, emphasis, clip, and pattern haptics
- Handles delayed multi-step haptic patterns through coroutines

### `HapticRequestManager.cs`

A lightweight request queue used to avoid excessive or duplicate haptic calls.

Key responsibilities:

- Queues pending haptic requests
- Prevents duplicate requests from being added repeatedly
- Uses a deduplication time window to avoid haptic spam
- Processes requests in a controlled update loop

### `GameHaptics.cs`

A gameplay-facing helper class that exposes simple, readable haptic methods.

Example calls:

```csharp
GameHaptics.Match();
GameHaptics.Bomb();
GameHaptics.Rocket();
GameHaptics.ColorBomb(selectedTileCount);
GameHaptics.CardFlip();
GameHaptics.BuildingComplete();
