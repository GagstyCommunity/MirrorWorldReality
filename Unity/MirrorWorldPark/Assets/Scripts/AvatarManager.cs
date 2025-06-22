using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace MirrorWorld
{
    public class AvatarManager : MonoBehaviour
    {
        [Header("Avatar Settings")]
        public Material defaultAvatarMaterial;
        public Transform avatarSpawnPoint;
        public float loadingTimeout = 30f;
        
        [Header("Animation Settings")]
        public float breathingIntensity = 0.05f;
        public float breathingSpeed = 2f;
        public float blinkInterval = 3f;
        public float headMovementRange = 15f;
        
        private GameObject currentAvatar;
        private AvatarAnimationController animationController;
        private AvatarData currentAvatarData;
        private bool isLoading = false;
        
        public static AvatarManager Instance { get; private set; }
        
        public event Action<GameObject> OnAvatarLoaded;
        public event Action<string> OnAvatarLoadFailed;
        
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
            InitializeAvatarSystem();
        }
        
        private void InitializeAvatarSystem()
        {
            if (avatarSpawnPoint == null)
            {
                GameObject spawnPoint = new GameObject("AvatarSpawnPoint");
                spawnPoint.transform.position = new Vector3(0, 0, -1);
                avatarSpawnPoint = spawnPoint.transform;
            }
            
            Debug.Log("MirrorWorld Avatar Manager initialized");
        }
        
        /// <summary>
        /// Called from iOS app via Unity messaging system
        /// </summary>
        /// <param name="avatarJson">JSON string containing avatar data</param>
        public void LoadAvatar(string avatarJson)
        {
            if (isLoading)
            {
                Debug.LogWarning("Avatar is already loading, please wait...");
                return;
            }
            
            StartCoroutine(LoadAvatarCoroutine(avatarJson));
        }
        
        private IEnumerator LoadAvatarCoroutine(string avatarJson)
        {
            isLoading = true;
            
            try
            {
                Debug.Log("Parsing avatar data...");
                currentAvatarData = JsonConvert.DeserializeObject<AvatarData>(avatarJson);
                
                if (currentAvatarData == null)
                {
                    throw new Exception("Failed to parse avatar data");
                }
                
                // Clear existing avatar
                if (currentAvatar != null)
                {
                    DestroyImmediate(currentAvatar);
                    currentAvatar = null;
                }
                
                // Create new avatar
                yield return StartCoroutine(CreateAvatarMesh());
                yield return StartCoroutine(ApplyAvatarTextures());
                yield return StartCoroutine(SetupAvatarAnimations());
                
                // Position avatar
                if (currentAvatar != null)
                {
                    currentAvatar.transform.position = avatarSpawnPoint.position;
                    currentAvatar.transform.rotation = avatarSpawnPoint.rotation;
                    
                    // Setup animation controller
                    animationController = currentAvatar.GetComponent<AvatarAnimationController>();
                    if (animationController == null)
                    {
                        animationController = currentAvatar.AddComponent<AvatarAnimationController>();
                    }
                    
                    animationController.Initialize(currentAvatarData, breathingIntensity, breathingSpeed, blinkInterval, headMovementRange);
                    
                    Debug.Log("Avatar loaded successfully!");
                    OnAvatarLoaded?.Invoke(currentAvatar);
                }
                else
                {
                    throw new Exception("Failed to create avatar mesh");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load avatar: {e.Message}");
                OnAvatarLoadFailed?.Invoke(e.Message);
            }
            finally
            {
                isLoading = false;
            }
        }
        
        private IEnumerator CreateAvatarMesh()
        {
            Debug.Log("Creating avatar mesh...");
            
            // Create avatar game object
            currentAvatar = new GameObject("3D Avatar");
            
            // Add mesh components
            MeshFilter meshFilter = currentAvatar.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = currentAvatar.AddComponent<MeshRenderer>();
            
            // Create mesh from vertex data
            Mesh avatarMesh = new Mesh();
            avatarMesh.name = "AvatarMesh";
            
            // Convert vertices
            Vector3[] vertices = new Vector3[currentAvatarData.meshData.vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = currentAvatarData.meshData.vertices[i];
                vertices[i] = new Vector3(vertex.x, vertex.y, vertex.z);
            }
            
            // Convert triangles
            int[] triangles = currentAvatarData.meshData.triangles;
            
            // Convert normals
            Vector3[] normals = new Vector3[currentAvatarData.meshData.normals.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                var normal = currentAvatarData.meshData.normals[i];
                normals[i] = new Vector3(normal.x, normal.y, normal.z);
            }
            
            // Convert UVs
            Vector2[] uvs = new Vector2[currentAvatarData.meshData.uvs.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                var uv = currentAvatarData.meshData.uvs[i];
                uvs[i] = new Vector2(uv.x, uv.y);
            }
            
            // Assign mesh data
            avatarMesh.vertices = vertices;
            avatarMesh.triangles = triangles;
            avatarMesh.normals = normals;
            avatarMesh.uv = uvs;
            
            // Recalculate bounds and tangents
            avatarMesh.RecalculateBounds();
            avatarMesh.RecalculateTangents();
            
            meshFilter.mesh = avatarMesh;
            
            // Setup default material
            meshRenderer.material = defaultAvatarMaterial != null ? defaultAvatarMaterial : CreateDefaultMaterial();
            
            yield return null;
        }
        
        private IEnumerator ApplyAvatarTextures()
        {
            Debug.Log("Applying avatar textures...");
            
            if (currentAvatar == null) yield break;
            
            MeshRenderer renderer = currentAvatar.GetComponent<MeshRenderer>();
            Material avatarMaterial = new Material(Shader.Find("Standard"));
            
            // Load diffuse texture
            if (!string.IsNullOrEmpty(currentAvatarData.textureData.diffuseTexture))
            {
                Texture2D diffuseTexture = LoadTextureFromBase64(currentAvatarData.textureData.diffuseTexture);
                if (diffuseTexture != null)
                {
                    avatarMaterial.mainTexture = diffuseTexture;
                }
            }
            
            // Load normal map
            if (!string.IsNullOrEmpty(currentAvatarData.textureData.normalTexture))
            {
                Texture2D normalTexture = LoadTextureFromBase64(currentAvatarData.textureData.normalTexture);
                if (normalTexture != null)
                {
                    avatarMaterial.SetTexture("_BumpMap", normalTexture);
                    avatarMaterial.EnableKeyword("_NORMALMAP");
                }
            }
            
            // Load specular map (using it as metallic map for Standard shader)
            if (!string.IsNullOrEmpty(currentAvatarData.textureData.specularTexture))
            {
                Texture2D specularTexture = LoadTextureFromBase64(currentAvatarData.textureData.specularTexture);
                if (specularTexture != null)
                {
                    avatarMaterial.SetTexture("_MetallicGlossMap", specularTexture);
                    avatarMaterial.EnableKeyword("_METALLICGLOSSMAP");
                }
            }
            
            // Apply material properties for realistic skin
            avatarMaterial.SetFloat("_Metallic", 0.0f);
            avatarMaterial.SetFloat("_Glossiness", 0.2f);
            avatarMaterial.EnableKeyword("_EMISSION");
            
            renderer.material = avatarMaterial;
            
            yield return null;
        }
        
        private IEnumerator SetupAvatarAnimations()
        {
            Debug.Log("Setting up avatar animations...");
            
            if (currentAvatar == null) yield break;
            
            // Add Animator component
            Animator animator = currentAvatar.GetComponent<Animator>();
            if (animator == null)
            {
                animator = currentAvatar.AddComponent<Animator>();
            }
            
            // Create runtime animator controller
            UnityEngine.AnimatorController controller = UnityEngine.AnimatorController.CreateAnimatorControllerAtPath("Assets/Generated/AvatarController.controller");
            
            // Add animation states
            var rootStateMachine = controller.layers[0].stateMachine;
            
            // Idle breathing state
            var idleState = rootStateMachine.AddState("Idle_Breathing");
            idleState.motion = CreateBreathingAnimation();
            
            // Blink state
            var blinkState = rootStateMachine.AddState("Blink");
            blinkState.motion = CreateBlinkAnimation();
            
            // Set default state
            rootStateMachine.defaultState = idleState;
            
            animator.runtimeAnimatorController = controller;
            
            yield return null;
        }
        
        private AnimationClip CreateBreathingAnimation()
        {
            AnimationClip clip = new AnimationClip();
            clip.name = "BreathingAnimation";
            clip.wrapMode = WrapMode.Loop;
            
            // Create breathing curve for chest/body scale
            AnimationCurve breathingCurve = new AnimationCurve();
            breathingCurve.AddKey(0f, 1f);
            breathingCurve.AddKey(1f, 1f + breathingIntensity);
            breathingCurve.AddKey(2f, 1f);
            
            // Apply curve to transform scale
            clip.SetCurve("", typeof(Transform), "localScale.y", breathingCurve);
            
            return clip;
        }
        
        private AnimationClip CreateBlinkAnimation()
        {
            AnimationClip clip = new AnimationClip();
            clip.name = "BlinkAnimation";
            clip.wrapMode = WrapMode.Once;
            
            // Create blink curve for eye scale
            AnimationCurve blinkCurve = new AnimationCurve();
            blinkCurve.AddKey(0f, 1f);
            blinkCurve.AddKey(0.1f, 0.1f);
            blinkCurve.AddKey(0.2f, 1f);
            
            // Apply to blend shapes if available
            if (currentAvatarData?.animationData?.blendShapes?.ContainsKey("blink") == true)
            {
                clip.SetCurve("", typeof(SkinnedMeshRenderer), "blendShape.blink", blinkCurve);
            }
            
            return clip;
        }
        
        private Texture2D LoadTextureFromBase64(string base64Data)
        {
            try
            {
                byte[] imageData = Convert.FromBase64String(base64Data);
                Texture2D texture = new Texture2D(2, 2);
                
                if (texture.LoadImage(imageData))
                {
                    return texture;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load texture from base64: {e.Message}");
            }
            
            return null;
        }
        
        private Material CreateDefaultMaterial()
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.9f, 0.8f, 0.7f); // Skin-like color
            material.SetFloat("_Metallic", 0.0f);
            material.SetFloat("_Glossiness", 0.2f);
            return material;
        }
        
        public void ResetAvatar()
        {
            if (currentAvatar != null)
            {
                currentAvatar.transform.position = avatarSpawnPoint.position;
                currentAvatar.transform.rotation = avatarSpawnPoint.rotation;
                
                if (animationController != null)
                {
                    animationController.ResetAnimations();
                }
            }
        }
        
        public void SetAvatarExpression(string expression, float intensity)
        {
            if (animationController != null)
            {
                animationController.SetExpression(expression, intensity);
            }
        }
        
        private void OnDestroy()
        {
            if (currentAvatar != null)
            {
                DestroyImmediate(currentAvatar);
            }
        }
    }
    
    // Data structures for avatar loading
    [Serializable]
    public class AvatarData
    {
        public UnityMeshData meshData;
        public UnityTextureData textureData;
        public UnityAnimationData animationData;
        public UnityMaterialData materialData;
    }
    
    [Serializable]
    public class UnityMeshData
    {
        public Vector3Data[] vertices;
        public int[] triangles;
        public Vector3Data[] normals;
        public Vector2Data[] uvs;
    }
    
    [Serializable]
    public class UnityTextureData
    {
        public string diffuseTexture;
        public string normalTexture;
        public string specularTexture;
    }
    
    [Serializable]
    public class UnityAnimationData
    {
        public Dictionary<string, float[]> blendShapes;
        public UnityAnimationClip[] animationClips;
    }
    
    [Serializable]
    public class UnityAnimationClip
    {
        public string name;
        public float length;
        public string wrapMode;
        public UnityAnimationCurve[] curves;
    }
    
    [Serializable]
    public class UnityAnimationCurve
    {
        public string propertyName;
        public UnityKeyframe[] keyframes;
    }
    
    [Serializable]
    public class UnityKeyframe
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;
    }
    
    [Serializable]
    public class UnityMaterialData
    {
        public string materialName;
        public string shaderName;
        public Dictionary<string, object> properties;
    }
    
    [Serializable]
    public class Vector3Data
    {
        public float x, y, z;
        
        public Vector3Data(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public static implicit operator Vector3(Vector3Data data)
        {
            return new Vector3(data.x, data.y, data.z);
        }
    }
    
    [Serializable]
    public class Vector2Data
    {
        public float x, y;
        
        public Vector2Data(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        
        public static implicit operator Vector2(Vector2Data data)
        {
            return new Vector2(data.x, data.y);
        }
    }
}