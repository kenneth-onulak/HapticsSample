using System;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

namespace _GO.Scripts.Core.UI
{
    /// <summary>
    /// Represents a haptic feedback request of any type.
    /// </summary>
    public struct HapticRequest : IEquatable<HapticRequest>
    {
        public enum RequestType { Preset, Constant, Emphasis, Clip, Pattern }

        /// <summary>
        /// A single step in a haptic pattern sequence.
        /// </summary>
        public struct HapticStep : IEquatable<HapticStep>
        {
            public RequestType Type;
            public HapticPatterns.PresetType Preset;
            public float Amplitude;
            public float Frequency;
            public float Duration;
            public HapticClip Clip;
            public HapticPatterns.PresetType FallbackPreset;
            public float Delay; // Delay (in seconds, realtime) before this step plays

            public bool Equals(HapticStep other)
            {
                if (Type != other.Type) return false;
                if (!Mathf.Approximately(Delay, other.Delay)) return false;
                switch (Type)
                {
                    case RequestType.Preset:
                        return Preset == other.Preset && FallbackPreset == other.FallbackPreset;
                    case RequestType.Constant:
                        return Mathf.Approximately(Amplitude, other.Amplitude)
                               && Mathf.Approximately(Frequency, other.Frequency)
                               && Mathf.Approximately(Duration, other.Duration)
                               && FallbackPreset == other.FallbackPreset;
                    case RequestType.Emphasis:
                        return Mathf.Approximately(Amplitude, other.Amplitude)
                               && Mathf.Approximately(Frequency, other.Frequency)
                               && FallbackPreset == other.FallbackPreset;
                    case RequestType.Clip:
                        return Clip == other.Clip && FallbackPreset == other.FallbackPreset;
                    default:
                        return false;
                }
            }

