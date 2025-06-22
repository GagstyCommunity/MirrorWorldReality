//
//  ContentView.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI

struct ContentView: View {
    @EnvironmentObject var appState: AppState
    
    var body: some View {
        ZStack {
            // Background gradient
            LinearGradient(
                gradient: Gradient(colors: [
                    Color(red: 0.4, green: 0.49, blue: 0.92),
                    Color(red: 0.46, green: 0.29, blue: 0.64)
                ]),
                startPoint: .topLeading,
                endPoint: .bottomTrailing
            )
            .ignoresSafeArea()
            
            // Main content based on current view
            Group {
                switch appState.currentView {
                case .welcome:
                    WelcomeView()
                case .onboarding:
                    OnboardingView()
                case .camera:
                    CameraView()
                case .processing:
                    ProcessingView()
                case .unity:
                    UnityView()
                }
            }
            .transition(.asymmetric(
                insertion: .move(edge: .trailing).combined(with: .opacity),
                removal: .move(edge: .leading).combined(with: .opacity)
            ))
            .animation(.easeInOut(duration: 0.5), value: appState.currentView)
        }
        .alert("Error", isPresented: $appState.hasError) {
            Button("OK") {
                appState.clearError()
            }
        } message: {
            Text(appState.errorMessage)
        }
    }
}

#Preview {
    ContentView()
        .environmentObject(AppState())
}