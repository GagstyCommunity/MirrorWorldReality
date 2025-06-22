//
//  Models.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import Foundation

// MARK: - Processing Status

enum ProcessingStatus: String, Codable {
    case processing = "processing"
    case completed = "completed"
    case failed = "failed"
}

// MARK: - 3D Avatar Model

struct Avatar3DModel: Codable {
    let id: String
    let createdAt: String
    let vertices: [[Float]]
    let faces: [[Int]]
    let textures: [String: String] // Base64 encoded textures
    let blendShapes: [String: [Float]]
    let skeleton: [String: Any]?
    let animations: [AnimationSequence]
    let materials: [String: [String: Any]]
    let lightingParams: [String: Float]
    let sourceImageHash: String
    let generationParams: [String: Any]
    
    enum CodingKeys: String, CodingKey {
        case id
        case createdAt = "created_at"
        case vertices
        case faces
        case textures
        case blendShapes = "blend_shapes"
        case skeleton
        case animations
        case materials
        case lightingParams = "lighting_params"
        case sourceImageHash = "source_image_hash"
        case generationParams = "generation_params"
    }
}

// MARK: - Animation Sequence

struct AnimationSequence: Codable {
    let name: String
    let duration: Float
    let loop: Bool
    let keyframes: [AnimationKeyframe]
}

struct AnimationKeyframe: Codable {
    let time: Float
    let blendWeights: [String: Float]?
    let eyeScale: [String: Float]?
    let headRotation: [String: Float]?
    
    enum CodingKeys: String, CodingKey {
        case time
        case blendWeights = "blend_weights"
        case eyeScale = "eye_scale"
        case headRotation = "head_rotation"
    }
}

// MARK: - Processing Request/Response

struct ProcessingRequest: Codable {
    let imageData: String // Base64 encoded image
    let userPreferences: [String: Any]?
    let qualityLevel: String
    
    enum CodingKeys: String, CodingKey {
        case imageData = "image_data"
        case userPreferences = "user_preferences"
        case qualityLevel = "quality_level"
    }
}

struct ProcessingResponse: Codable {
    let processId: String
    let status: ProcessingStatus
    let progress: Int
    let message: String
    let avatarData: Avatar3DModel?
    let estimatedTime: Int?
    
    enum CodingKeys: String, CodingKey {
        case processId = "process_id"
        case status
        case progress
        case message
        case avatarData = "avatar_data"
        case estimatedTime = "estimated_time"
    }
}

// MARK: - Avatar Metrics

struct AvatarMetrics: Codable {
    let geometryQuality: Float
    let textureResolution: Int
    let animationSmoothness: Float
    let renderingPerformance: Float
    
    enum CodingKeys: String, CodingKey {
        case geometryQuality = "geometry_quality"
        case textureResolution = "texture_resolution"
        case animationSmoothness = "animation_smoothness"
        case renderingPerformance = "rendering_performance"
    }
}

// MARK: - User Feedback

struct UserFeedback: Codable {
    let processId: String
    let rating: Int // 1-5 stars
    let comments: String?
    let issues: [String]
    
    enum CodingKeys: String, CodingKey {
        case processId = "process_id"
        case rating
        case comments
        case issues
    }
}

// MARK: - Unity Communication Models

struct UnityAvatarData: Codable {
    let meshData: UnityMeshData
    let textureData: UnityTextureData
    let animationData: UnityAnimationData
    let materialData: UnityMaterialData
}

struct UnityMeshData: Codable {
    let vertices: [Vector3]
    let triangles: [Int]
    let normals: [Vector3]
    let uvs: [Vector2]
}

struct UnityTextureData: Codable {
    let diffuseTexture: String // Base64
    let normalTexture: String // Base64
    let specularTexture: String // Base64
}

struct UnityAnimationData: Codable {
    let blendShapes: [String: [Float]]
    let animationClips: [UnityAnimationClip]
}

struct UnityAnimationClip: Codable {
    let name: String
    let length: Float
    let wrapMode: String
    let curves: [UnityAnimationCurve]
}

struct UnityAnimationCurve: Codable {
    let propertyName: String
    let keyframes: [UnityKeyframe]
}

struct UnityKeyframe: Codable {
    let time: Float
    let value: Float
    let inTangent: Float
    let outTangent: Float
}

struct UnityMaterialData: Codable {
    let materialName: String
    let shaderName: String
    let properties: [String: Any]
}

// MARK: - Vector Types for Unity

