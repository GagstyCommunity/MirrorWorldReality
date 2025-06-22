//
//  CameraView.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI
import PhotosUI
import AVFoundation

struct CameraView: View {
    @EnvironmentObject var appState: AppState
    @StateObject private var cameraManager = CameraManager()
    @State private var selectedPhotoItem: PhotosPickerItem?
    @State private var showingImagePicker = false
    @State private var showingCamera = false
    @State private var capturedImage: UIImage?
    
    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                Button(action: {
                    appState.navigateTo(.onboarding)
                }) {
                    Image(systemName: "chevron.left")
                        .font(.title2)
                        .foregroundColor(.white)
                }
                Spacer()
            }
            .padding(.horizontal, 24)
            .padding(.top, 20)
            
            Spacer().frame(height: 40)
            
            // Main content
            VStack(spacing: 32) {
                Text("Upload Your Photo")
                    .font(.largeTitle)
                    .fontWeight(.bold)
                    .foregroundColor(.white)
                
                // Photo preview or placeholder
                if let image = capturedImage ?? appState.selectedImage {
                    VStack(spacing: 20) {
                        Image(uiImage: image)
                            .resizable()
                            .aspectRatio(contentMode: .fill)
                            .frame(width: 250, height: 250)
                            .clipped()
                            .cornerRadius(20)
                            .overlay(
                                RoundedRectangle(cornerRadius: 20)
                                    .stroke(Color.white, lineWidth: 3)
                            )
                            .shadow(color: .black.opacity(0.3), radius: 10, x: 0, y: 5)
                        
                        Button(action: {
                            processSelectedImage(image)
                        }) {
                            HStack(spacing: 12) {
                                Image(systemName: "sparkles")
                                    .font(.title2)
                                Text("Create My Avatar")
                                    .font(.title2)
                                    .fontWeight(.semibold)
                            }
                            .foregroundColor(.white)
                            .frame(maxWidth: .infinity)
                            .frame(height: 56)
                            .background(
                                RoundedRectangle(cornerRadius: 28)
                                    .fill(
                                        LinearGradient(
                                            gradient: Gradient(colors: [
                                                Color.purple,
                                                Color.blue
                                            ]),
                                            startPoint: .leading,
                                            endPoint: .trailing
                                        )
                                    )
                            )
                        }
                        .buttonStyle(ScaleButtonStyle())
                    }
                } else {
                    VStack(spacing: 24) {
                        // Camera placeholder
                        RoundedRectangle(cornerRadius: 20)
                            .fill(Color.white.opacity(0.1))
                            .frame(width: 250, height: 250)
                            .overlay(
                                VStack(spacing: 16) {
                                    Image(systemName: "camera.fill")
                                        .font(.system(size: 48))
                                        .foregroundColor(.white.opacity(0.6))
                                    
                                    Text("Choose a photo")
                                        .font(.title2)
                                        .fontWeight(.semibold)
                                        .foregroundColor(.white)
                                    
                                    Text("Select a clear photo where your face is visible")
                                        .font(.body)
                                        .foregroundColor(.white.opacity(0.8))
                                        .multilineTextAlignment(.center)
                                        .padding(.horizontal, 20)
                                }
                            )
                            .overlay(
                                RoundedRectangle(cornerRadius: 20)
                                    .stroke(Color.white.opacity(0.3), lineWidth: 2, linecap: .round, dash: [10, 5])
                            )
                    }
                }
                
                // Action buttons
                if capturedImage == nil && appState.selectedImage == nil {
                    VStack(spacing: 16) {
                        Button(action: {
                            showingCamera = true
                        }) {
                            HStack(spacing: 12) {
                                Image(systemName: "camera.fill")
                                    .font(.title2)
                                Text("Take Photo")
                                    .font(.title2)
                                    .fontWeight(.semibold)
                            }
                            .foregroundColor(.white)
                            .frame(maxWidth: .infinity)
                            .frame(height: 56)
                            .background(
                                RoundedRectangle(cornerRadius: 28)
                                    .fill(Color.black.opacity(0.2))
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 28)
                                            .stroke(Color.white.opacity(0.3), lineWidth: 1)
                                    )
                            )
                        }
                        .buttonStyle(ScaleButtonStyle())
                        
                        PhotosPicker(selection: $selectedPhotoItem, matching: .images) {
                            HStack(spacing: 12) {
                                Image(systemName: "photo.fill")
                                    .font(.title2)
                                Text("Choose from Gallery")
                                    .font(.title2)
                                    .fontWeight(.semibold)
                            }
                            .foregroundColor(.white)
                            .frame(maxWidth: .infinity)
                            .frame(height: 56)
                            .background(
                                RoundedRectangle(cornerRadius: 28)
                                    .fill(Color.black.opacity(0.2))
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 28)
                                            .stroke(Color.white.opacity(0.3), lineWidth: 1)
                                    )
                            )
                        }
                        .buttonStyle(ScaleButtonStyle())
                    }
                    .padding(.horizontal, 32)
                }
            }
            
            Spacer()
        }
        .sheet(isPresented: $showingCamera) {
            CameraViewController { image in
                capturedImage = image
                showingCamera = false
            }
            .ignoresSafeArea()
        }
        .onChange(of: selectedPhotoItem) { item in
            Task {
                if let item = item,
                   let data = try? await item.loadTransferable(type: Data.self),
                   let image = UIImage(data: data) {
                    await MainActor.run {
                        capturedImage = image
                    }
                }
            }
        }
    }
    
    private func processSelectedImage(_ image: UIImage) {
        Task {
            do {
                appState.setImage(image)
                let processId = try await APIService.shared.uploadPhoto(image)
                appState.startProcessing(with: processId)
                
                // Start monitoring processing status
                await monitorProcessingStatus(processId)
            } catch {
                appState.showError("Failed to upload photo: \(error.localizedDescription)")
            }
        }
    }
    
    private func monitorProcessingStatus(_ processId: String) async {
        while appState.isProcessing {
            do {
                let response = try await APIService.shared.getProcessingStatus(processId)
                
                await MainActor.run {
                    appState.updateProcessing(
                        progress: Double(response.progress) / 100.0,
                        message: response.message
                    )
                }
                
                if response.status == .completed, let avatarData = response.avatarData {
                    await MainActor.run {
                        appState.completeProcessing(with: avatarData)
                    }
                    break
                } else if response.status == .failed {
                    await MainActor.run {
                        appState.showError(response.message)
                    }
                    break
                }
                
                try await Task.sleep(nanoseconds: 1_000_000_000) // 1 second
            } catch {
                await MainActor.run {
                    appState.showError("Failed to check processing status: \(error.localizedDescription)")
                }
                break
            }
        }
    }
}

