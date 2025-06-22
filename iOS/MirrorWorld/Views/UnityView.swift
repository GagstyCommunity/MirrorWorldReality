//
//  UnityView.swift
//  MirrorWorld
//
//  Created by MirrorWorld Team on 6/22/25.
//

import SwiftUI
import UnityFramework

struct UnityView: View {
    @EnvironmentObject var appState: AppState
    @StateObject private var unityManager = UnityManager()
    @State private var showingControls = true
    @State private var controlsTimer: Timer?
    
    var body: some View {
        ZStack {
            // Unity View Container
            UnityViewControllerWrapper(
                avatarData: appState.avatarData,
                unityManager: unityManager
            )
            .ignoresSafeArea()
            .onTapGesture {
                toggleControls()
            }
            
            // Overlay Controls
            if showingControls {
                VStack {
                    // Top controls
                    HStack {
                        Button(action: {
                            appState.reset()
                        }) {
                            Image(systemName: "house.fill")
                                .font(.title2)
                                .foregroundColor(.white)
                                .frame(width: 44, height: 44)
                                .background(
                                    Circle()
                                        .fill(Color.black.opacity(0.6))
                                        .blur(radius: 10)
                                )
                        }
                        .buttonStyle(ScaleButtonStyle())
                        
                        Spacer()
                        
                        HStack(spacing: 12) {
                            Button(action: {
                                unityManager.resetCamera()
                            }) {
                                Image(systemName: "arrow.counterclockwise")
                                    .font(.title3)
                                    .foregroundColor(.white)
                                    .frame(width: 40, height: 40)
                                    .background(
                                        Circle()
                                            .fill(Color.black.opacity(0.6))
                                            .blur(radius: 10)
                                    )
                            }
                            .buttonStyle(ScaleButtonStyle())
                            
                            Button(action: {
                                unityManager.captureScreenshot()
                            }) {
                                Image(systemName: "camera.fill")
                                    .font(.title3)
                                    .foregroundColor(.white)
                                    .frame(width: 40, height: 40)
                                    .background(
                                        Circle()
                                            .fill(Color.black.opacity(0.6))
                                            .blur(radius: 10)
                                    )
                            }
                            .buttonStyle(ScaleButtonStyle())
                        }
                    }
                    .padding(.horizontal, 20)
                    .padding(.top, 10)
                    
                    Spacer()
                    
                    // Bottom info panel
                    VStack(spacing: 8) {
                        HStack {
                            VStack(alignment: .leading, spacing: 4) {
                                Text("Your Avatar in Paradise Park")
                                    .font(.headline)
                                    .fontWeight(.semibold)
                                    .foregroundColor(.white)
                                
                                Text("Drag to rotate • Pinch to zoom • Tap to hide controls")
                                    .font(.caption)
                                    .foregroundColor(.white.opacity(0.8))
                            }
                            
                            Spacer()
                        }
                        .padding(.horizontal, 20)
                        .padding(.vertical, 12)
                        .background(
                            RoundedRectangle(cornerRadius: 12)
                                .fill(Color.black.opacity(0.6))
                                .blur(radius: 10)
                        )
                    }
                    .padding(.horizontal, 20)
                    .padding(.bottom, 30)
                }
                .transition(.opacity)
                .animation(.easeInOut(duration: 0.3), value: showingControls)
            }
            
            // Loading overlay if Unity is not ready
            if !unityManager.isUnityReady {
                ZStack {
                    Color.black.opacity(0.8)
                        .ignoresSafeArea()
                    
                    VStack(spacing: 20) {
                        ProgressView()
                            .progressViewStyle(CircularProgressViewStyle(tint: .white))
                            .scaleEffect(1.5)
                        
                        Text("Loading Unity Environment...")
                            .font(.headline)
                            .foregroundColor(.white)
                    }
                }
            }
        }
        .onAppear {
            unityManager.initializeUnity()
            startControlsTimer()
        }
        .onDisappear {
            unityManager.pauseUnity()
            stopControlsTimer()
        }
        .onChange(of: appState.avatarData) { avatarData in
            if let avatarData = avatarData {
                unityManager.loadAvatar(avatarData)
            }
        }
        .alert("Screenshot Saved", isPresented: $unityManager.showingScreenshotAlert) {
            Button("OK") { }
        } message: {
            Text("Your avatar screenshot has been saved to Photos.")
        }
    }
    
    private func toggleControls() {
        withAnimation {
            showingControls.toggle()
        }
        
        if showingControls {
            startControlsTimer()
        }
    }
    