struct Vector3: Codable {
    let x: Float
    let y: Float
    let z: Float
}

struct Vector2: Codable {
    let x: Float
    let y: Float
}

// MARK: - App State Models

struct CameraPermissionState {
    let isAuthorized: Bool
    let authorizationStatus: String
    let hasAskedPermission: Bool
}

struct PhotoLibraryPermissionState {
    let isAuthorized: Bool
    let authorizationStatus: String
    let hasAskedPermission: Bool
}

// MARK: - Error Models

enum MirrorWorldError: LocalizedError {
    case noInternetConnection
    case serverUnavailable
    case invalidImage
    case processingFailed(String)
    case unityInitializationFailed
    case permissionDenied(String)
    case unexpectedError(String)
    
    var errorDescription: String? {
        switch self {
        case .noInternetConnection:
            return "No internet connection available"
        case .serverUnavailable:
            return "Server is currently unavailable"
        case .invalidImage:
            return "The selected image is not valid"
        case .processingFailed(let message):
            return "Processing failed: \(message)"
        case .unityInitializationFailed:
            return "Failed to initialize 3D environment"
        case .permissionDenied(let permission):
            return "\(permission) permission is required"
        case .unexpectedError(let message):
            return "An unexpected error occurred: \(message)"
        }
    }
}

// MARK: - Configuration Models

struct AppConfiguration {
    let apiBaseURL: String
    let maxImageSize: Int
    let supportedImageFormats: [String]
    let maxProcessingTime: TimeInterval
    let enableAnalytics: Bool
    let debugMode: Bool
    
    static let `default` = AppConfiguration(
        apiBaseURL: "https://api.mirrorworld.app",
        maxImageSize: 10 * 1024 * 1024, // 10MB
        supportedImageFormats: ["jpg", "jpeg", "png", "heic"],
        maxProcessingTime: 300, // 5 minutes
        enableAnalytics: false,
        debugMode: false
    )
}

// MARK: - Analytics Models

struct AnalyticsEvent {
    let eventName: String
    let parameters: [String: Any]
    let timestamp: Date
    let userId: String?
}

// MARK: - Extensions for Convenience

extension Avatar3DModel {
    func toUnityFormat() -> UnityAvatarData {
        // Convert vertices to Unity Vector3 format
        let unityVertices = vertices.map { vertex in
            Vector3(x: vertex[0], y: vertex[1], z: vertex[2])
        }
        
        // Convert triangles (faces) to Unity format
        let unityTriangles = faces.flatMap { $0 }
        
        // Generate normals (simplified)
        let unityNormals = vertices.map { _ in
            Vector3(x: 0, y: 0, z: 1) // Placeholder normal
        }
        
        // Generate UVs (simplified)
        let unityUVs = vertices.map { vertex in
            Vector2(x: (vertex[0] + 1.0) / 2.0, y: (vertex[1] + 1.0) / 2.0)
        }
        
        let meshData = UnityMeshData(
            vertices: unityVertices,
            triangles: unityTriangles,
            normals: unityNormals,
            uvs: unityUVs
        )
        
        let textureData = UnityTextureData(
            diffuseTexture: textures["diffuse"] ?? "",
            normalTexture: textures["normal"] ?? "",
            specularTexture: textures["specular"] ?? ""
        )
        
        // Convert animations to Unity format
        let unityAnimationClips = animations.map { animation in
            let curves = animation.keyframes.compactMap { keyframe -> UnityAnimationCurve? in
                guard let blendWeights = keyframe.blendWeights else { return nil }
                
                let unityKeyframes = blendWeights.map { (_, value) in
                    UnityKeyframe(
                        time: keyframe.time,
                        value: value,
                        inTangent: 0,
                        outTangent: 0
                    )
                }
                
                return UnityAnimationCurve(
                    propertyName: "blendShape.weight",
                    keyframes: unityKeyframes
                )
            }
            
            return UnityAnimationClip(
                name: animation.name,
                length: animation.duration,
                wrapMode: animation.loop ? "Loop" : "Once",
                curves: curves
            )
        }
        
        let animationData = UnityAnimationData(
            blendShapes: blendShapes,
            animationClips: unityAnimationClips
        )
        
        let materialData = UnityMaterialData(
            materialName: "AvatarMaterial",
            shaderName: "Standard",
            properties: materials.first?.value ?? [:]
        )
        
        return UnityAvatarData(
            meshData: meshData,
            textureData: textureData,
            animationData: animationData,
            materialData: materialData
        )
    }
}