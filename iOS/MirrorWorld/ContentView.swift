import SwiftUI
import AVFoundation
import PhotosUI

struct ContentView: View {
    @StateObject private var avatarProcessor = AvatarProcessor()
    @State private var selectedImage: UIImage?
    @State private var showingImagePicker = false
    @State private var showingCamera = false
    @State private var isProcessing = false
    @State private var processedAvatar: Avatar3DModel?
    @State private var showingUnityView = false
    
    var body: some View {
        NavigationView {
            VStack(spacing: 20) {
                // Header
                VStack {
                    Text("MirrorWorld")
                        .font(.largeTitle)
                        .fontWeight(.bold)
                        .foregroundColor(.primary)
                    
                    Text("Transform your photo into a 3D avatar")
                        .font(.subheadline)
                        .foregroundColor(.secondary)
                }
                .padding(.top)
                
                // Image Display
                Group {
                    if let image = selectedImage {
                        Image(uiImage: image)
                            .resizable()
                            .aspectRatio(contentMode: .fill)
                            .frame(width: 200, height: 200)
                            .clipShape(Circle())
                            .overlay(Circle().stroke(Color.blue, lineWidth: 3))
                    } else {
                        Circle()
                            .fill(Color.gray.opacity(0.3))
                            .frame(width: 200, height: 200)
                            .overlay(
                                Image(systemName: "person.crop.circle.fill")
                                    .font(.system(size: 80))
                                    .foregroundColor(.gray)
                            )
                    }
                }
                
                // Action Buttons
                VStack(spacing: 15) {
                    HStack(spacing: 20) {
                        Button(action: { showingCamera = true }) {
                            Label("Take Photo", systemImage: "camera")
                                .font(.headline)
                                .foregroundColor(.white)
                                .frame(maxWidth: .infinity)
                                .padding()
                                .background(Color.blue)
                                .cornerRadius(12)
                        }
                        
                        Button(action: { showingImagePicker = true }) {
                            Label("Choose Photo", systemImage: "photo.on.rectangle")
                                .font(.headline)
                                .foregroundColor(.white)
                                .frame(maxWidth: .infinity)
                                .padding()
                                .background(Color.green)
                                .cornerRadius(12)
                        }
                    }
                    
                    if selectedImage != nil {
                        Button(action: processPhoto) {
                            HStack {
                                if isProcessing {
                                    ProgressView()
                                        .scaleEffect(0.8)
                                } else {
                                    Image(systemName: "person.3d")
                                }
                                Text(isProcessing ? "Creating Avatar..." : "Create 3D Avatar")
                            }
                            .font(.headline)
                            .foregroundColor(.white)
                            .frame(maxWidth: .infinity)
                            .padding()
                            .background(isProcessing ? Color.gray : Color.purple)
                            .cornerRadius(12)
                        }
                        .disabled(isProcessing)
                    }
                    
                    if processedAvatar != nil {
                        Button(action: { showingUnityView = true }) {
                            Label("View in 3D", systemImage: "cube.transparent")
                                .font(.headline)
                                .foregroundColor(.white)
                                .frame(maxWidth: .infinity)
                                .padding()
                                .background(Color.orange)
                                .cornerRadius(12)
                        }
                    }
                }
                .padding(.horizontal)
                
                Spacer()
                
                // Processing Status
                if isProcessing {
                    VStack {
                        Text("Processing your photo...")
                            .font(.subheadline)
                            .foregroundColor(.secondary)
                        
                        Text("Using AI to detect facial landmarks")
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                }
            }
            .padding()
            .navigationBarHidden(true)
        }
        .sheet(isPresented: $showingImagePicker) {
            ImagePicker(selectedImage: $selectedImage)
        }
        .sheet(isPresented: $showingCamera) {
            CameraView(selectedImage: $selectedImage)
        }
        .sheet(isPresented: $showingUnityView) {
            if let avatar = processedAvatar {
                UnityViewWrapper(avatarData: avatar)
            }
        }
    }
    
    private func processPhoto() {
        guard let image = selectedImage else { return }
        
        isProcessing = true
        
        Task {
            do {
                let avatar = try await avatarProcessor.processImage(image)
                await MainActor.run {
                    self.processedAvatar = avatar
                    self.isProcessing = false
                }
            } catch {
                await MainActor.run {
                    self.isProcessing = false
                    // Handle error
                    print("Processing failed: \(error)")
                }
            }
        }
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}