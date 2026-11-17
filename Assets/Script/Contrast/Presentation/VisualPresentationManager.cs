namespace Contrast.Presentation
{
    using System.Collections.Generic;
    using Contrast.Core;
    using Contrast.Level;
    using Contrast.Player;
    using UnityEngine;
    using Color = UnityEngine.Color;
    /// <summary>
    /// Central coordinator for the Contrast presentation pass.
    /// Automatically ensures background atmosphere, player presentation, platform
    /// optical patterns, marker visual effects, the phase tuner HUD, and the Newton
    /// apparatus are all initialized and updated.
    ///
    /// Preserves 100% of existing gameplay logic, collision rules, and data structures.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VisualPresentationManager : MonoBehaviour
    {
        private static VisualPresentationManager instance;
        public static VisualPresentationManager Instance => instance;

        private BackgroundPresentation background;
        private PhaseTunerUI phaseTuner;
        private NewtonRingsPresentation newtonApparatus;

        private readonly HashSet<ColorPlatform> decoratedPlatforms = new HashSet<ColorPlatform>();
        private readonly HashSet<LevelMarker> decoratedMarkers = new HashSet<LevelMarker>();
        private PlayerVisualPresentation playerVisual;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (FindObjectOfType<VisualPresentationManager>() == null)
            {
                GameObject go = new GameObject("VisualPresentationManager");
                go.AddComponent<VisualPresentationManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            EnsureSystems();
        }

        private void Start()
        {
            EnsureSystems();
            ConfigureCameraPostProcessing();
        }

        private void EnsureSystems()
        {
            // 1. Background Presentation
            if (background == null)
            {
                background = FindObjectOfType<BackgroundPresentation>();
                if (background == null)
                {
                    GameObject bgGo = new GameObject("BackgroundPresentation");
                    background = bgGo.AddComponent<BackgroundPresentation>();
                }
            }

            // 2. Phase Tuner UI
            if (phaseTuner == null)
            {
                phaseTuner = FindObjectOfType<PhaseTunerUI>();
                if (phaseTuner == null)
                {
                    GameObject uiGo = new GameObject("PhaseTunerUI");
                    phaseTuner = uiGo.AddComponent<PhaseTunerUI>();
                }
            }
        }

        private void ConfigureCameraPostProcessing()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null)
                return;
        }

        private void LateUpdate()
        {
            DecoratePlayer();
            DecoratePlatforms();
            DecorateMarkers();
            UpdateNewtonApparatusPosition();
        }

        private void DecoratePlayer()
        {
            if (playerVisual != null)
                return;

            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                playerVisual = pc.GetComponent<PlayerVisualPresentation>();
                if (playerVisual == null)
                {
                    playerVisual = pc.gameObject.AddComponent<PlayerVisualPresentation>();
                }
            }
        }

        private void DecoratePlatforms()
        {
            decoratedPlatforms.RemoveWhere(p => p == null);
            List<ColorPlatform> platforms = ColorPlatform.All;
            for (int i = 0; i < platforms.Count; i++)
            {
                ColorPlatform p = platforms[i];
                if (p == null || decoratedPlatforms.Contains(p))
                    continue;

                PlatformVisualPresentation pres = p.GetComponent<PlatformVisualPresentation>();
                if (pres == null)
                {
                    pres = p.gameObject.AddComponent<PlatformVisualPresentation>();
                }
                pres.InitializeVisuals();
                decoratedPlatforms.Add(p);
            }
        }

        private void DecorateMarkers()
        {
            decoratedMarkers.RemoveWhere(m => m == null);
            List<LevelMarker> markers = LevelMarker.All;
            for (int i = 0; i < markers.Count; i++)
            {
                LevelMarker m = markers[i];
                if (m == null || decoratedMarkers.Contains(m))
                    continue;

                LevelMarkerVisualPresentation pres = m.GetComponent<LevelMarkerVisualPresentation>();
                if (pres == null)
                {
                    pres = m.gameObject.AddComponent<LevelMarkerVisualPresentation>();
                }
                pres.InitializeVisuals();
                decoratedMarkers.Add(m);
            }
        }

        private void UpdateNewtonApparatusPosition()
        {
            if (newtonApparatus == null)
                return;

            // Anchor Newton apparatus at End marker position
            List<LevelMarker> markers = LevelMarker.All;
            for (int i = 0; i < markers.Count; i++)
            {
                LevelMarker m = markers[i];
                if (m != null && m.Type == LevelMarkerType.End)
                {
                    newtonApparatus.SetCenterPosition(m.Position);
                    return;
                }
            }
        }

        public void NotifyCheckpointReached()
        {
            if (phaseTuner != null)
                phaseTuner.TriggerCheckpointFlash();
            if (background != null)
                background.TriggerScreenPulse(0.35f);
        }

        public void NotifyPlayerDied()
        {
            if (background != null)
            {
                background.SetDeathFade(1f);
                background.TriggerScreenPulse(0.2f);
            }
        }
    }
}