    private func startControlsTimer() {
        stopControlsTimer()
        controlsTimer = Timer.scheduledTimer(withTimeInterval: 3.0, repeats: false) { _ in
            withAnimation {
                showingControls = false
            }
        }
    }
    
    private func stopControlsTimer() {
        controlsTimer?.invalidate()
        controlsTimer = nil
    }
}

// Unity Manager to handle Unity Framework integration
class UnityManager: ObservableObject {
    @Published var isUnityReady = false
    @Published var showingScreenshotAlert = false
    
    private var unityFramework: UnityFramework?
    private var currentAvatarData: Avatar3DModel?
    
    func initializeUnity() {
        guard unityFramework == nil else { return }
        
        // Initialize Unity Framework
        unityFramework = UnityFrameworkLoad()
        
        // Set up Unity callbacks
        unityFramework?.setDataBundleId("com.unity3d.framework")
        unityFramework?.register(self)
        
        // Start Unity
        unityFramework?.runEmbedded(withArgc: 0, argv: nil, appLaunchOpts: nil)
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 2.0) {
            self.isUnityReady = true
            
            // Load avatar if we have data
            if let avatarData = self.currentAvatarData {
                self.loadAvatar(avatarData)
            }
        }
    }
    
    func loadAvatar(_ avatarData: Avatar3DModel) {
        currentAvatarData = avatarData
        
        guard isUnityReady else { return }
        
        // Send avatar data to Unity
        let avatarJson = encodeAvatarData(avatarData)
        unityFramework?.sendMessageToGO(withName: "AvatarManager", functionName: "LoadAvatar", message: avatarJson)
    }
    
    func resetCamera() {
        unityFramework?.sendMessageToGO(withName: "CameraController", functionName: "ResetCamera", message: "")
    }
    
    func captureScreenshot() {
        unityFramework?.sendMessageToGO(withName: "ScreenshotManager", functionName: "CaptureScreenshot", message: "")
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
            self.showingScreenshotAlert = true
        }
    }
    
    func pauseUnity() {
        unityFramework?.pause(true)
    }
    
    func resumeUnity() {
        unityFramework?.pause(false)
    }
    
    private func encodeAvatarData(_ avatarData: Avatar3DModel) -> String {
        do {
            let encoder = JSONEncoder()
            let data = try encoder.encode(avatarData)
            return String(data: data, encoding: .utf8) ?? "{}"
        } catch {
            print("Failed to encode avatar data: \(error)")
            return "{}"
        }
    }
}

// Unity Framework loading helper
func UnityFrameworkLoad() -> UnityFramework? {
    let bundlePath = Bundle.main.bundlePath + "/Frameworks/UnityFramework.framework"
    let bundle = Bundle(path: bundlePath)
    
    if bundle?.isLoaded == false {
        bundle?.load()
    }
    
    let ufw = bundle?.principalClass?.getInstance()
    if ufw?.appController() == nil {
        let machineHeader = UnsafeMutablePointer<MachHeader>.allocate(capacity: 1)
        machineHeader.pointee = _mh_execute_header
        ufw?.setExecuteHeader(machineHeader)
    }
    
    return ufw
}

// Unity View Controller Wrapper
struct UnityViewControllerWrapper: UIViewControllerRepresentable {
    let avatarData: Avatar3DModel?
    let unityManager: UnityManager
    
    func makeUIViewController(context: Context) -> UnityViewController {
        return UnityViewController(unityManager: unityManager)
    }
    
    func updateUIViewController(_ uiViewController: UnityViewController, context: Context) {
        // Update Unity view if needed
    }
}

// Unity View Controller
class UnityViewController: UIViewController {
    private let unityManager: UnityManager
    
    init(unityManager: UnityManager) {
        self.unityManager = unityManager
        super.init(nibName: nil, bundle: nil)
    }
    
    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
    
    override func viewDidLoad() {
        super.viewDidLoad()
        setupUnityView()
    }
    
    private func setupUnityView() {
        // Unity will attach its view to this view controller
        // The Unity Framework handles the actual view setup
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        unityManager.resumeUnity()
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
        unityManager.pauseUnity()
    }
}

// Unity Framework protocol conformance
extension UnityManager: UnityFrameworkListener {
    func unityDidUnload(_ notification: Notification!) {
        isUnityReady = false
    }
    
    func unityDidQuit(_ notification: Notification!) {
        isUnityReady = false
    }
}

#Preview {
    UnityView()
        .environmentObject({
            let state = AppState()
            // Mock avatar data for preview
            return state
        }())
}