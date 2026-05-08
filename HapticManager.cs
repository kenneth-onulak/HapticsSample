// Copyright (C) 2024 Global Outlier Gaming. All rights reserved.
// Author: Kenneth Onulak Jr.

using UnityEngine;
using Lofelt.NiceVibrations;

namespace _GO.Scripts.Core.UI
{
    [DefaultExecutionOrder(-2)] // Ensure this script is executed before other scripts that access this
    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance;

        // Delegate with parameters for haptic request.
        public delegate void HapticEvent(HapticRequest request);
        public event HapticEvent OnHapticFeedback;

        /// <remarks>
        /// These keys are used internally by the HapticManager.
        /// Do not access directly!
        /// Instead, access HapticsEnabled and HapticsCanPlay which manage the PlayerPref for you.
        /// </remarks>
        private const string HapticsEnabledKey = "haptics_enabled";
        private const string HapticsCanPlayKey = "haptics_can_play";
        
        private bool _hapticsEnabled = true;
        private bool _hapticsCanPlay;
        
        // Haptic request manager for batching and deduplicating requests
        private HapticRequestManager _hapticRequestManager = new HapticRequestManager();
        
        [Tooltip("Time window in seconds to prevent duplicate haptic feedback of the same type")]
        [SerializeField] private float _deduplicationWindow = 0.1f;

        /// <summary>
        /// Determines whether haptics are enabled or disabled.
        /// </summary>
        /// <remarks>
        /// This property allows you to enable or disable haptics.
        /// When haptics are enabled, the HapticManager will trigger haptic feedback based on the HapticType.
        /// When haptics are disabled, the HapticManager will not trigger any haptic feedback.
        /// </remarks>
        public bool HapticsEnabled
        {
            get => _hapticsEnabled;
            set
            {
                _hapticsEnabled = value;
                SetEnabled();            
            }
        }

        /// <summary>
        /// Determines whether haptics can be played.
        /// </summary>
        /// <remarks>
        /// This property allows you to check whether haptics can be played on the device.
        /// When haptics can be played, the HapticManager will trigger haptic feedback based on the HapticType.
        /// When haptics cannot be played, the HapticManager will not trigger any haptic feedback.
        /// </remarks>
        public bool HapticsCanPlay
        {
            get => _hapticsCanPlay;
            set
            {
                _hapticsCanPlay = value;
                SetCanPlay();
            }
        }
        
        /// <summary>
        /// The Awake method is called when the script instance is being loaded.
        /// It is used to initialize the haptic manager and set up the necessary variables and preferences.
        /// </summary>
        void Awake()
        {
            // Singleton pattern to ensure only one instance exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // Load haptic preference if saved
                GetPlayerPrefs();
                
                // Initialize the deduplication window
                _hapticRequestManager.SetDeduplicationWindow(_deduplicationWindow);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Process haptic requests in Update to ensure they're handled efficiently.
        /// </summary>
        void Update()
        {
            // Process any pending haptic requests
            _hapticRequestManager.ProcessPendingRequests(OnHapticFeedback.Invoke);
        }

        /// <summary>
        /// Toggles the haptics on or off.
        /// </summary>
        public void ToggleHaptics()
        { 
            _hapticsEnabled = !_hapticsEnabled;
            SetEnabled(); ;
        }

        /// <summary>
        /// Triggers haptic feedback of the specified type.
        /// </summary>
        /// <param name="request">The type of haptic feedback to trigger.</param>
        public void TriggerHapticFeedback(HapticRequest request)
        {
            // Skip if haptics are disabled
            if (!_hapticsCanPlay || !_hapticsEnabled || OnHapticFeedback == null)
            {
                return;
            }
            
            // Use the HapticRequestManager to handle the request
            _hapticRequestManager.RequestHapticFeedback(request);
        }
        
        /// <summary>
        /// Sets the deduplication window in seconds.
        /// </summary>
        /// <param name="seconds">The time window in seconds.</param>
        public void SetDeduplicationWindow(float seconds)
        {
            _deduplicationWindow = Mathf.Max(0.01f, seconds);
            _hapticRequestManager.SetDeduplicationWindow(_deduplicationWindow);
        }

        /// <summary>
        /// Gets or sets the PlayerPref value indicating whether haptics are enabled or disabled.
        /// </summary>
        private bool PlayerPrefsEnabled
        {
            get => PlayerPrefs.GetInt(HapticsEnabledKey, 1) == 1;
            set => PlayerPrefs.SetInt(HapticsEnabledKey, value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets the PlayerPref value indicating whether the device can receive haptic feedback.
        /// </summary>
        /// <remarks>
        private bool PlayerPrefsCanPlay
        {
            get => PlayerPrefs.GetInt(HapticsCanPlayKey, 1) == 1;
            set => PlayerPrefs.SetInt(HapticsCanPlayKey, value ? 1 : 0);
        }

        /// <summary>
        /// Sets the haptics to the specified enabled state.
        /// </summary>
        private void SetEnabled()
        {
            PlayerPrefsEnabled = _hapticsEnabled;
            Debug.Log("Haptics Enabled: " + _hapticsEnabled);
        }

        /// <summary>
        /// Sets the device's haptics capability for playing haptic feedback.
        /// </summary>
        private void SetCanPlay()
        {
            PlayerPrefsCanPlay = _hapticsCanPlay;
            Debug.Log("Device Supports Haptics: " + _hapticsCanPlay);
        }

        /// <summary>
        /// Retrieves the haptic preferences from PlayerPrefs and sets the corresponding variables.
        /// </summary>
        private void GetPlayerPrefs()
        {
            _hapticsEnabled = PlayerPrefsEnabled;
            _hapticsCanPlay = PlayerPrefsCanPlay;
        }

        // Overloads for convenience
        public void TriggerHapticFeedback(HapticPatterns.PresetType preset, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.None)
            => TriggerHapticFeedback(HapticRequest.PresetRequest(preset, fallback));
        public void TriggerHapticFeedback(float amplitude, float frequency, float duration, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.HeavyImpact)
            => TriggerHapticFeedback(HapticRequest.ConstantRequest(amplitude, frequency, duration, fallback));
        public void TriggerHapticFeedback(float amplitude, float frequency, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.MediumImpact)
            => TriggerHapticFeedback(HapticRequest.EmphasisRequest(amplitude, frequency, fallback));
        public void TriggerHapticFeedback(HapticClip clip, HapticPatterns.PresetType fallback = HapticPatterns.PresetType.MediumImpact)
            => TriggerHapticFeedback(HapticRequest.ClipRequest(clip, fallback));
        public void TriggerHapticFeedback(HapticRequest.HapticStep[] steps)
            => TriggerHapticFeedback(HapticRequest.PatternRequest(steps));
    }
}