using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MirrorWorld
{
    [Serializable]
    public class Avatar3DModel
    {
        [JsonProperty("id")]
        public string id;
        
        [JsonProperty("created_at")]
        public string created_at;
        
        [JsonProperty("vertices")]
        public List<List<float>> vertices;
        
        [JsonProperty("faces")]
        public List<List<int>> faces;
        
        [JsonProperty("textures")]
        public Dictionary<string, string> textures;
        
        [JsonProperty("blend_shapes")]
        public Dictionary<string, List<float>> blend_shapes;
        
        [JsonProperty("skeleton")]
        public Dictionary<string, object> skeleton;
        
        [JsonProperty("animations")]
        public List<Dictionary<string, object>> animations;
        
        [JsonProperty("materials")]
        public Dictionary<string, Dictionary<string, object>> materials;
        
        [JsonProperty("lighting_params")]
        public Dictionary<string, float> lighting_params;
        
        [JsonProperty("source_image_hash")]
        public string source_image_hash;
        
        [JsonProperty("generation_params")]
        public Dictionary<string, object> generation_params;
        
        public Avatar3DModel()
        {
            vertices = new List<List<float>>();
            faces = new List<List<int>>();
            textures = new Dictionary<string, string>();
            blend_shapes = new Dictionary<string, List<float>>();
            animations = new List<Dictionary<string, object>>();
            materials = new Dictionary<string, Dictionary<string, object>>();
            lighting_params = new Dictionary<string, float>();
            generation_params = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Validates the avatar data structure
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && 
                   vertices != null && vertices.Count > 0 &&
                   faces != null && faces.Count > 0;
        }
        
        /// <summary>
        /// Gets the total number of triangular faces
        /// </summary>
        public int GetTriangleCount()
        {
            int triangles = 0;
            foreach (var face in faces)
            {
                if (face.Count >= 3)
                    triangles++;
            }
            return triangles;
        }
        
        /// <summary>
        /// Gets the vertex count
        /// </summary>
        public int GetVertexCount()
        {
            return vertices?.Count ?? 0;
        }
        
        /// <summary>
        /// Checks if the avatar has photo texture data
        /// </summary>
        public bool HasPhotoTexture()
        {
            return textures != null && textures.ContainsKey("diffuse") && 
                   !string.IsNullOrEmpty(textures["diffuse"]);
        }
        
        /// <summary>
        /// Checks if the avatar has blend shapes for animation
        /// </summary>
        public bool HasBlendShapes()
        {
            return blend_shapes != null && blend_shapes.Count > 0;
        }
        
        /// <summary>
        /// Gets available texture types
        /// </summary>
        public string[] GetAvailableTextures()
        {
            if (textures == null) return new string[0];
            
            var textureTypes = new List<string>();
            foreach (var kvp in textures)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                    textureTypes.Add(kvp.Key);
            }
            return textureTypes.ToArray();
        }
        
        /// <summary>
        /// Gets available blend shape names
        /// </summary>
        public string[] GetBlendShapeNames()
        {
            if (blend_shapes == null) return new string[0];
            
            var names = new List<string>();
            foreach (var kvp in blend_shapes)
            {
                names.Add(kvp.Key);
            }
            return names.ToArray();
        }
        
        /// <summary>
        /// Creates a summary string for debugging
        /// </summary>
        public override string ToString()
        {
            return $"Avatar3DModel [ID: {id}, Vertices: {GetVertexCount()}, " +
                   $"Triangles: {GetTriangleCount()}, HasTexture: {HasPhotoTexture()}, " +
                   $"BlendShapes: {blend_shapes?.Count ?? 0}]";
        }
    }
    
    /// <summary>
    /// Utility class for avatar processing
    /// </summary>
    public static class AvatarUtilities
    {
        /// <summary>
        /// Converts a base64 texture string to a Unity Color array
        /// </summary>
        public static UnityEngine.Color[] ExtractColorsFromBase64Texture(string base64Data, int width, int height)
        {
            try
            {
                byte[] imageData = Convert.FromBase64String(base64Data);
                var texture = new UnityEngine.Texture2D(width, height);
                texture.LoadImage(imageData);
                return texture.GetPixels();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to extract colors from texture: {e.Message}");
                return new UnityEngine.Color[0];
            }
        }
        
        /// <summary>
        /// Validates vertex data for Unity mesh creation
        /// </summary>
        public static bool ValidateVertexData(List<List<float>> vertices)
        {
            if (vertices == null || vertices.Count == 0)
                return false;
                
            foreach (var vertex in vertices)
            {
                if (vertex == null || vertex.Count < 3)
                    return false;
                    
                // Check for invalid float values
                foreach (var component in vertex)
                {
                    if (float.IsNaN(component) || float.IsInfinity(component))
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Validates face indices for Unity mesh creation
        /// </summary>
        public static bool ValidateFaceData(List<List<int>> faces, int vertexCount)
        {
            if (faces == null || faces.Count == 0)
                return false;
                
            foreach (var face in faces)
            {
                if (face == null || face.Count < 3)
                    return false;
                    
                // Check if indices are within vertex range
                foreach (var index in face)
                {
                    if (index < 0 || index >= vertexCount)
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Calculates the bounding box of vertex data
        /// </summary>
        public static UnityEngine.Bounds CalculateBounds(List<List<float>> vertices)
        {
            if (vertices == null || vertices.Count == 0)
                return new UnityEngine.Bounds();
                
            var min = new UnityEngine.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new UnityEngine.Vector3(float.MinValue, float.MinValue, float.MinValue);
            
            foreach (var vertex in vertices)
            {
                if (vertex.Count >= 3)
                {
                    min.x = UnityEngine.Mathf.Min(min.x, vertex[0]);
                    min.y = UnityEngine.Mathf.Min(min.y, vertex[1]);
                    min.z = UnityEngine.Mathf.Min(min.z, vertex[2]);
                    
                    max.x = UnityEngine.Mathf.Max(max.x, vertex[0]);
                    max.y = UnityEngine.Mathf.Max(max.y, vertex[1]);
                    max.z = UnityEngine.Mathf.Max(max.z, vertex[2]);
                }
            }
            
            var center = (min + max) * 0.5f;
            var size = max - min;
            
            return new UnityEngine.Bounds(center, size);
        }
    }
}