//
//  WelcomeView.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI

struct WelcomeView: View {
    @EnvironmentObject var appState: AppState
    @State private var isAnimating = false
    
    var body: some View {
        VStack(spacing: 40) {
            Spacer()
            
            // App Icon with animation
            VStack(spacing: 20) {
                ZStack {
                    Circle()
                        .fill(
                            LinearGradient(
                                gradient: Gradient(colors: [
                                    Color(red: 0.4, green: 0.49, blue: 0.92),
                                    Color(red: 0.46, green: 0.29, blue: 0.64)
                                ]),
                                startPoint: .topLeading,
                                endPoint: .bottomTrailing
                            )
                        )
                        .frame(width: 120, height: 120)
                        .overlay(
                            Circle()
                                .stroke(Color.white, lineWidth: 3)
                        )
                        .scaleEffect(isAnimating ? 1.1 : 1.0)
                        .animation(
                            Animation.easeInOut(duration: 2.0).repeatForever(autoreverses: true),
                            value: isAnimating
                        )
                    
                    VStack(spacing: 8) {
                        HStack(spacing: 12) {
                            Circle()
                                .fill(Color.white)
                                .frame(width: 12, height: 12)
                            Circle()
                                .fill(Color.white)
                                .frame(width: 12, height: 12)
                        }
                        
                        RoundedRectangle(cornerRadius: 8)
                            .fill(Color.white)
                            .frame(width: 30, height: 4)
                    }
                }
                
                Text("MirrorWorld")
                    .font(.largeTitle)
                    .fontWeight(.bold)
                    .foregroundColor(.white)
                    .opacity(isAnimating ? 1.0 : 0.8)
                    .animation(
                        Animation.easeInOut(duration: 2.0).repeatForever(autoreverses: true),
                        value: isAnimating
                    )
            }
            
            // Description
            VStack(spacing: 16) {
                Text("Transform your photo into a photorealistic 3D avatar living in a beautiful virtual park")
                    .font(.title2)
                    .fontWeight(.medium)
                    .foregroundColor(.white)
                    .multilineTextAlignment(.center)
                    .padding(.horizontal, 32)
                
                Text("Experience yourself in 3D - breathing, blinking, alive in a virtual memory")
                    .font(.body)
                    .foregroundColor(.white.opacity(0.8))
                    .multilineTextAlignment(.center)
                    .padding(.horizontal, 40)
            }
            
            Spacer()
            
            // Get Started Button
            Button(action: {
                appState.navigateTo(.onboarding)
            }) {
                HStack(spacing: 12) {
                    Image(systemName: "camera.fill")
                        .font(.title2)
                    Text("Get Started")
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
                .padding(.horizontal, 32)
            }
            .buttonStyle(ScaleButtonStyle())
            
            Spacer().frame(height: 40)
        }
        .onAppear {
            isAnimating = true
        }
    }
}

struct ScaleButtonStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .scaleEffect(configuration.isPressed ? 0.95 : 1.0)
            .opacity(configuration.isPressed ? 0.8 : 1.0)
            .animation(.easeInOut(duration: 0.1), value: configuration.isPressed)
    }
}

#Preview {
    WelcomeView()
        .environmentObject(AppState())
}