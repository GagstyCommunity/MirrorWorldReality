using UnityEngine;
using System.Collections;

namespace MirrorWorld
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Configuration")]
        public Transform target; // Avatar to follow
        public float distance = 5f;
        public float height = 2f;
        public float rotationSpeed = 100f;
        public float zoomSpeed = 4f;
        public float minDistance = 1f;
        public float maxDistance = 10f;
        
        [Header("Touch Controls")]
        public bool enableTouchControls = true;
        public float touchSensitivity = 2f;
        public float pinchSensitivity = 0.5f;
        
        [Header("Auto Focus")]
        public bool autoFocusOnAvatar = true;
        public float focusTransitionTime = 2f;
        
        private float currentX = 0f;
        private float currentY = 20f;
        private float currentDistance;
        private bool isTransitioning = false;
        
        // Touch input tracking
        private Vector2 lastTouchPosition;
        private bool isTouching = false;
        private bool isPinching = false;
        private float lastPinchDistance = 0f;
        
        private void Start()
        {
            currentDistance = distance;
            
            // Find avatar if not assigned
            if (target == null)
            {
                var avatarManager = FindObjectOfType<AvatarManager>();
                if (avatarManager != null)
                {
                    StartCoroutine(WaitForAvatarAndFocus(avatarManager));
                }
            }
            
            UpdateCameraPosition();
        }
        
        private IEnumerator WaitForAvatarAndFocus(AvatarManager avatarManager)
        {
            // Wait for avatar to be created
            while (target == null)
            {
                var avatar = GameObject.Find("RealisticAvatar");
                if (avatar != null)
                {
                    target = avatar.transform;
                    if (autoFocusOnAvatar)
                    {
                        FocusOnAvatar();
                    }
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void Update()
        {
            if (target == null) return;
            
            HandleInput();
            
            if (!isTransitioning)
            {
                UpdateCameraPosition();
            }
        }
        
        private void HandleInput()
        {
            // Desktop mouse controls
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
                
                currentX += mouseX;
                currentY -= mouseY;
                currentY = Mathf.Clamp(currentY, -80f, 80f);
            }
            
            // Mouse wheel zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                currentDistance -= scroll * zoomSpeed;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            }
            
            // Touch controls for mobile
            if (enableTouchControls)
            {
                HandleTouchInput();
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
                    isPinching = false;
                }
                else if (touch.phase == TouchPhase.Moved && isTouching && !isPinching)
                {
                    Vector2 deltaPosition = touch.position - lastTouchPosition;
                    
                    currentX += deltaPosition.x * touchSensitivity * Time.deltaTime;
                    currentY -= deltaPosition.y * touchSensitivity * Time.deltaTime;
                    currentY = Mathf.Clamp(currentY, -80f, 80f);
                    
                    lastTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    isTouching = false;
                }
            }
            else if (Input.touchCount == 2)
            {
                // Pinch to zoom
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                
                float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                
                if (!isPinching)
                {
                    isPinching = true;
                    lastPinchDistance = currentPinchDistance;
                    isTouching = false;
                }
                else
                {
                    float deltaPinch = currentPinchDistance - lastPinchDistance;
                    currentDistance -= deltaPinch * pinchSensitivity * Time.deltaTime;
                    currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
                    
                    lastPinchDistance = currentPinchDistance;
                }
            }
            else
            {
                isTouching = false;
                isPinching = false;
            }
        }
        
        private void UpdateCameraPosition()
        {
            // Calculate desired position
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 direction = rotation * Vector3.back;
            Vector3 desiredPosition = target.position + direction * currentDistance + Vector3.up * height;
            
            // Apply position and look at target
            transform.position = desiredPosition;
            transform.LookAt(target.position + Vector3.up * height * 0.5f);
        }
        
        public void FocusOnAvatar()
        {
            if (target == null) return;
            
            StartCoroutine(SmoothFocusTransition());
        }
        
        private IEnumerator SmoothFocusTransition()
        {
            isTransitioning = true;
            
            // Calculate optimal viewing position
            Vector3 targetPosition = target.position;
            float optimalDistance = 3f; // Good distance to view the avatar
            float optimalHeight = 1.5f;
            float optimalAngleX = 0f; // Face the avatar directly
            float optimalAngleY = 10f; // Slightly above
            
            float startDistance = currentDistance;
            float startHeight = height;
            float startX = currentX;
            float startY = currentY;
            
            float elapsed = 0f;
            while (elapsed < focusTransitionTime)
            {
                float t = elapsed / focusTransitionTime;
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing
                
                currentDistance = Mathf.Lerp(startDistance, optimalDistance, t);
                height = Mathf.Lerp(startHeight, optimalHeight, t);
                currentX = Mathf.LerpAngle(startX, optimalAngleX, t);
                currentY = Mathf.Lerp(startY, optimalAngleY, t);
                
                UpdateCameraPosition();
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final values
            currentDistance = optimalDistance;
            height = optimalHeight;
            currentX = optimalAngleX;
            currentY = optimalAngleY;
            
            UpdateCameraPosition();
            isTransitioning = false;
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (autoFocusOnAvatar)
            {
                FocusOnAvatar();
            }
        }
        
        public void ResetCamera()
        {
            currentX = 0f;
            currentY = 20f;
            currentDistance = distance;
            height = 2f;
            
            if (target != null)
            {
                UpdateCameraPosition();
            }
        }
        
        // Called from UI buttons
        public void ZoomIn()
        {
            currentDistance = Mathf.Clamp(currentDistance - 1f, minDistance, maxDistance);
        }
        
        public void ZoomOut()
        {
            currentDistance = Mathf.Clamp(currentDistance + 1f, minDistance, maxDistance);
        }
        
        public void RotateLeft()
        {
            currentX -= 45f;
        }
        
        public void RotateRight()
        {
            currentX += 45f;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                // Draw camera target
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(target.position, 0.2f);
                
                // Draw distance limits
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(target.position, minDistance);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target.position, maxDistance);
                
                // Draw current camera position
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(target.position, transform.position);
            }
        }
    }
}