//
//  ProcessingView.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI

struct ProcessingView: View {
    @EnvironmentObject var appState: AppState
    @State private var rotationAngle: Double = 0
    @State private var pulseScale: CGFloat = 1.0
    @State private var currentStep = 0
    
    private let processingSteps = [
        "Person Segmentation",
        "Depth Analysis", 
        "3D Mesh Generation",
        "Animation Rigging",
        "Optimization"
    ]
    
    var body: some View {
        VStack(spacing: 40) {
            Spacer()
            
            // Processing Animation
            VStack(spacing: 32) {
                ZStack {
                    // Outer rotating ring
                    Circle()
                        .stroke(Color.white.opacity(0.2), lineWidth: 4)
                        .frame(width: 120, height: 120)
                    
                    Circle()
                        .trim(from: 0, to: CGFloat(appState.processingProgress))
                        .stroke(
                            LinearGradient(
                                gradient: Gradient(colors: [Color.blue, Color.purple]),
                                startPoint: .topLeading,
                                endPoint: .bottomTrailing
                            ),
                            style: StrokeStyle(lineWidth: 4, lineCap: .round)
                        )
                        .frame(width: 120, height: 120)
                        .rotationEffect(.degrees(-90))
                        .animation(.easeInOut(duration: 0.5), value: appState.processingProgress)
                    
                    // Inner pulsing circle
                    Circle()
                        .fill(
                            LinearGradient(
                                gradient: Gradient(colors: [
                                    Color.white.opacity(0.3),
                                    Color.white.opacity(0.1)
                                ]),
                                startPoint: .topLeading,
                                endPoint: .bottomTrailing
                            )
                        )
                        .frame(width: 80, height: 80)
                        .scaleEffect(pulseScale)
                        .animation(
                            Animation.easeInOut(duration: 1.5).repeatForever(autoreverses: true),
                            value: pulseScale
                        )
                    
                    // AI brain icon
                    Image(systemName: "brain.head.profile")
                        .font(.system(size: 32))
                        .foregroundColor(.white)
                        .rotationEffect(.degrees(rotationAngle))
                        .animation(
                            Animation.linear(duration: 3.0).repeatForever(autoreverses: false),
                            value: rotationAngle
                        )
                }
                
                // Progress percentage
                VStack(spacing: 8) {
                    Text("\(Int(appState.processingProgress * 100))%")
                        .font(.largeTitle)
                        .fontWeight(.bold)
                        .foregroundColor(.white)
                        .animation(.easeInOut(duration: 0.3), value: appState.processingProgress)
                    
                    Text("Creating Your Avatar")
                        .font(.title2)
                        .fontWeight(.semibold)
                        .foregroundColor(.white)
                }
            }
            
            // Current processing message
            Text(appState.processingMessage)
                .font(.body)
                .foregroundColor(.white.opacity(0.8))
                .multilineTextAlignment(.center)
                .padding(.horizontal, 32)
                .animation(.easeInOut(duration: 0.3), value: appState.processingMessage)
            
            // Processing steps
            VStack(spacing: 12) {
                LazyVGrid(columns: [
                    GridItem(.flexible()),
                    GridItem(.flexible())
                ], spacing: 12) {
                    ForEach(Array(processingSteps.enumerated()), id: \.offset) { index, step in
                        ProcessingStepBadge(
                            title: step,
                            isActive: index <= currentStep,
                            isCompleted: appState.processingProgress > Double(index) / Double(processingSteps.count)
                        )
                    }
                }
            }
            .padding(.horizontal, 32)
            
            Spacer()
            
            // Cancel button (optional)
            Button(action: {
                appState.reset()
            }) {
                Text("Cancel")
                    .font(.body)
                    .foregroundColor(.white.opacity(0.6))
                    .padding(.horizontal, 24)
                    .padding(.vertical, 12)
                    .background(
                        RoundedRectangle(cornerRadius: 20)
                            .fill(Color.black.opacity(0.2))
                    )
            }
            .buttonStyle(ScaleButtonStyle())
            
            Spacer().frame(height: 40)
        }
        .onAppear {
            startAnimations()
            updateCurrentStep()
        }
        .onChange(of: appState.processingProgress) { _ in
            updateCurrentStep()
        }
    }
    
    private func startAnimations() {
        rotationAngle = 360
        pulseScale = 1.2
    }
    
    private func updateCurrentStep() {
        let progress = appState.processingProgress
        let stepProgress = progress * Double(processingSteps.count)
        currentStep = min(Int(stepProgress), processingSteps.count - 1)
    }
}

struct ProcessingStepBadge: View {
    let title: String
    let isActive: Bool
    let isCompleted: Bool
    
    var body: some View {
        HStack(spacing: 8) {
            // Status indicator
            if isCompleted {
                Image(systemName: "checkmark.circle.fill")
                    .font(.caption)
                    .foregroundColor(.green)
            } else if isActive {
                Circle()
                    .fill(Color.blue)
                    .frame(width: 8, height: 8)
            } else {
                Circle()
                    .fill(Color.white.opacity(0.3))
                    .frame(width: 8, height: 8)
            }
            
            Text(title)
                .font(.caption)
                .fontWeight(isActive ? .semibold : .medium)
                .foregroundColor(isActive ? .white : .white.opacity(0.6))
                .multilineTextAlignment(.leading)
            
            Spacer(minLength: 0)
        }
        .padding(.horizontal, 12)
        .padding(.vertical, 8)
        .background(
            RoundedRectangle(cornerRadius: 16)
                .fill(isActive ? Color.white.opacity(0.2) : Color.white.opacity(0.1))
        )
        .scaleEffect(isActive ? 1.05 : 1.0)
        .animation(.easeInOut(duration: 0.3), value: isActive)
    }
}

#Preview {
    ProcessingView()
        .environmentObject({
            let state = AppState()
            state.processingProgress = 0.6
            state.processingMessage = "Generating 3D mesh and textures..."
            return state
        }())
}