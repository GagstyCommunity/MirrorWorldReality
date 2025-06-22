//
//  APIService.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import Foundation
import UIKit

class APIService {
    static let shared = APIService()
    
    // Configure your backend URL here
    private let baseURL = "https://your-backend-url.com"
    // For local development: "http://localhost:5000"
    
    private let session = URLSession.shared
    
    private init() {}
    
    // MARK: - Photo Upload
    
    func uploadPhoto(_ image: UIImage) async throws -> String {
        guard let url = URL(string: "\(baseURL)/api/upload-photo") else {
            throw APIError.invalidURL
        }
        
        guard let imageData = image.jpegData(compressionQuality: 0.8) else {
            throw APIError.imageProcessingFailed
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        
        let boundary = UUID().uuidString
        request.setValue("multipart/form-data; boundary=\(boundary)", forHTTPHeaderField: "Content-Type")
        
        let httpBody = createMultipartBody(imageData: imageData, boundary: boundary)
        request.httpBody = httpBody
        
        let (data, response) = try await session.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse,
              200...299 ~= httpResponse.statusCode else {
            throw APIError.serverError
        }
        
        let uploadResponse = try JSONDecoder().decode(UploadResponse.self, from: data)
        return uploadResponse.processId
    }
    
    // MARK: - Processing Status
    
    func getProcessingStatus(_ processId: String) async throws -> ProcessingResponse {
        guard let url = URL(string: "\(baseURL)/api/status/\(processId)") else {
            throw APIError.invalidURL
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        
        let (data, response) = try await session.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse,
              200...299 ~= httpResponse.statusCode else {
            throw APIError.serverError
        }
        
        return try JSONDecoder().decode(ProcessingResponse.self, from: data)
    }
    
    // MARK: - Avatar Data Retrieval
    
    func getAvatarData(_ processId: String) async throws -> Avatar3DModel {
        guard let url = URL(string: "\(baseURL)/api/avatar/\(processId)") else {
            throw APIError.invalidURL
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        
        let (data, response) = try await session.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse,
              200...299 ~= httpResponse.statusCode else {
            throw APIError.serverError
        }
        
        return try JSONDecoder().decode(Avatar3DModel.self, from: data)
    }
    
    // MARK: - Cleanup
    
    func cleanup(_ processId: String) async throws {
        guard let url = URL(string: "\(baseURL)/api/cleanup/\(processId)") else {
            throw APIError.invalidURL
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "DELETE"
        
        let (_, response) = try await session.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse,
              200...299 ~= httpResponse.statusCode else {
            throw APIError.serverError
        }
    }
    
    // MARK: - Helper Methods
    
    private func createMultipartBody(imageData: Data, boundary: String) -> Data {
        var body = Data()
        
        let boundaryPrefix = "--\(boundary)\r\n"
        
        body.append(boundaryPrefix.data(using: .utf8)!)
        body.append("Content-Disposition: form-data; name=\"file\"; filename=\"image.jpg\"\r\n".data(using: .utf8)!)
        body.append("Content-Type: image/jpeg\r\n\r\n".data(using: .utf8)!)
        body.append(imageData)
        body.append("\r\n".data(using: .utf8)!)
        body.append("--\(boundary)--\r\n".data(using: .utf8)!)
        
        return body
    }
}

// MARK: - API Response Models

struct UploadResponse: Codable {
    let processId: String
    let status: String
    let message: String
    
    enum CodingKeys: String, CodingKey {
        case processId = "process_id"
        case status
        case message
    }
}

// MARK: - API Errors

enum APIError: LocalizedError {
    case invalidURL
    case imageProcessingFailed
    case serverError
    case networkError
    case decodingError
    
    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Invalid URL"
        case .imageProcessingFailed:
            return "Failed to process image"
        case .serverError:
            return "Server error occurred"
        case .networkError:
            return "Network error occurred"
        case .decodingError:
            return "Failed to decode response"
        }
    }
}