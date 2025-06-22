using System.Collections;
using UnityEngine;
using System.IO;

namespace MirrorWorld
{
    public class ScreenshotManager : MonoBehaviour
    {
        [Header("Screenshot Settings")]
        public int screenshotWidth = 1920;
        public int screenshotHeight = 1080;
        public bool hideUIForScreenshot = true;
        public float screenshotDelay = 0.1f;
        
        [Header("Camera Settings")]
        public Camera screenshotCamera;
        public LayerMask screenshotLayers = -1;
        
        private bool isCapturing = false;
        
        public static ScreenshotManager Instance { get; private set; }
        
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
            if (screenshotCamera == null)
            {
                screenshotCamera = Camera.main;
            }
        }
        
        /// <summary>
        /// Called from iOS app via Unity messaging system
        /// </summary>
        public void CaptureScreenshot()
        {
            if (!isCapturing)
            {
                StartCoroutine(CaptureScreenshotCoroutine());
            }
        }
        
        private IEnumerator CaptureScreenshotCoroutine()
        {
            isCapturing = true;
            
            try
            {
                Debug.Log("Capturing screenshot...");
                
                // Wait for end of frame to ensure everything is rendered
                yield return new WaitForEndOfFrame();
                
                // Hide UI elements if requested
                Canvas[] canvases = null;
                bool[] originalCanvasStates = null;
                
                if (hideUIForScreenshot)
                {
                    canvases = FindObjectsOfType<Canvas>();
                    originalCanvasStates = new bool[canvases.Length];
                    
                    for (int i = 0; i < canvases.Length; i++)
                    {
                        originalCanvasStates[i] = canvases[i].enabled;
                        canvases[i].enabled = false;
                    }
                }
                
                // Wait a frame for UI to disappear
                if (hideUIForScreenshot)
                {
                    yield return null;
                }
                
                // Additional delay if specified
                if (screenshotDelay > 0)
                {
                    yield return new WaitForSeconds(screenshotDelay);
                }
                
                // Capture the screenshot
                Texture2D screenshot = CaptureScreenshotTexture();
                
                if (screenshot != null)
                {
                    // Save to device photo library
                    yield return StartCoroutine(SaveScreenshotToGallery(screenshot));
                    
                    // Clean up texture
                    Destroy(screenshot);
                    
                    Debug.Log("Screenshot saved successfully!");
                }
                else
                {
                    Debug.LogError("Failed to capture screenshot");
                }
                
                // Restore UI elements
                if (hideUIForScreenshot && canvases != null)
                {
                    for (int i = 0; i < canvases.Length; i++)
                    {
                        if (canvases[i] != null)
                        {
                            canvases[i].enabled = originalCanvasStates[i];
                        }
                    }
                }
            }
            finally
            {
                isCapturing = false;
            }
        }
        
        private Texture2D CaptureScreenshotTexture()
        {
            try
            {
                // Create render texture
                RenderTexture renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
                renderTexture.antiAliasing = 4;
                
                // Store original camera settings
                RenderTexture originalTarget = screenshotCamera.targetTexture;
                int originalCullingMask = screenshotCamera.cullingMask;
                
                // Configure camera for screenshot
                screenshotCamera.targetTexture = renderTexture;
                screenshotCamera.cullingMask = screenshotLayers;
                
                // Render to texture
                screenshotCamera.Render();
                
                // Read pixels from render texture
                RenderTexture.active = renderTexture;
                Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
                screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
                screenshot.Apply();
                
                // Restore camera settings
                screenshotCamera.targetTexture = originalTarget;
                screenshotCamera.cullingMask = originalCullingMask;
                RenderTexture.active = null;
                
                // Clean up render texture
                Destroy(renderTexture);
                
                return screenshot;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error capturing screenshot: {e.Message}");
                return null;
            }
        }
        
        private IEnumerator SaveScreenshotToGallery(Texture2D screenshot)
        {
            #if UNITY_IOS
            yield return StartCoroutine(SaveToIOSPhotoLibrary(screenshot));
            #elif UNITY_ANDROID
            yield return StartCoroutine(SaveToAndroidGallery(screenshot));
            #else
            yield return StartCoroutine(SaveToDesktop(screenshot));
            #endif
        }
        
        #if UNITY_IOS
        private IEnumerator SaveToIOSPhotoLibrary(Texture2D screenshot)
        {
            // Convert to PNG
            byte[] pngData = screenshot.EncodeToPNG();
            
            // Save to iOS photo library using native plugin
            string base64Data = System.Convert.ToBase64String(pngData);
            
            // Call iOS native method to save to photo library
            // This would typically use a native iOS plugin
            SaveImageToIOSPhotos(base64Data);
            
            yield return null;
        }
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SaveImageToIOSPhotos(string base64ImageData);
        #endif
        
