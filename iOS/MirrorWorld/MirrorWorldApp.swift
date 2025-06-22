//
//  MirrorWorldApp.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI

@main
struct MirrorWorldApp: App {
    @StateObject private var appState = AppState()
    
    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(appState)
                .preferredColorScheme(.light)
        }
    }
}

class AppState: ObservableObject {
    @Published var currentView: AppView = .welcome
    @Published var selectedImage: UIImage?
    @Published var processId: String?
    @Published var avatarData: Avatar3DModel?
    @Published var isProcessing = false
    @Published var processingProgress: Double = 0.0
    @Published var processingMessage = ""
    @Published var hasError = false
    @Published var errorMessage = ""
    
    enum AppView {
        case welcome
        case onboarding
        case camera
        case processing
        case unity
    }
    
    func navigateTo(_ view: AppView) {
        DispatchQueue.main.async {
            self.currentView = view
        }
    }
    
    func setImage(_ image: UIImage) {
        DispatchQueue.main.async {
            self.selectedImage = image
        }
    }
    
    func startProcessing(with processId: String) {
        DispatchQueue.main.async {
            self.processId = processId
            self.isProcessing = true
            self.processingProgress = 0.0
            self.processingMessage = "Starting AI processing..."
            self.navigateTo(.processing)
        }
    }
    
    func updateProcessing(progress: Double, message: String) {
        DispatchQueue.main.async {
            self.processingProgress = progress
            self.processingMessage = message
        }
    }
    
    func completeProcessing(with avatarData: Avatar3DModel) {
        DispatchQueue.main.async {
            self.avatarData = avatarData
            self.isProcessing = false
            self.navigateTo(.unity)
        }
    }
    
    func showError(_ message: String) {
        DispatchQueue.main.async {
            self.hasError = true
            self.errorMessage = message
        }
    }
    
    func clearError() {
        DispatchQueue.main.async {
            self.hasError = false
            self.errorMessage = ""
        }
    }
    
    func reset() {
        DispatchQueue.main.async {
            self.selectedImage = nil
            self.processId = nil
            self.avatarData = nil
            self.isProcessing = false
            self.processingProgress = 0.0
            self.processingMessage = ""
            self.clearError()
            self.navigateTo(.welcome)
        }
    }
}