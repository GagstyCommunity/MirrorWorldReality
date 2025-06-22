using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorWorld
{
    public class AvatarAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float breathingIntensity = 0.05f;
        public float breathingSpeed = 2f;
        public float blinkInterval = 3f;
        public float headMovementRange = 15f;
        public float expressionTransitionSpeed = 2f;
        
        [Header("References")]
        public Transform headBone;
        public Transform eyeLeft;
        public Transform eyeRight;
        public SkinnedMeshRenderer faceMeshRenderer;
        
        private AvatarData avatarData;
        private Animator animator;
        private Coroutine breathingCoroutine;
        private Coroutine blinkingCoroutine;
        private Coroutine headMovementCoroutine;
        
        private Dictionary<string, float> currentBlendShapeWeights = new Dictionary<string, float>();
        private Dictionary<string, float> targetBlendShapeWeights = new Dictionary<string, float>();
        
        private Vector3 originalScale;
        private Vector3 originalHeadRotation;
        private Vector3 originalEyeScale;
        
        public bool IsInitialized { get; private set; }
        
        public void Initialize(AvatarData data, float breathingIntensity, float breathingSpeed, float blinkInterval, float headMovementRange)
        {
            this.avatarData = data;
            this.breathingIntensity = breathingIntensity;
            this.breathingSpeed = breathingSpeed;
            this.blinkInterval = blinkInterval;
            this.headMovementRange = headMovementRange;
            
            SetupComponents();
            InitializeBlendShapes();
            StartAnimations();
            
            IsInitialized = true;
            Debug.Log("Avatar Animation Controller initialized successfully");
        }
        
        private void SetupComponents()
        {
            animator = GetComponent<Animator>();
            
            // Store original values
            originalScale = transform.localScale;
            
            // Find or create bone references
            if (headBone == null)
            {
                headBone = FindChildByName(transform, "Head") ?? transform;
            }
            
            if (eyeLeft == null)
            {
                eyeLeft = FindChildByName(transform, "EyeLeft") ?? FindChildByName(transform, "LeftEye");
            }
            
            if (eyeRight == null)
            {
                eyeRight = FindChildByName(transform, "EyeRight") ?? FindChildByName(transform, "RightEye");
            }
            
            if (faceMeshRenderer == null)
            {
                faceMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }
            
            if (headBone != null)
            {
                originalHeadRotation = headBone.localEulerAngles;
            }
            
            if (eyeLeft != null)
            {
                originalEyeScale = eyeLeft.localScale;
            }
        }
        
        private void InitializeBlendShapes()
        {
            if (avatarData?.animationData?.blendShapes == null) return;
            
            foreach (var blendShape in avatarData.animationData.blendShapes)
            {
                currentBlendShapeWeights[blendShape.Key] = 0f;
                targetBlendShapeWeights[blendShape.Key] = 0f;
            }
        }
        
        private void StartAnimations()
        {
            StopAllAnimations();
            
            breathingCoroutine = StartCoroutine(BreathingAnimation());
            blinkingCoroutine = StartCoroutine(BlinkingAnimation());
            headMovementCoroutine = StartCoroutine(HeadMovementAnimation());
            
            // Start blend shape interpolation
            StartCoroutine(BlendShapeInterpolation());
        }
        
        private void StopAllAnimations()
        {
            if (breathingCoroutine != null) StopCoroutine(breathingCoroutine);
            if (blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);
            if (headMovementCoroutine != null) StopCoroutine(headMovementCoroutine);
        }
        
        private IEnumerator BreathingAnimation()
        {
            while (true)
            {
                float time = Time.time * breathingSpeed;
                float breathingScale = 1f + Mathf.Sin(time) * breathingIntensity;
                
                Vector3 newScale = originalScale;
                newScale.y *= breathingScale;
                transform.localScale = newScale;
                
                yield return null;
            }
        }
        
        private IEnumerator BlinkingAnimation()
        {
            while (true)
            {
                yield return new WaitForSeconds(blinkInterval + Random.Range(-0.5f, 0.5f));
                
                // Trigger blink
                yield return StartCoroutine(PerformBlink());
            }
        }
        
        private IEnumerator PerformBlink()
        {
            float blinkDuration = 0.15f;
            float halfDuration = blinkDuration / 2f;
            
            // Blink down
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float progress = t / halfDuration;
                ApplyBlinkScale(Mathf.Lerp(1f, 0.1f, progress));
                yield return null;
            }
            
            // Blink up
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float progress = t / halfDuration;
                ApplyBlinkScale(Mathf.Lerp(0.1f, 1f, progress));
                yield return null;
            }
            
            ApplyBlinkScale(1f);
        }
        
        private void ApplyBlinkScale(float scale)
        {
            if (eyeLeft != null)
            {
                Vector3 eyeScale = originalEyeScale;
                eyeScale.y *= scale;
                eyeLeft.localScale = eyeScale;
            }
            
            if (eyeRight != null)
            {
                Vector3 eyeScale = originalEyeScale;
                eyeScale.y *= scale;
                eyeRight.localScale = eyeScale;
            }
            
            // Also apply to blend shapes if available
            if (targetBlendShapeWeights.ContainsKey("blink"))
            {
                targetBlendShapeWeights["blink"] = 1f - scale;
            }
        }
        
        private IEnumerator HeadMovementAnimation()
        {
            if (headBone == null) yield break;
            
            while (true)
            {
                float duration = Random.Range(3f, 6f);
                Vector3 targetRotation = originalHeadRotation + new Vector3(
                    Random.Range(-headMovementRange, headMovementRange) * 0.3f,
                    Random.Range(-headMovementRange, headMovementRange),
                    Random.Range(-headMovementRange, headMovementRange) * 0.2f
                );
                
                Vector3 startRotation = headBone.localEulerAngles;
                
                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    float progress = t / duration;
                    Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, progress);
                    headBone.localEulerAngles = currentRotation;
                    yield return null;
                }
                
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }
        }
        
        private IEnumerator BlendShapeInterpolation()
        {
            if (faceMeshRenderer == null) yield break;
            
            while (true)
            {
                bool hasChanges = false;
                
                foreach (var blendShape in currentBlendShapeWeights.Keys)
                {
                    float current = currentBlendShapeWeights[blendShape];
                    float target = targetBlendShapeWeights[blendShape];
                    
                    if (Mathf.Abs(current - target) > 0.01f)
                    {
                        float newValue = Mathf.Lerp(current, target, Time.deltaTime * expressionTransitionSpeed);
                        currentBlendShapeWeights[blendShape] = newValue;
                        
                        // Apply to mesh renderer if the blend shape exists
                        int blendShapeIndex = faceMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShape);
                        if (blendShapeIndex >= 0)
                        {
                            faceMeshRenderer.SetBlendShapeWeight(blendShapeIndex, newValue * 100f);
                        }
                        
                        hasChanges = true;
                    }
                }
                
                yield return hasChanges ? null : new WaitForSeconds(0.016f); // ~60 FPS
            }
        }
        
        public void SetExpression(string expression, float intensity)
        {
            if (targetBlendShapeWeights.ContainsKey(expression))
            {
                targetBlendShapeWeights[expression] = Mathf.Clamp01(intensity);
            }
            else
            {
                Debug.LogWarning($"Expression '{expression}' not found in avatar blend shapes");
            }
        }
        
        public void SetMultipleExpressions(Dictionary<string, float> expressions)
        {
            foreach (var expression in expressions)
            {
                SetExpression(expression.Key, expression.Value);
            }
        }
        
        public void ResetAnimations()
        {
            // Reset transform
            transform.localScale = originalScale;
            
            if (headBone != null)
            {
                headBone.localEulerAngles = originalHeadRotation;
            }
            
            if (eyeLeft != null)
            {
                eyeLeft.localScale = originalEyeScale;
            }
            
            if (eyeRight != null)
            {
                eyeRight.localScale = originalEyeScale;
            }
            
            // Reset blend shapes
            foreach (var key in targetBlendShapeWeights.Keys.ToArray())
            {
                targetBlendShapeWeights[key] = 0f;
            }
            
            // Restart animations
            if (IsInitialized)
            {
                StartAnimations();
            }
        }
        
        public void PauseAnimations()
        {
            StopAllAnimations();
        }
        
        public void ResumeAnimations()
        {
            if (IsInitialized)
            {
                StartAnimations();
            }
        }
        
        public void SetAnimationSpeed(float multiplier)
        {
            breathingSpeed *= multiplier;
            blinkInterval /= multiplier;
            expressionTransitionSpeed *= multiplier;
        }
        
        public void TriggerExpression(string expression, float intensity, float duration)
        {
            StartCoroutine(TemporaryExpression(expression, intensity, duration));
        }
        
        private IEnumerator TemporaryExpression(string expression, float intensity, float duration)
        {
            float originalIntensity = targetBlendShapeWeights.ContainsKey(expression) ? targetBlendShapeWeights[expression] : 0f;
            
            SetExpression(expression, intensity);
            yield return new WaitForSeconds(duration);
            SetExpression(expression, originalIntensity);
        }
        
        private Transform FindChildByName(Transform parent, string name)
        {
            if (parent.name.ToLower().Contains(name.ToLower()))
            {
                return parent;
            }
            
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildByName(parent.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
        
        private void OnDestroy()
        {
            StopAllAnimations();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseAnimations();
            }
            else
            {
                ResumeAnimations();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                ResumeAnimations();
            }
            else
            {
                PauseAnimations();
            }
        }
    }
    
    // Extension methods for Dictionary access
    public static class DictionaryExtensions
    {
        public static IEnumerable<TKey> ToArray<TKey, TValue>(this Dictionary<TKey, TValue>.KeyCollection keys)
        {
            return keys.ToList();
        }
    }
}