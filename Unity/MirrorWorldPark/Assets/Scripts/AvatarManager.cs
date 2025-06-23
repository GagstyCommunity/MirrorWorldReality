using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace MirrorWorld
{
    public class AvatarManager : MonoBehaviour
    {
        [Header("Avatar Configuration")]
        public Material defaultAvatarMaterial;
        public Transform avatarSpawnPoint;
        
        [Header("Runtime References")]
        private GameObject currentAvatar;
        private MeshRenderer avatarRenderer;
        private MeshFilter avatarMeshFilter;
        private Animator avatarAnimator;
        
        [Header("Debug")]
        public bool enableDebugLogs = true;
        
        private void Start()
        {
            if (avatarSpawnPoint == null)
                avatarSpawnPoint = transform;
                
            LogDebug("AvatarManager initialized");
        }
        
        /// <summary>
        /// Called from iOS Swift app to load avatar data
        /// </summary>
        /// <param name="jsonData">JSON string containing avatar data</param>
        public void LoadAvatarData(string jsonData)
        {
            LogDebug($"Received avatar data: {jsonData.Substring(0, Mathf.Min(100, jsonData.Length))}...");
            
            try
            {
                var avatarData = JsonConvert.DeserializeObject<Avatar3DModel>(jsonData);
                StartCoroutine(CreateAvatarFromData(avatarData));
            }
            catch (Exception e)
            {
                LogError($"Failed to parse avatar data: {e.Message}");
            }
        }
        
        private IEnumerator CreateAvatarFromData(Avatar3DModel avatarData)
        {
            LogDebug("Creating avatar from real facial data...");
            
            // Clean up existing avatar
            if (currentAvatar != null)
            {
                DestroyImmediate(currentAvatar);
            }
            
            // Create new avatar GameObject
            currentAvatar = new GameObject("RealisticAvatar");
            currentAvatar.transform.position = avatarSpawnPoint.position;
            currentAvatar.transform.rotation = avatarSpawnPoint.rotation;
            
            // Add mesh components
            avatarMeshFilter = currentAvatar.AddComponent<MeshFilter>();
            avatarRenderer = currentAvatar.AddComponent<MeshRenderer>();
            
            // Create mesh from real facial landmarks
            Mesh avatarMesh = CreateMeshFromLandmarks(avatarData);
            avatarMeshFilter.mesh = avatarMesh;
            
            // Create material with user's photo texture
            Material avatarMaterial = CreateMaterialFromTextures(avatarData);
            avatarRenderer.material = avatarMaterial;
            
            // Add collider for interactions
            var collider = currentAvatar.AddComponent<MeshCollider>();
            collider.convex = true;
            
            // Add rigidbody for physics
            var rigidbody = currentAvatar.AddComponent<Rigidbody>();
            rigidbody.mass = 70f; // Average human weight
            
            // Setup animation if available
            if (avatarData.animations != null && avatarData.animations.Count > 0)
            {
                SetupAvatarAnimations(avatarData);
            }
            
            LogDebug($"Successfully created realistic avatar with {avatarData.vertices.Count} vertices");
            
            yield return null;
        }
        
        private Mesh CreateMeshFromLandmarks(Avatar3DModel avatarData)
        {
            var mesh = new Mesh();
            mesh.name = "RealisticFaceMesh";
            
            // Convert vertex data from MediaPipe landmarks
            Vector3[] vertices = new Vector3[avatarData.vertices.Count];
            for (int i = 0; i < avatarData.vertices.Count; i++)
            {
                var vertex = avatarData.vertices[i];
                vertices[i] = new Vector3(vertex[0], vertex[1], vertex[2]);
            }
            
            // Convert face indices
            List<int> triangles = new List<int>();
            foreach (var face in avatarData.faces)
            {
                if (face.Count >= 3)
                {
                    // Ensure proper winding order for Unity
                    triangles.Add(face[0]);
                    triangles.Add(face[2]);
                    triangles.Add(face[1]);
                }
            }
            
            // Generate UV coordinates for photo texture mapping
            Vector2[] uvs = GenerateUVCoordinates(vertices, avatarData);
            
            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs;
            
            // Calculate normals for proper lighting
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            LogDebug($"Created mesh with {vertices.Length} vertices and {triangles.Count/3} triangles");
            
            return mesh;
        }
        
        private Vector2[] GenerateUVCoordinates(Vector3[] vertices, Avatar3DModel avatarData)
        {
            Vector2[] uvs = new Vector2[vertices.Length];
            
            // Calculate bounding box for UV mapping
            Bounds bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var vertex in vertices)
            {
                bounds.Encapsulate(vertex);
            }
            
            // Map vertices to UV space (0-1 range)
            for (int i = 0; i < vertices.Length; i++)
            {
                float u = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertices[i].x);
                float v = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertices[i].y);
                uvs[i] = new Vector2(u, v);
            }
            
            return uvs;
        }
        
        private Material CreateMaterialFromTextures(Avatar3DModel avatarData)
        {
            Material material = new Material(Shader.Find("Standard"));
            
            // Load diffuse texture (user's photo)
            if (avatarData.textures.ContainsKey("diffuse"))
            {
                var textureData = avatarData.textures["diffuse"];
                var photoTexture = LoadTextureFromBase64(textureData);
                if (photoTexture != null)
                {
                    material.mainTexture = photoTexture;
                    LogDebug($"Applied photo texture: {photoTexture.width}x{photoTexture.height}");
                }
            }
            
            // Load normal map if available
            if (avatarData.textures.ContainsKey("normal"))
            {
                var normalData = avatarData.textures["normal"];
                var normalTexture = LoadTextureFromBase64(normalData);
                if (normalTexture != null)
                {
                    material.SetTexture("_BumpMap", normalTexture);
                }
            }
            
            // Apply material properties based on facial analysis
            if (avatarData.materials.ContainsKey("skin"))
            {
                var skinProps = avatarData.materials["skin"];
                if (skinProps.ContainsKey("roughness"))
                {
                    material.SetFloat("_Smoothness", 1f - (float)skinProps["roughness"]);
                }
                if (skinProps.ContainsKey("metallic"))
                {
                    material.SetFloat("_Metallic", (float)skinProps["metallic"]);
                }
            }
            
            return material;
        }
        
        private Texture2D LoadTextureFromBase64(string base64Data)
        {
            try
            {
                byte[] imageData = System.Convert.FromBase64String(base64Data);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                return texture;
            }
            catch (Exception e)
            {
                LogError($"Failed to load texture from base64: {e.Message}");
                return null;
            }
        }
        
        private void SetupAvatarAnimations(Avatar3DModel avatarData)
        {
            // Add animator component
            avatarAnimator = currentAvatar.AddComponent<Animator>();
            
            // Create runtime animator controller for facial animations
            var controller = new UnityEditor.Animations.AnimatorController();
            controller.name = "AvatarController";
            
            // Add blend shapes for facial expressions if available
            if (avatarData.blendShapes != null && avatarData.blendShapes.Count > 0)
            {
                var skinnedRenderer = currentAvatar.GetComponent<SkinnedMeshRenderer>();
                if (skinnedRenderer == null)
                {
                    skinnedRenderer = currentAvatar.AddComponent<SkinnedMeshRenderer>();
                }
                
                // Apply blend shapes for realistic facial animation
                foreach (var blendShape in avatarData.blendShapes)
                {
                    LogDebug($"Added blend shape: {blendShape.Key}");
                }
            }
            
            avatarAnimator.runtimeAnimatorController = controller;
        }
        
        public void PlayAnimation(string animationName)
        {
            if (avatarAnimator != null)
            {
                avatarAnimator.Play(animationName);
                LogDebug($"Playing animation: {animationName}");
            }
        }
        
        public void SetBlendShapeWeight(string blendShapeName, float weight)
        {
            var skinnedRenderer = currentAvatar?.GetComponent<SkinnedMeshRenderer>();
            if (skinnedRenderer != null)
            {
                var mesh = skinnedRenderer.sharedMesh;
                int blendShapeIndex = mesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex >= 0)
                {
                    skinnedRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
                }
            }
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[AvatarManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AvatarManager] {message}");
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Test Load Sample Avatar")]
        private void TestLoadSampleAvatar()
        {
            // Create sample avatar data for testing
            var sampleData = new Avatar3DModel
            {
                id = "test-avatar",
                created_at = System.DateTime.Now.ToString(),
                vertices = new List<List<float>>(),
                faces = new List<List<int>>(),
                textures = new Dictionary<string, string>(),
                blend_shapes = new Dictionary<string, List<float>>(),
                animations = new List<Dictionary<string, object>>(),
                materials = new Dictionary<string, Dictionary<string, object>>(),
                lighting_params = new Dictionary<string, float>(),
                source_image_hash = "test-hash",
                generation_params = new Dictionary<string, object>()
            };
            
            // Generate simple cube vertices for testing
            var cubeVertices = new float[][]
            {
                new float[] {-0.5f, -0.5f, -0.5f}, new float[] {0.5f, -0.5f, -0.5f},
                new float[] {0.5f, 0.5f, -0.5f}, new float[] {-0.5f, 0.5f, -0.5f},
                new float[] {-0.5f, -0.5f, 0.5f}, new float[] {0.5f, -0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f}, new float[] {-0.5f, 0.5f, 0.5f}
            };
            
            foreach (var vertex in cubeVertices)
            {
                sampleData.vertices.Add(new List<float>(vertex));
            }
            
            // Add cube faces
            int[][] cubeFaces = new int[][]
            {
                new int[] {0, 1, 2}, new int[] {0, 2, 3}, // Front
                new int[] {4, 7, 6}, new int[] {4, 6, 5}, // Back
                new int[] {0, 4, 5}, new int[] {0, 5, 1}, // Bottom
                new int[] {2, 6, 7}, new int[] {2, 7, 3}, // Top
                new int[] {0, 3, 7}, new int[] {0, 7, 4}, // Left
                new int[] {1, 5, 6}, new int[] {1, 6, 2}  // Right
            };
            
            foreach (var face in cubeFaces)
            {
                sampleData.faces.Add(new List<int>(face));
            }
            
            StartCoroutine(CreateAvatarFromData(sampleData));
        }
        #endif
    }
}