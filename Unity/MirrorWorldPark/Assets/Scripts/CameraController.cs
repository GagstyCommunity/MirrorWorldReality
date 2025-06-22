using System.Collections;
using UnityEngine;

namespace MirrorWorld
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        public Transform target;
        public float distance = 5f;
        public float minDistance = 2f;
        public float maxDistance = 15f;
        public float height = 2f;
        public float smoothTime = 0.3f;
        
        [Header("Rotation Settings")]
        public float rotationSpeed = 2f;
        public float minVerticalAngle = -30f;
        public float maxVerticalAngle = 60f;
        public bool invertY = false;
        
        [Header("Touch Settings")]
        public float touchSensitivity = 1f;
        public float pinchSensitivity = 1f;
        public float doubleTapZoomSpeed = 2f;
        
        [Header("Auto Movement")]
        public bool enableAutoRotation = true;
        public float autoRotationSpeed = 5f;
        public float autoRotationDelay = 5f;
        
        private Camera cam;
        private float currentHorizontalAngle = 0f;
        private float currentVerticalAngle = 20f;
        private float currentDistance;
        private Vector3 currentVelocity;
        
        private bool isUserControlling = false;
        private float lastInputTime;
        private Vector2 lastTouchPosition;
        private float lastTouchDistance;
        private bool isTouching = false;
        
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float originalDistance;
        
        public static CameraController Instance { get; private set; }
        
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
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
            
            if (target == null)
            {
                // Find avatar target or create default
                GameObject avatarTarget = GameObject.FindGameObjectWithTag("Avatar");
                if (avatarTarget != null)
                {
                    target = avatarTarget.transform;
                }
                else
                {
                    // Create default target
                    GameObject defaultTarget = new GameObject("Camera Target");
                    defaultTarget.transform.position = new Vector3(0, 1.6f, -1f);
                    target = defaultTarget.transform;
                }
            }
            
            currentDistance = distance;
            
            // Store original values for reset
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalDistance = distance;
            
            // Setup initial camera position
            UpdateCameraPosition();
        }
        
        private void Update()
        {
            HandleInput();
            UpdateAutoRotation();
            UpdateCameraPosition();
        }
        
        private void HandleInput()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
            #elif UNITY_IOS || UNITY_ANDROID
            HandleTouchInput();
            #endif
        }
        
        private void HandleMouseInput()
        {
            // Mouse rotation
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
                
                if (invertY) mouseY = -mouseY;
                
                currentHorizontalAngle += mouseX;
                currentVerticalAngle -= mouseY;
                currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
                
                isUserControlling = true;
                lastInputTime = Time.time;
            }
            
            // Mouse scroll zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentDistance -= scroll * pinchSensitivity * 2f;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
                
                isUserControlling = true;
                lastInputTime = Time.time;
            }
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPosition = touch.position;
                    isTouching = true;
                }
                else if (touch.phase == TouchPhase.Moved && isTouching)
                {
                    Vector2 deltaPosition = touch.position - lastTouchPosition;
                    
                    float deltaX = deltaPosition.x * touchSensitivity * 0.01f;
                    float deltaY = deltaPosition.y * touchSensitivity * 0.01f;
                    
                    if (invertY) deltaY = -deltaY;
                    
                    currentHorizontalAngle += deltaX;
                    currentVerticalAngle -= deltaY;
                    currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
                    
                    lastTouchPosition = touch.position;
                    isUserControlling = true;
                    lastInputTime = Time.time;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isTouching = false;
                    
                    // Check for double tap
                    if (touch.tapCount == 2)
                    {
                        StartCoroutine(DoubleTapZoom());
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                
                float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);
                
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    lastTouchDistance = currentTouchDistance;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    float deltaDistance = (lastTouchDistance - currentTouchDistance) * pinchSensitivity * 0.01f;
                    currentDistance += deltaDistance;
                    currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
                    
                    lastTouchDistance = currentTouchDistance;
                    isUserControlling = true;
                    lastInputTime = Time.time;
                }
                
                isTouching = false; // Disable single touch rotation during pinch
            }
            else
            {
                isTouching = false;
            }
        }
        
        private IEnumerator DoubleTapZoom()
        {
            float startDistance = currentDistance;
            float targetDistance = currentDistance > (minDistance + maxDistance) * 0.5f ? minDistance * 1.5f : maxDistance * 0.8f;
            
            float elapsedTime = 0f;
            float duration = 0.5f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                progress = Mathf.SmoothStep(0f, 1f, progress);
                
                currentDistance = Mathf.Lerp(startDistance, targetDistance, progress);
                yield return null;
            }
            
            currentDistance = targetDistance;
        }
        
        private void UpdateAutoRotation()
        {
            if (!enableAutoRotation) return;
            
            // Start auto rotation if user hasn't interacted recently
            if (!isUserControlling && Time.time - lastInputTime > autoRotationDelay)
            {
                currentHorizontalAngle += autoRotationSpeed * Time.deltaTime;
            }
            
            // Stop user control flag after input
            if (isUserControlling && Time.time - lastInputTime > 0.5f)
            {
                isUserControlling = false;
            }
        }
        
        private void UpdateCameraPosition()
        {
            if (target == null) return;
            
            // Calculate desired position
            Vector3 targetPosition = target.position + Vector3.up * height;
            
            // Calculate rotation
            Quaternion horizontalRotation = Quaternion.AngleAxis(currentHorizontalAngle, Vector3.up);
            Quaternion verticalRotation = Quaternion.AngleAxis(currentVerticalAngle, Vector3.right);
            Quaternion finalRotation = horizontalRotation * verticalRotation;
            
            // Calculate camera position
            Vector3 direction = finalRotation * Vector3.back;
            Vector3 desiredPosition = targetPosition + direction * currentDistance;
            
            // Smooth movement
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
            transform.LookAt(targetPosition);
        }
        
        /// <summary>
        /// Called from iOS app via Unity messaging system
        /// </summary>
        public void ResetCamera()
        {
            StartCoroutine(ResetCameraSmooth());
        }
        
        private IEnumerator ResetCameraSmooth()
        {
            float duration = 1f;
            float elapsedTime = 0f;
            
            float startHorizontalAngle = currentHorizontalAngle;
            float startVerticalAngle = currentVerticalAngle;
            float startDistance = currentDistance;
            
            float targetHorizontalAngle = 0f;
            float targetVerticalAngle = 20f;
            float targetDistance = originalDistance;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                progress = Mathf.SmoothStep(0f, 1f, progress);
                
                currentHorizontalAngle = Mathf.Lerp(startHorizontalAngle, targetHorizontalAngle, progress);
                currentVerticalAngle = Mathf.Lerp(startVerticalAngle, targetVerticalAngle, progress);
                currentDistance = Mathf.Lerp(startDistance, targetDistance, progress);
                
                yield return null;
            }
            
            currentHorizontalAngle = targetHorizontalAngle;
            currentVerticalAngle = targetVerticalAngle;
            currentDistance = targetDistance;
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void FocusOnAvatar()
        {
            if (AvatarManager.Instance?.currentAvatar != null)
            {
                SetTarget(AvatarManager.Instance.currentAvatar.transform);
            }
        }
        
        public void SetCameraDistance(float newDistance)
        {
            currentDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }
        
        public void EnableAutoRotation(bool enable)
        {
            enableAutoRotation = enable;
        }
        
        public void SetAutoRotationSpeed(float speed)
        {
            autoRotationSpeed = speed;
        }
        
        // Cinematic camera movements
        public void StartCinematicIntro()
        {
            StartCoroutine(CinematicIntroSequence());
        }
        
        private IEnumerator CinematicIntroSequence()
        {
            // Start far and high
            currentDistance = maxDistance;
            currentVerticalAngle = 45f;
            currentHorizontalAngle = -45f;
            
            // Smoothly move to normal position
            yield return StartCoroutine(SmoothTransition(
                currentHorizontalAngle, 0f,
                currentVerticalAngle, 20f,
                currentDistance, originalDistance,
                3f
            ));
        }
        
        private IEnumerator SmoothTransition(float startH, float endH, float startV, float endV, float startD, float endD, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                progress = Mathf.SmoothStep(0f, 1f, progress);
                
                currentHorizontalAngle = Mathf.Lerp(startH, endH, progress);
                currentVerticalAngle = Mathf.Lerp(startV, endV, progress);
                currentDistance = Mathf.Lerp(startD, endD, progress);
                
                yield return null;
            }
            
            currentHorizontalAngle = endH;
            currentVerticalAngle = endV;
            currentDistance = endD;
        }
        
        // Camera shake for dramatic effect
        public void ShakeCamera(float intensity = 0.1f, float duration = 0.5f)
        {
            StartCoroutine(CameraShake(intensity, duration));
        }
        
        private IEnumerator CameraShake(float intensity, float duration)
        {
            Vector3 originalPos = transform.position;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                
                Vector3 shakeOffset = Random.insideUnitSphere * intensity;
                transform.position = originalPos + shakeOffset;
                
                yield return null;
            }
            
            transform.position = originalPos;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
            
            Gizmos.color = Color.blue;
            Vector3 targetPos = target.position + Vector3.up * height;
            Gizmos.DrawWireSphere(targetPos, minDistance);
            Gizmos.DrawWireSphere(targetPos, maxDistance);
        }
    }
}