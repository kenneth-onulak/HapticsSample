// Copyright (C) 2024 Global Outlier Gaming. All rights reserved.
// Author: Kenneth Onulak Jr.

using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;
using _GO.Scripts.Core.UI;

namespace _GO.Scripts.Game.Haptics
{
    /// <summary>
    /// Static helper for game-specific haptic patterns.
    /// Keeps convenience methods separate from the core haptic system.
    /// </summary>
    public static class GameHaptics
    {
        private const float FastInterval = 0.05f;
        private const float NormalInterval = 0.08f;
        private const float SlotInterval = 0.03f;

        // --- Simple events ---

        /// <summary>
        /// Match - 1 light vibration
        /// </summary>
        public static void Match()
        {
            PlayPreset(HapticPatterns.PresetType.LightImpact);
        }

        /// <summary>
        /// Incorrect match - 2 light vibrations
        /// </summary>
        public static void IncorrectMatch()
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval)
            };
            PlayPattern(steps);
        }

        /// <summary>
        /// Bomb - 1 light and 1 medium fast vibrations
        /// </summary>
        public static void Bomb()
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.MediumImpact, FastInterval)
            };
            PlayPattern(steps);
        }

        /// <summary>
        /// Rocket - 3 fast light vibrations
        /// </summary>
        public static void Rocket()
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval),
                StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval)
            };
            PlayPattern(steps);
        }

        /// <summary>
        /// Color Bomb - 1 light vibration for each selected tile
        /// </summary>
        public static void ColorBomb(int selectedTileCount, float perTapInterval = NormalInterval)
        {
            if (selectedTileCount <= 0) return;
            var steps = new List<HapticRequest.HapticStep>(selectedTileCount);
            for (int i = 0; i < selectedTileCount; i++)
            {
                steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, i == 0 ? 0f : Mathf.Max(0.01f, perTapInterval)));
            }
            PlayPattern(steps);
        }

        /// <summary>
        /// Color Bomb + Color Bomb - sequence of 10-15 light fast vibrations for the duration of the animation
        /// </summary>
        public static void ColorBombCombo(float durationSeconds, int taps = 12)
        {
            taps = Mathf.Clamp(taps, 10, 15);
            durationSeconds = Mathf.Max(0.05f, durationSeconds);

            var steps = new List<HapticRequest.HapticStep>(taps);
            float interval = Mathf.Max(0.03f, durationSeconds / taps);
            for (int i = 0; i < taps; i++)
            {
                steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, i == 0 ? 0f : interval));
            }
            PlayPattern(steps);
        }

        /// <summary>
        /// Dice - 2 fast light vibrations and 1 light vibration when the die lands
        /// Provide landing delay to schedule the final tap.
        /// </summary>
        public static void Dice(float landingDelaySeconds)
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval)
            };
            float thirdDelay = Mathf.Max(0f, landingDelaySeconds - FastInterval);
            steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, thirdDelay));
            PlayPattern(steps);
        }

        /// <summary>
        /// Bomb + Rocket - 1 light vibration and 3 fast medium vibrations when the rockets are launched
        /// Provide delay to rocket launch from the start of the combo.
        /// </summary>
        public static void BombRocket(float rocketsLaunchDelaySeconds)
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.MediumImpact, Mathf.Max(0f, rocketsLaunchDelaySeconds)),
                StepPreset(HapticPatterns.PresetType.MediumImpact, FastInterval),
                StepPreset(HapticPatterns.PresetType.MediumImpact, FastInterval)
            };
            PlayPattern(steps);
        }

        /// <summary>
        /// Bomb + Bomb - light fast vibrations while the animation is playing and then 1 medium vibration on explosion
        /// Provide duration of the pre-explosion animation.
        /// </summary>
        public static void BombBomb(float durationSeconds)
        {
            durationSeconds = Mathf.Max(FastInterval, durationSeconds);
            int taps = Mathf.Max(1, Mathf.FloorToInt(durationSeconds / FastInterval));

            var steps = new List<HapticRequest.HapticStep>(taps + 1);
            for (int i = 0; i < taps; i++)
            {
                steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, i == 0 ? 0f : FastInterval));
            }
            float used = (taps - 1) * FastInterval;
            float finalDelay = Mathf.Max(0f, durationSeconds - used);
            steps.Add(StepPreset(HapticPatterns.PresetType.MediumImpact, finalDelay));
            PlayPattern(steps);
        }

        /// <summary>
        /// Dice + Bomb - 2 fast light vibrations and 1 light and 1 medium vibration on landing
        /// </summary>
        public static void DiceBomb(float landingDelaySeconds)
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval)
            };
            float toLanding = Mathf.Max(0f, landingDelaySeconds - FastInterval);
            steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, toLanding));
            steps.Add(StepPreset(HapticPatterns.PresetType.MediumImpact, FastInterval));
            PlayPattern(steps);
        }

        /// <summary>
        /// Dice + Rockets - 2 fast light vibrations and 3 fast light vibrations on landing
        /// </summary>
        public static void DiceRockets(float landingDelaySeconds)
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepPreset(HapticPatterns.PresetType.LightImpact, 0f),
                StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval)
            };
            float toLanding = Mathf.Max(0f, landingDelaySeconds - FastInterval);
            steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, toLanding));
            steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval));
            steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, FastInterval));
            PlayPattern(steps);
        }

        /// <summary>
        /// Slot reel start spin - 1 light vibration (per slot)
        /// </summary>
        public static void SlotReelStart(int slotCount)
        {
            PlayPerSlot(slotCount);
        }

        /// <summary>
        /// Slot reel end spin - 1 light vibration (per slot)
        /// </summary>
        public static void SlotReelEnd(int slotCount)
        {
            PlayPerSlot(slotCount);
        }

        /// <summary>
        /// Slot reel show payout - 1 medium vibration
        /// </summary>
        public static void SlotReelPayout()
        {
            PlayPreset(HapticPatterns.PresetType.MediumImpact);
        }

        /// <summary>
        /// Building animation - repeated light taps over a duration at a fixed interval.
        /// Replaces coroutine-based haptic loops.
        /// </summary>
        public static void BuildingAnimation(float durationSeconds, float intervalSeconds)
        {
            durationSeconds = Mathf.Max(0.01f, durationSeconds);
            intervalSeconds = Mathf.Max(0.01f, intervalSeconds);

            int taps = Mathf.Max(1, Mathf.FloorToInt(durationSeconds / intervalSeconds));
            var steps = new List<HapticRequest.HapticStep>(taps);
            for (int i = 0; i < taps; i++)
            {
                steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, i == 0 ? 0f : intervalSeconds));
            }
            PlayPattern(steps);
        }

        /// <summary>
        /// Building complete - success
        /// </summary>
        public static void BuildingComplete()
        {
            PlayPreset(HapticPatterns.PresetType.Success);
        }

        /// <summary>
        /// Wild tile spawning haptic.
        /// </summary>
        public static void WildTileSpawn()
        {
            PlayPreset(HapticPatterns.PresetType.LightImpact);
        }

        /// <summary>
        /// Collecting a gold bag in heist mini-game.
        /// </summary>
        public static void HeistGoldBag()
        {
            PlayPreset(HapticPatterns.PresetType.LightImpact);
        }

        /// <summary>
        /// Colum stopped during free spins.
        /// </summary>
        public static void FreeSpinsColumn()
        {
            PlayPreset(HapticPatterns.PresetType.LightImpact);
        }

        /// <summary>
        /// Triple flip card dealing haptic.
        /// </summary>
        public static void TripleFlipDealCard()
        {
            PlayPreset(HapticPatterns.PresetType.SoftImpact);
        }

        /// <summary>
        /// Triple flip card flip haptic
        /// </summary>
        /// <param name="slotCount"></param>
        public static void CardFlip()
        {
            PlayPreset(HapticPatterns.PresetType.Selection);
        }

        /// <summary>
        /// Play a basic success haptic.
        /// </summary>
        public static void Success()
        {
            PlayPreset(HapticPatterns.PresetType.Success);
        }

        /// <summary>
        /// Trigger when a tile explodes.
        /// </summary>
        public static void TileExplode()
        {
            PlayPreset(HapticPatterns.PresetType.SoftImpact);
        }

        /// <summary>
        /// Trigger after input or full auto in the gold win popup script.
        /// </summary>
        public static void GoldWinPopup()
        {
            PlayPreset(HapticPatterns.PresetType.Selection);
        }

        /// <summary>
        /// Used in the highlight helper when auto matching.
        /// </summary>
        public static void HighlightHelper()
        {
            PlayPreset(HapticPatterns.PresetType.RigidImpact);
        }

        /// <summary>
        /// Fun haptics for when you get a mini-game event on slots.
        /// </summary>
        public static void BonusReel(float delay = 0.5f, float duration = 0.5f)
        {
            var steps = new List<HapticRequest.HapticStep>
            {
                StepContinuous(0.5f, 0.5f, duration, 0f, HapticPatterns.PresetType.HeavyImpact),
                StepContinuous(0.5f, 0.5f, duration, delay, HapticPatterns.PresetType.HeavyImpact),
                StepContinuous(0.5f, 0.5f, duration, delay, HapticPatterns.PresetType.HeavyImpact)
            };
            PlayPattern(steps);
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // --- Internal helpers ---
        ////////////////////////////////////////////////////////////////////////////////////////////////

        private static void PlayPerSlot(int slotCount)
        {
            slotCount = Mathf.Max(0, slotCount);
            if (slotCount == 0) return;
            var steps = new List<HapticRequest.HapticStep>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                steps.Add(StepPreset(HapticPatterns.PresetType.LightImpact, i == 0 ? 0f : SlotInterval));
            }
            PlayPattern(steps);
        }

        private static HapticRequest.HapticStep StepPreset(HapticPatterns.PresetType preset, float delay)
        {
            return new HapticRequest.HapticStep
            {
                Type = HapticRequest.RequestType.Preset,
                Preset = preset,
                FallbackPreset = preset,
                Delay = Mathf.Max(0f, delay)
            };
        }

        private static HapticRequest.HapticStep StepContinuous(float amplitude, float frequency, float duration, float delay, HapticPatterns.PresetType fallback)
        {
            return new HapticRequest.HapticStep
            {
                Type = HapticRequest.RequestType.Constant,
                Amplitude = amplitude,
                Frequency = frequency,
                Duration = duration,
                FallbackPreset = fallback,
                Delay = Mathf.Max(0f, delay)
            };
        }

        private static void PlayPreset(HapticPatterns.PresetType preset)
        {
            if (HapticManager.Instance == null) return;
            HapticManager.Instance.TriggerHapticFeedback(preset, preset);
        }

        private static void PlayPattern(List<HapticRequest.HapticStep> steps)
        {
            if (HapticManager.Instance == null) return;
            if (steps == null || steps.Count == 0)
            {
                return;
            }
            var request = HapticRequest.PatternRequest(steps.ToArray());
            HapticManager.Instance.TriggerHapticFeedback(request);
        }
    }
}


