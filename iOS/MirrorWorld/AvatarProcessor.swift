import Foundation
import UIKit
import Combine

class AvatarProcessor: ObservableObject {
    private let baseURL = "http://your-server.com/api" // Replace with your server URL
    private var cancellables = Set<AnyCancellable>()
    
    @Published var isProcessing = false
    @Published var progress: Double = 0.0
    
    func processImage(_ image: UIImage) async throws -> Avatar3DModel {
        isProcessing = true
        defer { isProcessing = false }
        
        // Convert image to base64
        guard let imageData = image.jpegData(compressionQuality: 0.8) else {
            throw ProcessingError.invalidImage
        }
        
        let base64String = imageData.base64EncodedString()
        
        // Create processing request
        let request = ProcessingRequest(
            imageData: base64String,
            userPreferences: nil,
            qualityLevel: "high"
        )
        
        // Send to server
        let processId = try await uploadPhoto(request)
        
        // Monitor processing
        let avatar = try await monitorProcessing(processId: processId)
        
        return avatar
    }
    
    private func uploadPhoto(_ request: ProcessingRequest) async throws -> String {
        guard let url = URL(string: "\(baseURL)/upload-photo") else {
            throw ProcessingError.invalidURL
        }
        
        var urlRequest = URLRequest(url: url)
        urlRequest.httpMethod = "POST"
        urlRequest.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let encoder = JSONEncoder()
        urlRequest.httpBody = try encoder.encode(request)
        
        let (data, response) = try await URLSession.shared.data(for: urlRequest)
        
        guard let httpResponse = response as? HTTPURLResponse,
              httpResponse.statusCode == 200 else {
            throw ProcessingError.serverError
        }
        
        let uploadResponse = try JSONDecoder().decode(UploadResponse.self, from: data)
        return uploadResponse.processId
    }
    
    private func monitorProcessing(processId: String) async throws -> Avatar3DModel {
        guard let url = URL(string: "\(baseURL)/status/\(processId)") else {
            throw ProcessingError.invalidURL
        }
        
        while true {
            let (data, _) = try await URLSession.shared.data(from: url)
            let response = try JSONDecoder().decode(ProcessingResponse.self, from: data)
            
            await MainActor.run {
                self.progress = Double(response.progress) / 100.0
            }
            
            switch response.status {
            case .completed:
                guard let avatarData = response.avatarData else {
                    throw ProcessingError.noAvatarData
                }
                return avatarData
                
            case .failed:
                throw ProcessingError.processingFailed(response.message)
                
            case .processing:
                // Wait and continue monitoring
                try await Task.sleep(nanoseconds: 1_000_000_000) // 1 second
                continue
            }
        }
    }
}

// MARK: - Models
struct ProcessingRequest: Codable {
    let imageData: String
    let userPreferences: [String: Any]?
    let qualityLevel: String
    
    enum CodingKeys: String, CodingKey {
        case imageData = "image_data"
        case userPreferences = "user_preferences"
        case qualityLevel = "quality_level"
    }
    
    func encode(to encoder: Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(imageData, forKey: .imageData)
        try container.encode(qualityLevel, forKey: .qualityLevel)
        // Skip userPreferences for now as it's complex to encode [String: Any]
    }
}

struct UploadResponse: Codable {
    let processId: String
    let status: String
    let message: String
    
    enum CodingKeys: String, CodingKey {
        case processId = "process_id"
        case status, message
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
        case status, progress, message
        case avatarData = "avatar_data"
        case estimatedTime = "estimated_time"
    }
}

enum ProcessingStatus: String, Codable {
    case processing = "processing"
    case completed = "completed"
    case failed = "failed"
}

struct Avatar3DModel: Codable, Identifiable {
    let id: String
    let createdAt: String
    let vertices: [[Float]]
    let faces: [[Int]]
    let textures: [String: String]
    let blendShapes: [String: [Float]]
    let skeleton: [String: Any]?
    let animations: [[String: Any]]
    let materials: [String: [String: Any]]
    let lightingParams: [String: Float]
    let sourceImageHash: String
    let generationParams: [String: Any]
    
    enum CodingKeys: String, CodingKey {
        case id, vertices, faces, textures, animations, materials
        case createdAt = "created_at"
        case blendShapes = "blend_shapes"
        case skeleton, lightingParams = "lighting_params"
        case sourceImageHash = "source_image_hash"
        case generationParams = "generation_params"
    }
    
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        id = try container.decode(String.self, forKey: .id)
        createdAt = try container.decode(String.self, forKey: .createdAt)
        vertices = try container.decode([[Float]].self, forKey: .vertices)
        faces = try container.decode([[Int]].self, forKey: .faces)
        textures = try container.decode([String: String].self, forKey: .textures)
        blendShapes = try container.decode([String: [Float]].self, forKey: .blendShapes)
        animations = try container.decode([[String: Any]].self, forKey: .animations)
        materials = try container.decode([String: [String: Any]].self, forKey: .materials)
        lightingParams = try container.decode([String: Float].self, forKey: .lightingParams)
        sourceImageHash = try container.decode(String.self, forKey: .sourceImageHash)
        generationParams = try container.decode([String: Any].self, forKey: .generationParams)
        
        // skeleton is optional
        skeleton = try container.decodeIfPresent([String: Any].self, forKey: .skeleton)
    }
}

enum ProcessingError: Error, LocalizedError {
    case invalidImage
    case invalidURL
    case serverError
    case noAvatarData
    case processingFailed(String)
    
    var errorDescription: String? {
        switch self {
        case .invalidImage:
            return "Invalid image format"
        case .invalidURL:
            return "Invalid server URL"
        case .serverError:
            return "Server error occurred"
        case .noAvatarData:
            return "No avatar data received"
        case .processingFailed(let message):
            return "Processing failed: \(message)"
        }
    }
}