        #if UNITY_ANDROID
        private IEnumerator SaveToAndroidGallery(Texture2D screenshot)
        {
            // Convert to PNG
            byte[] pngData = screenshot.EncodeToPNG();
            
            // Generate filename
            string filename = $"MirrorWorld_Avatar_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            
            // Save to Android gallery
            string path = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(path, pngData);
            
            // Add to Android media store
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
            {
                using (AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
                {
                    mediaStore.CallStatic<string>("insertImage", contentResolver, path, filename, "MirrorWorld Avatar Screenshot");
                }
            }
            
            yield return null;
        }
        #endif
        
        private IEnumerator SaveToDesktop(Texture2D screenshot)
        {
            // Convert to PNG
            byte[] pngData = screenshot.EncodeToPNG();
            
            // Generate filename
            string filename = $"MirrorWorld_Avatar_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = Path.Combine(Application.persistentDataPath, filename);
            
            // Save to file
            File.WriteAllBytes(path, pngData);
            
            Debug.Log($"Screenshot saved to: {path}");
            
            yield return null;
        }
        
        public void CaptureScreenshotWithCallback(System.Action<Texture2D> callback)
        {
            StartCoroutine(CaptureScreenshotWithCallbackCoroutine(callback));
        }
        
        private IEnumerator CaptureScreenshotWithCallbackCoroutine(System.Action<Texture2D> callback)
        {
            yield return new WaitForEndOfFrame();
            
            Texture2D screenshot = CaptureScreenshotTexture();
            callback?.Invoke(screenshot);
        }
        
        public void SetScreenshotResolution(int width, int height)
        {
            screenshotWidth = width;
            screenshotHeight = height;
        }
        
        public void SetScreenshotCamera(Camera camera)
        {
            screenshotCamera = camera;
        }
        
        // High quality screenshot for sharing
        public void CaptureHighQualityScreenshot()
        {
            StartCoroutine(CaptureHighQualityScreenshotCoroutine());
        }
        
        private IEnumerator CaptureHighQualityScreenshotCoroutine()
        {
            // Temporarily increase resolution
            int originalWidth = screenshotWidth;
            int originalHeight = screenshotHeight;
            
            screenshotWidth = 2048;
            screenshotHeight = 2048;
            
            yield return StartCoroutine(CaptureScreenshotCoroutine());
            
            // Restore original resolution
            screenshotWidth = originalWidth;
            screenshotHeight = originalHeight;
        }
        
        // Portrait mode screenshot
        public void CapturePortraitScreenshot()
        {
            StartCoroutine(CapturePortraitScreenshotCoroutine());
        }
        
        private IEnumerator CapturePortraitScreenshotCoroutine()
        {
            // Temporarily set portrait aspect ratio
            int originalWidth = screenshotWidth;
            int originalHeight = screenshotHeight;
            
            screenshotWidth = 1080;
            screenshotHeight = 1920;
            
            yield return StartCoroutine(CaptureScreenshotCoroutine());
            
            // Restore original resolution
            screenshotWidth = originalWidth;
            screenshotHeight = originalHeight;
        }
        
        // Panoramic screenshot by stitching multiple camera angles
        public void CapturePanoramicScreenshot()
        {
            StartCoroutine(CapturePanoramicScreenshotCoroutine());
        }
        
        private IEnumerator CapturePanoramicScreenshotCoroutine()
        {
            if (screenshotCamera == null) yield break;
            
            int panelCount = 6;
            float angleStep = 360f / panelCount;
            Texture2D[] panels = new Texture2D[panelCount];
            
            Vector3 originalRotation = screenshotCamera.transform.eulerAngles;
            
            // Capture panels
            for (int i = 0; i < panelCount; i++)
            {
                // Rotate camera
                float angle = i * angleStep;
                screenshotCamera.transform.eulerAngles = new Vector3(originalRotation.x, angle, originalRotation.z);
                
                yield return new WaitForEndOfFrame();
                
                // Capture panel
                panels[i] = CaptureScreenshotTexture();
                
                yield return null;
            }
            
            // Restore camera rotation
            screenshotCamera.transform.eulerAngles = originalRotation;
            
            // Create panoramic texture
            int panoramicWidth = screenshotWidth * panelCount;
            int panoramicHeight = screenshotHeight;
            
            Texture2D panoramic = new Texture2D(panoramicWidth, panoramicHeight, TextureFormat.RGB24, false);
            
            // Stitch panels together
            for (int i = 0; i < panelCount; i++)
            {
                if (panels[i] != null)
                {
                    Color[] pixels = panels[i].GetPixels();
                    panoramic.SetPixels(i * screenshotWidth, 0, screenshotWidth, screenshotHeight, pixels);
                    Destroy(panels[i]);
                }
            }
            
            panoramic.Apply();
            
            // Save panoramic screenshot
            yield return StartCoroutine(SaveScreenshotToGallery(panoramic));
            
            Destroy(panoramic);
            
            Debug.Log("Panoramic screenshot captured!");
        }
    }
}

#if UNITY_IOS && !UNITY_EDITOR
// iOS native plugin interface
public static class IOSScreenshotPlugin
{
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void SaveImageToIOSPhotos(string base64ImageData);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern bool HasPhotoLibraryPermission();
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void RequestPhotoLibraryPermission();
}
#endif