            public override bool Equals(object obj) => obj is HapticStep other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = (int)Type;
                    hash = hash * 31 + Preset.GetHashCode();
                    hash = hash * 31 + Amplitude.GetHashCode();
                    hash = hash * 31 + Frequency.GetHashCode();
                    hash = hash * 31 + Duration.GetHashCode();
                    hash = hash * 31 + (Clip != null ? Clip.GetHashCode() : 0);
                    hash = hash * 31 + FallbackPreset.GetHashCode();
                    hash = hash * 31 + Delay.GetHashCode();
                    return hash;
                }
            }
        }
        public RequestType Type;
        public HapticPatterns.PresetType Preset;
        public float Amplitude;
        public float Frequency;
        public float Duration;
        public HapticClip Clip;
        public HapticPatterns.PresetType FallbackPreset;
        public HapticStep[] Steps; // Used when Type == Pattern

        // Factory methods for convenience
        public static HapticRequest PresetRequest(HapticPatterns.PresetType preset, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.None)
            => new HapticRequest { Type = RequestType.Preset, Preset = preset, FallbackPreset = fallback };
        public static HapticRequest ConstantRequest(float amplitude, float frequency, float duration, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.HeavyImpact)
            => new HapticRequest { Type = RequestType.Constant, Amplitude = amplitude, Frequency = frequency, Duration = duration, FallbackPreset = fallback };
        public static HapticRequest EmphasisRequest(float amplitude, float frequency, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.MediumImpact)
            => new HapticRequest { Type = RequestType.Emphasis, Amplitude = amplitude, Frequency = frequency, FallbackPreset = fallback };
        public static HapticRequest ClipRequest(HapticClip clip, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.MediumImpact)
            => new HapticRequest { Type = RequestType.Clip, Clip = clip, FallbackPreset = fallback };
        public static HapticRequest PatternRequest(HapticStep[] steps)
            => new HapticRequest { Type = RequestType.Pattern, Steps = steps };

        public bool Equals(HapticRequest other)
        {
            if (Type != other.Type) return false;
            switch (Type)
            {
                case RequestType.Preset:
                    return Preset == other.Preset && FallbackPreset == other.FallbackPreset;
                case RequestType.Constant:
                    return Mathf.Approximately(Amplitude, other.Amplitude) && Mathf.Approximately(Frequency, other.Frequency) && Mathf.Approximately(Duration, other.Duration) && FallbackPreset == other.FallbackPreset;
                case RequestType.Emphasis:
                    return Mathf.Approximately(Amplitude, other.Amplitude) && Mathf.Approximately(Frequency, other.Frequency) && FallbackPreset == other.FallbackPreset;
                case RequestType.Clip:
                    return Clip == other.Clip && FallbackPreset == other.FallbackPreset;
                case RequestType.Pattern:
                    if (Steps == null && other.Steps == null) return true;
                    if (Steps == null || other.Steps == null) return false;
                    if (Steps.Length != other.Steps.Length) return false;
                    for (int i = 0; i < Steps.Length; i++)
                    {
                        if (!Steps[i].Equals(other.Steps[i])) return false;
                    }
                    return true;
                default:
                    return false;
            }
        }
        public override bool Equals(object obj) => obj is HapticRequest other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Type;
                hash = hash * 31 + Preset.GetHashCode();
                hash = hash * 31 + Amplitude.GetHashCode();
                hash = hash * 31 + Frequency.GetHashCode();
                hash = hash * 31 + Duration.GetHashCode();
                hash = hash * 31 + (Clip != null ? Clip.GetHashCode() : 0);
                hash = hash * 31 + FallbackPreset.GetHashCode();
                if (Type == RequestType.Pattern && Steps != null)
                {
                    for (int i = 0; i < Steps.Length; i++)
                    {
                        hash = hash * 31 + Steps[i].GetHashCode();
                    }
                }
                return hash;
            }
        }
    }

    /// <summary>
    /// Manages and optimizes haptic feedback requests by batching and deduplicating them.
    /// </summary>
    public class HapticRequestManager
    {
        // Dictionary to track the last time each haptic request was triggered
        private Dictionary<HapticRequest, float> _lastTriggerTimes = new Dictionary<HapticRequest, float>();
        // Time window in seconds to prevent duplicate haptic feedback of the same type
        private float _deduplicationWindow = 0.1f;
        // Queue of pending haptic requests
        private Queue<HapticRequest> _pendingRequests = new Queue<HapticRequest>();
        // Set to track which haptic requests are already in the queue
        private HashSet<HapticRequest> _queuedRequests = new HashSet<HapticRequest>();

        /// <summary>
        /// Requests haptic feedback of any type.
        /// </summary>
        public void RequestHapticFeedback(HapticRequest request)
        {
            if (_queuedRequests.Contains(request))
            {
                return;
            }
            _pendingRequests.Enqueue(request);
            _queuedRequests.Add(request);
        }

        /// <summary>
        /// Processes pending haptic requests and triggers them if appropriate.
        /// </summary>
        /// <param name="onHapticFeedback">The callback to invoke when a haptic should be triggered.</param>
        public void ProcessPendingRequests(System.Action<HapticRequest> onHapticFeedback)
        {
            while (_pendingRequests.Count > 0)
            {
                HapticRequest request = _pendingRequests.Dequeue();
                _queuedRequests.Remove(request);
                if (ShouldTriggerHaptic(request))
                {
                    _lastTriggerTimes[request] = Time.unscaledTime;
                    onHapticFeedback?.Invoke(request);
                }
            }
        }

        /// <summary>
        /// Determines if a haptic request should be triggered based on the deduplication window.
        /// </summary>
        private bool ShouldTriggerHaptic(HapticRequest request)
        {
            if (!_lastTriggerTimes.ContainsKey(request))
            {
                return true;
            }
            float timeSinceLastTrigger = Time.unscaledTime - _lastTriggerTimes[request];
            return timeSinceLastTrigger >= _deduplicationWindow;
        }

        /// <summary>
        /// Clears all pending haptic requests.
        /// </summary>
        public void ClearPendingRequests()
        {
            _pendingRequests.Clear();
            _queuedRequests.Clear();
        }

        /// <summary>
        /// Sets the deduplication window in seconds.
        /// </summary>
        public void SetDeduplicationWindow(float seconds)
        {
            _deduplicationWindow = Mathf.Max(0.01f, seconds);
        }
    }
} 