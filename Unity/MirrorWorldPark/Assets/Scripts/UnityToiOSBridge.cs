using System.Collections;
using UnityEngine;

namespace MirrorWorld
{
    public class UnityToiOSBridge : MonoBehaviour
    {
        public static UnityToiOSBridge Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Initialize Unity components
            InitializeManagers();
            
            // Notify iOS that Unity is ready
            SendMessageToiOS("UnityReady", "");
        }
        
        private void InitializeManagers()
        {
            // Ensure all managers are present
            if (AvatarManager.Instance == null)
            {
                GameObject avatarManagerObj = new GameObject("AvatarManager");
                avatarManagerObj.AddComponent<AvatarManager>();
            }
            
            if (ParkEnvironmentManager.Instance == null)
            {
                GameObject envManagerObj = new GameObject("ParkEnvironmentManager");
                envManagerObj.AddComponent<ParkEnvironmentManager>();
            }
            
            if (CameraController.Instance == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    mainCamera.gameObject.AddComponent<CameraController>();
                }
            }
            
            if (ScreenshotManager.Instance == null)
            {
                GameObject screenshotManagerObj = new GameObject("ScreenshotManager");
                screenshotManagerObj.AddComponent<ScreenshotManager>();
            }
        }
        
        // Methods called from iOS Swift code
        
        /// <summary>
        /// Load avatar data from iOS
        /// Called via UnityFramework messaging
        /// </summary>
        /// <param name="avatarJson">JSON string containing avatar data</param>
        public void LoadAvatarFromiOS(string avatarJson)
        {
            Debug.Log("Received avatar data from iOS");
            
            if (AvatarManager.Instance != null)
            {
                AvatarManager.Instance.LoadAvatar(avatarJson);
            }
            else
            {
                Debug.LogError("AvatarManager not found!");
            }
        }
        
        /// <summary>
        /// Reset camera position
        /// Called via UnityFramework messaging
        /// </summary>
        public void ResetCameraFromiOS()
        {
            Debug.Log("Reset camera requested from iOS");
            
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ResetCamera();
            }
        }
        
        /// <summary>
        /// Capture screenshot
        /// Called via UnityFramework messaging
        /// </summary>
        public void CaptureScreenshotFromiOS()
        {
            Debug.Log("Screenshot requested from iOS");
            
            if (ScreenshotManager.Instance != null)
            {
                ScreenshotManager.Instance.CaptureScreenshot();
            }
        }
        
        /// <summary>
        /// Set avatar expression
        /// Called via UnityFramework messaging
        /// </summary>
        /// <param name="expressionData">JSON containing expression and intensity</param>
        public void SetAvatarExpressionFromiOS(string expressionData)
        {
            try
            {
                var data = JsonUtility.FromJson<ExpressionData>(expressionData);
                
                if (AvatarManager.Instance != null)
                {
                    AvatarManager.Instance.SetAvatarExpression(data.expression, data.intensity);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse expression data: {e.Message}");
            }
        }
        
        /// <summary>
        /// Pause Unity rendering
        /// Called when iOS app goes to background
        /// </summary>
        public void PauseUnityFromiOS()
        {
            Debug.Log("Unity paused from iOS");
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        
        /// <summary>
        /// Resume Unity rendering
        /// Called when iOS app comes to foreground
        /// </summary>
        public void ResumeUnityFromiOS()
        {
            Debug.Log("Unity resumed from iOS");
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
        
        /// <summary>
        /// Update camera settings from iOS
        /// </summary>
        /// <param name="settingsJson">JSON containing camera settings</param>
        public void UpdateCameraSettingsFromiOS(string settingsJson)
        {
            try
            {
                var settings = JsonUtility.FromJson<CameraSettings>(settingsJson);
                
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.SetCameraDistance(settings.distance);
                    CameraController.Instance.EnableAutoRotation(settings.autoRotation);
                    CameraController.Instance.SetAutoRotationSpeed(settings.rotationSpeed);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse camera settings: {e.Message}");
            }
        }
        
        // Methods to send messages to iOS
        
        /// <summary>
        /// Send message to iOS Swift code
        /// </summary>
        /// <param name="methodName">Method name to call in iOS</param>
        /// <param name="message">Message data to send</param>
        private void SendMessageToiOS(string methodName, string message)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            Application.ExternalCall(methodName, message);
            #else
            Debug.Log($"Would send to iOS: {methodName} - {message}");
            #endif
        }
        
        /// <summary>
        /// Notify iOS that avatar loading is complete
        /// </summary>
        public void NotifyAvatarLoadComplete()
        {
            SendMessageToiOS("OnAvatarLoadComplete", "");
        }
        
        /// <summary>
        /// Notify iOS that avatar loading failed
        /// </summary>
        /// <param name="error">Error message</param>
        public void NotifyAvatarLoadFailed(string error)
        {
            SendMessageToiOS("OnAvatarLoadFailed", error);
        }
        
        /// <summary>
        /// Notify iOS that screenshot was captured
        /// </summary>
        public void NotifyScreenshotCaptured()
        {
            SendMessageToiOS("OnScreenshotCaptured", "");
        }
        
        /// <summary>
        /// Send analytics data to iOS
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="parameters">Event parameters as JSON</param>
        public void SendAnalyticsToiOS(string eventName, string parameters)
        {
            string analyticsData = JsonUtility.ToJson(new AnalyticsEvent
            {
                eventName = eventName,
                parameters = parameters,
                timestamp = System.DateTime.Now.Ticks
            });
            
            SendMessageToiOS("OnAnalyticsEvent", analyticsData);
        }
        
        // Event handlers for Unity components
        
        private void OnEnable()
        {
            // Subscribe to manager events
            if (AvatarManager.Instance != null)
            {
                AvatarManager.Instance.OnAvatarLoaded += OnAvatarLoaded;
                AvatarManager.Instance.OnAvatarLoadFailed += OnAvatarLoadError;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from manager events
            if (AvatarManager.Instance != null)
            {
                AvatarManager.Instance.OnAvatarLoaded -= OnAvatarLoaded;
                AvatarManager.Instance.OnAvatarLoadFailed -= OnAvatarLoadError;
            }
        }
        
        private void OnAvatarLoaded(GameObject avatar)
        {
            Debug.Log("Avatar loaded successfully in Unity");
            NotifyAvatarLoadComplete();
            
            // Send analytics
            SendAnalyticsToiOS("avatar_loaded", JsonUtility.ToJson(new { success = true }));
            
            // Start cinematic intro
            if (CameraController.Instance != null)
            {
                CameraController.Instance.StartCinematicIntro();
            }
        }
        
        private void OnAvatarLoadError(string error)
        {
            Debug.LogError($"Avatar load failed: {error}");
            NotifyAvatarLoadFailed(error);
            
            // Send analytics
            SendAnalyticsToiOS("avatar_load_failed", JsonUtility.ToJson(new { error = error }));
        }
        
        // Application lifecycle handlers
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SendMessageToiOS("OnUnityPaused", "");
            }
            else
            {
                SendMessageToiOS("OnUnityResumed", "");
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SendMessageToiOS("OnUnityFocused", "");
            }
            else
            {
                SendMessageToiOS("OnUnityUnfocused", "");
            }
        }
        
        // Performance monitoring
        private void Update()
        {
            // Send performance data periodically
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60 FPS
            {
                SendPerformanceData();
            }
        }
        
        private void SendPerformanceData()
        {
            var performanceData = new PerformanceData
            {
                fps = (int)(1f / Time.unscaledDeltaTime),
                frameTime = Time.unscaledDeltaTime * 1000f, // ms
                memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / (1024 * 1024), // MB
                drawCalls = UnityEngine.Rendering.DebugUI.Panel.children.Count // Approximation
            };
            
            SendMessageToiOS("OnPerformanceUpdate", JsonUtility.ToJson(performanceData));
        }
    }
    
    // Data structures for iOS communication
    
    [System.Serializable]
    public class ExpressionData
    {
        public string expression;
        public float intensity;
    }
    
    [System.Serializable]
    public class CameraSettings
    {
        public float distance = 5f;
        public bool autoRotation = true;
        public float rotationSpeed = 5f;
    }
    
    [System.Serializable]
    public class AnalyticsEvent
    {
        public string eventName;
        public string parameters;
        public long timestamp;
    }
    
    [System.Serializable]
    public class PerformanceData
    {
        public int fps;
        public float frameTime;
        public long memoryUsage;
        public int drawCalls;
    }
}