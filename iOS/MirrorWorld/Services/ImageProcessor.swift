//
//  ImageProcessor.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import Foundation
import UIKit
import CoreImage
import Vision

class ImageProcessor {
    static let shared = ImageProcessor()
    
    private init() {}
    
    // MARK: - Image Processing
    
    func processImageForUpload(_ image: UIImage) async -> UIImage? {
        return await withCheckedContinuation { continuation in
            DispatchQueue.global(qos: .userInitiated).async {
                let processedImage = self.enhanceImage(image)
                continuation.resume(returning: processedImage)
            }
        }
    }
    
    func detectFace(in image: UIImage) async -> Bool {
        return await withCheckedContinuation { continuation in
            guard let cgImage = image.cgImage else {
                continuation.resume(returning: false)
                return
            }
            
            let request = VNDetectFaceRectanglesRequest { request, error in
                if let results = request.results as? [VNFaceObservation] {
                    continuation.resume(returning: !results.isEmpty)
                } else {
                    continuation.resume(returning: false)
                }
            }
            
            let handler = VNImageRequestHandler(cgImage: cgImage, options: [:])
            
            do {
                try handler.perform([request])
            } catch {
                continuation.resume(returning: false)
            }
        }
    }
    
    func getFaceBounds(in image: UIImage) async -> CGRect? {
        return await withCheckedContinuation { continuation in
            guard let cgImage = image.cgImage else {
                continuation.resume(returning: nil)
                return
            }
            
            let request = VNDetectFaceRectanglesRequest { request, error in
                if let results = request.results as? [VNFaceObservation],
                   let firstFace = results.first {
                    let bounds = firstFace.boundingBox
                    // Convert normalized coordinates to image coordinates
                    let imageRect = CGRect(
                        x: bounds.origin.x * CGFloat(cgImage.width),
                        y: (1 - bounds.origin.y - bounds.height) * CGFloat(cgImage.height),
                        width: bounds.width * CGFloat(cgImage.width),
                        height: bounds.height * CGFloat(cgImage.height)
                    )
                    continuation.resume(returning: imageRect)
                } else {
                    continuation.resume(returning: nil)
                }
            }
            
            let handler = VNImageRequestHandler(cgImage: cgImage, options: [:])
            
            do {
                try handler.perform([request])
            } catch {
                continuation.resume(returning: nil)
            }
        }
    }
    
    // MARK: - Image Enhancement
    
    private func enhanceImage(_ image: UIImage) -> UIImage? {
        guard let cgImage = image.cgImage else { return image }
        
        let context = CIContext()
        let ciImage = CIImage(cgImage: cgImage)
        
        // Apply enhancement filters
        var enhancedImage = ciImage
        
        // 1. Auto enhance
        if let autoEnhanceFilter = CIFilter(name: "CIColorControls") {
            autoEnhanceFilter.setValue(enhancedImage, forKey: kCIInputImageKey)
            autoEnhanceFilter.setValue(1.1, forKey: kCIInputContrastKey)
            autoEnhanceFilter.setValue(1.05, forKey: kCIInputSaturationKey)
            autoEnhanceFilter.setValue(0.1, forKey: kCIInputBrightnessKey)
            
            if let output = autoEnhanceFilter.outputImage {
                enhancedImage = output
            }
        }
        
        // 2. Sharpen
        if let sharpenFilter = CIFilter(name: "CISharpenLuminance") {
            sharpenFilter.setValue(enhancedImage, forKey: kCIInputImageKey)
            sharpenFilter.setValue(0.4, forKey: kCIInputSharpnessKey)
            
            if let output = sharpenFilter.outputImage {
                enhancedImage = output
            }
        }
        
        // 3. Noise reduction
        if let noiseFilter = CIFilter(name: "CINoiseReduction") {
            noiseFilter.setValue(enhancedImage, forKey: kCIInputImageKey)
            noiseFilter.setValue(0.02, forKey: kCIInputNoiseReductionKey)
            noiseFilter.setValue(0.4, forKey: kCIInputSharpnessKey)
            
            if let output = noiseFilter.outputImage {
                enhancedImage = output
            }
        }
        
        // Convert back to UIImage
        guard let outputCGImage = context.createCGImage(enhancedImage, from: enhancedImage.extent) else {
            return image
        }
        
        return UIImage(cgImage: outputCGImage, scale: image.scale, orientation: image.imageOrientation)
    }
    
    // MARK: - Image Validation
    