// Camera Manager for handling camera permissions and state
class CameraManager: ObservableObject {
    @Published var isAuthorized = false
    @Published var authorizationStatus: AVAuthorizationStatus = .notDetermined
    
    init() {
        checkCameraAuthorization()
    }
    
    func checkCameraAuthorization() {
        authorizationStatus = AVCaptureDevice.authorizationStatus(for: .video)
        isAuthorized = authorizationStatus == .authorized
        
        if authorizationStatus == .notDetermined {
            AVCaptureDevice.requestAccess(for: .video) { granted in
                DispatchQueue.main.async {
                    self.authorizationStatus = granted ? .authorized : .denied
                    self.isAuthorized = granted
                }
            }
        }
    }
}

// Camera View Controller for capturing photos
struct CameraViewController: UIViewControllerRepresentable {
    let onImageCaptured: (UIImage) -> Void
    
    func makeUIViewController(context: Context) -> UIImagePickerController {
        let picker = UIImagePickerController()
        picker.sourceType = .camera
        picker.delegate = context.coordinator
        return picker
    }
    
    func updateUIViewController(_ uiViewController: UIImagePickerController, context: Context) {}
    
    func makeCoordinator() -> Coordinator {
        Coordinator(self)
    }
    
    class Coordinator: NSObject, UIImagePickerControllerDelegate, UINavigationControllerDelegate {
        let parent: CameraViewController
        
        init(_ parent: CameraViewController) {
            self.parent = parent
        }
        
        func imagePickerController(_ picker: UIImagePickerController, didFinishPickingMediaWithInfo info: [UIImagePickerController.InfoKey : Any]) {
            if let image = info[.originalImage] as? UIImage {
                parent.onImageCaptured(image)
            }
            picker.dismiss(animated: true)
        }
        
        func imagePickerControllerDidCancel(_ picker: UIImagePickerController) {
            picker.dismiss(animated: true)
        }
    }
}

#Preview {
    CameraView()
        .environmentObject(AppState())
}