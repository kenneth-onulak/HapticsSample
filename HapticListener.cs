// Copyright (C) 2024 Global Outlier Gaming. All rights reserved.
// Author: Kenneth Onulak Jr.

using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEngine.Assertions;

namespace _GO.Scripts.Core.UI
{
    [DefaultExecutionOrder(-1)] // Ensure this script is executed after HapticManager
    public class HapticListener : MonoBehaviour
    {
        void Awake()
        {
            // Check if the device supports haptics
            if (DeviceCapabilities.isVersionSupported)
            {
                HapticManager.Instance.HapticsCanPlay = true;
            }
            else
            {
                HapticManager.Instance.HapticsEnabled = false;
                HapticManager.Instance.HapticsCanPlay = false;
            }
        }

        void OnEnable()
        {
            HapticManager.Instance.OnHapticFeedback += PlayHapticFeedback;
        }

        void OnDisable()
        {
            HapticManager.Instance.OnHapticFeedback -= PlayHapticFeedback;
        }

        // Main event handler for HapticManager events (now using HapticRequest)
        void PlayHapticFeedback(HapticRequest request)
        {
            switch (request.Type)
            {
                case HapticRequest.RequestType.Preset:
                    RequestHapticPresetFeedback(request.Preset, request.FallbackPreset);
                    break;
                case HapticRequest.RequestType.Constant:
                    RequestHapticConstantFeedback(request.Amplitude, request.Frequency, request.Duration, request.FallbackPreset);
                    break;
                case HapticRequest.RequestType.Emphasis:
                    RequestHapticEmphasisFeedback(request.Amplitude, request.Frequency, request.FallbackPreset);
                    break;
                case HapticRequest.RequestType.Clip:
                    RequestHapticClipFeedback(request.Clip, request.FallbackPreset);
                    break;
                case HapticRequest.RequestType.Pattern:
                    // Start a coroutine to play the sequence with delays
                    StopCoroutine("PlayPatternCoroutine");
                    StartCoroutine(PlayPatternCoroutine(request));
                    break;
            }
        }

        System.Collections.IEnumerator PlayPatternCoroutine(HapticRequest request)
        {
            if (request.Steps == null)
            {
                yield break;
            }

            // Ensure controller is stopped before sequence
            HapticController.Stop();

            foreach (var step in request.Steps)
            {
                if (step.Delay > 0f)
                {
                    yield return new WaitForSecondsRealtime(step.Delay);
                }

                switch (step.Type)
                {
                    case HapticRequest.RequestType.Preset:
                        RequestHapticPresetFeedback(step.Preset, step.FallbackPreset);
                        break;
                    case HapticRequest.RequestType.Constant:
                        RequestHapticConstantFeedback(step.Amplitude, step.Frequency, step.Duration, step.FallbackPreset);
                        break;
                    case HapticRequest.RequestType.Emphasis:
                        RequestHapticEmphasisFeedback(step.Amplitude, step.Frequency, step.FallbackPreset);
                        break;
                    case HapticRequest.RequestType.Clip:
                        RequestHapticClipFeedback(step.Clip, step.FallbackPreset);
                        break;
                }
            }
        }

        // --- API: RequestHapticFeedback methods ---

        /// <summary>
        /// Triggers a haptic preset pattern.
        /// </summary>
        public void RequestHapticPresetFeedback(HapticPatterns.PresetType presetType, HapticPatterns.PresetType fallbackPreset = HapticPatterns.PresetType.None)
        {
            HapticController.fallbackPreset = fallbackPreset;
            HapticPatterns.PlayPreset(presetType);
        }

        /// <summary>
        /// Triggers a constant haptic pattern.
        /// </summary>
        /// <param name="amplitude">Amplitude (0.0 to 1.0)</param>
        /// <param name="frequency">Frequency (0.0 to 1.0)</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="fallbackPreset">Fallback preset if not supported</param>
        public void RequestHapticConstantFeedback(float amplitude, float frequency, float duration, HapticPatterns.PresetType fallbackPreset = HapticPatterns.PresetType.HeavyImpact)
        {
            HapticController.fallbackPreset = fallbackPreset;
            HapticPatterns.PlayConstant(amplitude, frequency, duration);
        }

        /// <summary>
        /// Triggers an emphasis haptic pattern.
        /// </summary>
        /// <param name="amplitude">Amplitude (0.0 to 1.0)</param>
        /// <param name="frequency">Frequency (0.0 to 1.0)</param>
        /// <param name="fallbackPreset">Fallback preset if not supported</param>
        public void RequestHapticEmphasisFeedback(float amplitude, float frequency, HapticPatterns.PresetType fallbackPreset = HapticPatterns.PresetType.MediumImpact)
        {
            HapticController.fallbackPreset = fallbackPreset;
            HapticPatterns.PlayEmphasis(amplitude, frequency);
        }

        /// <summary>
        /// Triggers a haptic clip using HapticController.
        /// </summary>
        /// <param name="clip">The HapticClip to play</param>
        /// <param name="fallbackPreset">Fallback preset if not supported</param>
        public void RequestHapticClipFeedback(HapticClip clip, HapticPatterns.PresetType fallbackPreset = HapticPatterns.PresetType.MediumImpact)
        {
            HapticController.fallbackPreset = fallbackPreset;
            HapticController.Play(clip);
        }
    }
}