    func validateImage(_ image: UIImage) -> ImageValidationResult {
        var issues: [ImageValidationIssue] = []
        var score: Float = 1.0
        
        // Check image size
        let minSize: CGFloat = 512
        if min(image.size.width, image.size.height) < minSize {
            issues.append(.tooSmall)
            score -= 0.3
        }
        
        // Check image quality
        if let cgImage = image.cgImage {
            let pixels = cgImage.width * cgImage.height
            if pixels < 262144 { // Less than 512x512
                issues.append(.lowQuality)
                score -= 0.2
            }
        }
        
        // Check brightness
        let brightness = calculateBrightness(image)
        if brightness < 0.2 {
            issues.append(.tooDark)
            score -= 0.2
        } else if brightness > 0.9 {
            issues.append(.tooBlright)
            score -= 0.2
        }
        
        // Check blur
        let blurLevel = calculateBlurLevel(image)
        if blurLevel > 0.7 {
            issues.append(.blurry)
            score -= 0.3
        }
        
        let quality: ImageQuality
        if score >= 0.8 {
            quality = .excellent
        } else if score >= 0.6 {
            quality = .good
        } else if score >= 0.4 {
            quality = .fair
        } else {
            quality = .poor
        }
        
        return ImageValidationResult(quality: quality, score: score, issues: issues)
    }
    
    // MARK: - Helper Methods
    
    private func calculateBrightness(_ image: UIImage) -> Float {
        guard let cgImage = image.cgImage else { return 0.5 }
        
        let ciImage = CIImage(cgImage: cgImage)
        let extentVector = CIVector(x: ciImage.extent.origin.x,
                                  y: ciImage.extent.origin.y,
                                  z: ciImage.extent.size.width,
                                  w: ciImage.extent.size.height)
        
        guard let filter = CIFilter(name: "CIAreaAverage") else { return 0.5 }
        filter.setValue(ciImage, forKey: kCIInputImageKey)
        filter.setValue(extentVector, forKey: kCIInputExtentKey)
        
        guard let outputImage = filter.outputImage else { return 0.5 }
        
        var bitmap = [UInt8](repeating: 0, count: 4)
        let context = CIContext()
        
        context.render(outputImage,
                      toBitmap: &bitmap,
                      rowBytes: 4,
                      bounds: CGRect(x: 0, y: 0, width: 1, height: 1),
                      format: .RGBA8,
                      colorSpace: nil)
        
        let brightness = (Float(bitmap[0]) + Float(bitmap[1]) + Float(bitmap[2])) / (3.0 * 255.0)
        return brightness
    }
    
    private func calculateBlurLevel(_ image: UIImage) -> Float {
        guard let cgImage = image.cgImage else { return 0.0 }
        
        let ciImage = CIImage(cgImage: cgImage)
        
        // Use Laplacian filter to detect edges (blur detection)
        let laplacianKernel: [Float] = [
            0, -1, 0,
            -1, 4, -1,
            0, -1, 0
        ]
        
        let kernel = CIKernel(source:
            "kernel vec4 laplacian(sampler image) {" +
            "  vec2 d = destCoord();" +
            "  vec4 sum = vec4(0.0);" +
            "  sum += sample(image, d + vec2(-1, -1)) * 0.0;" +
            "  sum += sample(image, d + vec2(0, -1)) * -1.0;" +
            "  sum += sample(image, d + vec2(1, -1)) * 0.0;" +
            "  sum += sample(image, d + vec2(-1, 0)) * -1.0;" +
            "  sum += sample(image, d + vec2(0, 0)) * 4.0;" +
            "  sum += sample(image, d + vec2(1, 0)) * -1.0;" +
            "  sum += sample(image, d + vec2(-1, 1)) * 0.0;" +
            "  sum += sample(image, d + vec2(0, 1)) * -1.0;" +
            "  sum += sample(image, d + vec2(1, 1)) * 0.0;" +
            "  return sum;" +
            "}"
        )
        
        // Simplified blur detection using variance
        let filter = CIFilter(name: "CIAreaMaximum")
        filter?.setValue(ciImage, forKey: kCIInputImageKey)
        
        // Return a normalized blur level (0.0 = sharp, 1.0 = very blurry)
        return 0.3 // Placeholder - in real implementation would calculate actual variance
    }
}

// MARK: - Supporting Types

struct ImageValidationResult {
    let quality: ImageQuality
    let score: Float
    let issues: [ImageValidationIssue]
    
    var isAcceptable: Bool {
        return quality != .poor && !issues.contains(.tooSmall)
    }
    
    var recommendations: [String] {
        var recs: [String] = []
        
        for issue in issues {
            switch issue {
            case .tooSmall:
                recs.append("Use a higher resolution image (at least 512x512)")
            case .lowQuality:
                recs.append("Choose a clearer, higher quality photo")
            case .tooDark:
                recs.append("Use better lighting or brighten the image")
            case .tooBlright:
                recs.append("Reduce exposure or use softer lighting")
            case .blurry:
                recs.append("Use a sharper, less blurry photo")
            case .noFaceDetected:
                recs.append("Make sure your face is clearly visible in the photo")
            }
        }
        
        return recs
    }
}

enum ImageQuality {
    case excellent
    case good
    case fair
    case poor
    
    var description: String {
        switch self {
        case .excellent:
            return "Excellent quality - perfect for 3D avatar generation"
        case .good:
            return "Good quality - will produce great results"
        case .fair:
            return "Fair quality - results may vary"
        case .poor:
            return "Poor quality - consider using a different photo"
        }
    }
}

enum ImageValidationIssue {
    case tooSmall
    case lowQuality
    case tooDark
    case tooBlright
    case blurry
    case noFaceDetected
}