//
//  OnboardingView.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI

struct OnboardingView: View {
    @EnvironmentObject var appState: AppState
    @State private var currentStep = 0
    private let steps = [
        OnboardingStep(
            icon: "camera.fill",
            title: "Upload Your Photo",
            description: "Take a new photo or choose from your gallery. We need a clear view of your face for best results."
        ),
        OnboardingStep(
            icon: "brain.head.profile",
            title: "AI Processing",
            description: "Our advanced AI analyzes your photo and creates a detailed 3D model with realistic textures and animations."
        ),
        OnboardingStep(
            icon: "cube.transparent",
            title: "Enter Your World",
            description: "See yourself come alive in a beautiful virtual park with cinematic lighting and natural movements."
        )
    ]
    
    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                Button(action: {
                    appState.navigateTo(.welcome)
                }) {
                    Image(systemName: "chevron.left")
                        .font(.title2)
                        .foregroundColor(.white)
                }
                Spacer()
            }
            .padding(.horizontal, 24)
            .padding(.top, 20)
            
            Spacer().frame(height: 60)
            
            // Steps content
            VStack(spacing: 40) {
                Text("How it works")
                    .font(.largeTitle)
                    .fontWeight(.bold)
                    .foregroundColor(.white)
                
                VStack(spacing: 32) {
                    ForEach(Array(steps.enumerated()), id: \.offset) { index, step in
                        OnboardingStepView(
                            step: step,
                            stepNumber: index + 1,
                            isActive: index <= currentStep
                        )
                    }
                }
                .padding(.horizontal, 32)
            }
            
            Spacer()
            
            // Privacy section
            VStack(spacing: 20) {
                HStack(spacing: 12) {
                    Image(systemName: "shield.checkered")
                        .font(.title)
                        .foregroundColor(.green)
                    
                    VStack(alignment: .leading, spacing: 4) {
                        Text("Privacy First")
                            .font(.headline)
                            .foregroundColor(.white)
                        
                        Text("Your photos are processed securely and never stored permanently. Only you can see your 3D avatar.")
                            .font(.caption)
                            .foregroundColor(.white.opacity(0.8))
                            .multilineTextAlignment(.leading)
                    }
                    
                    Spacer()
                }
                .padding(.horizontal, 32)
                
                Button(action: {
                    appState.navigateTo(.camera)
                }) {
                    Text("Continue")
                        .font(.title2)
                        .fontWeight(.semibold)
                        .foregroundColor(.white)
                        .frame(maxWidth: .infinity)
                        .frame(height: 56)
                        .background(
                            RoundedRectangle(cornerRadius: 28)
                                .fill(Color.green.opacity(0.8))
                        )
                }
                .buttonStyle(ScaleButtonStyle())
                .padding(.horizontal, 32)
            }
            
            Spacer().frame(height: 40)
        }
        .onAppear {
            startStepAnimation()
        }
    }
    
    private func startStepAnimation() {
        Timer.scheduledTimer(withTimeInterval: 1.0, repeats: true) { timer in
            withAnimation(.easeInOut(duration: 0.5)) {
                currentStep = (currentStep + 1) % steps.count
            }
            
            if currentStep == 0 {
                DispatchQueue.main.asyncAfter(deadline: .now() + 2.0) {
                    timer.invalidate()
                }
            }
        }
    }
}

struct OnboardingStep {
    let icon: String
    let title: String
    let description: String
}

struct OnboardingStepView: View {
    let step: OnboardingStep
    let stepNumber: Int
    let isActive: Bool
    
    var body: some View {
        HStack(spacing: 16) {
            // Step number circle
            ZStack {
                Circle()
                    .fill(isActive ? Color.white : Color.white.opacity(0.3))
                    .frame(width: 40, height: 40)
                
                Text("\(stepNumber)")
                    .font(.headline)
                    .fontWeight(.bold)
                    .foregroundColor(isActive ? Color.blue : Color.white)
            }
            .scaleEffect(isActive ? 1.1 : 1.0)
            .animation(.spring(response: 0.5, dampingFraction: 0.7), value: isActive)
            
            // Step content
            VStack(alignment: .leading, spacing: 8) {
                HStack(spacing: 8) {
                    Image(systemName: step.icon)
                        .font(.title3)
                        .foregroundColor(isActive ? .white : .white.opacity(0.6))
                    
                    Text(step.title)
                        .font(.headline)
                        .fontWeight(.semibold)
                        .foregroundColor(isActive ? .white : .white.opacity(0.6))
                }
                
                Text(step.description)
                    .font(.body)
                    .foregroundColor(isActive ? .white.opacity(0.9) : .white.opacity(0.5))
                    .fixedSize(horizontal: false, vertical: true)
            }
            
            Spacer()
        }
        .opacity(isActive ? 1.0 : 0.6)
        .animation(.easeInOut(duration: 0.3), value: isActive)
    }
}

#Preview {
    OnboardingView()
        .environmentObject(AppState())
}