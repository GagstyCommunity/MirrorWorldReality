import SwiftUI
import UnityFramework

struct UnityViewWrapper: UIViewRepresentable {
    let avatarData: Avatar3DModel
    
    func makeUIView(context: Context) -> UIView {
        let unityView = UnityEmbeddedSwift.getInstance().getUnityView()
        
        // Send avatar data to Unity
        sendAvatarDataToUnity(avatarData)
        
        return unityView!
    }
    
    func updateUIView(_ uiView: UIView, context: Context) {
        // Update Unity view if needed
    }
    
    private func sendAvatarDataToUnity(_ avatar: Avatar3DModel) {
        // Convert avatar data to JSON string
        do {
            let encoder = JSONEncoder()
            let jsonData = try encoder.encode(avatar)
            let jsonString = String(data: jsonData, encoding: .utf8) ?? ""
            
            // Send to Unity via message system
            UnityFramework.getInstance().sendMessageToGO(
                withName: "AvatarManager",
                functionName: "LoadAvatarData",
                message: jsonString
            )
        } catch {
            print("Failed to encode avatar data: \(error)")
        }
    }
}

// Unity Bridge Class
class UnityEmbeddedSwift: UIResponder, UIApplicationDelegate, UnityFrameworkListener {
    private static var instance: UnityEmbeddedSwift!
    private var ufw: UnityFramework!
    private var hostMainWindow: UIWindow? = nil
    
    private var isInitialized: Bool {
        ufw?.appController() != nil
    }
    
    static func getInstance() -> UnityEmbeddedSwift {
        if UnityEmbeddedSwift.instance == nil {
            UnityEmbeddedSwift.instance = UnityEmbeddedSwift()
        }
        return UnityEmbeddedSwift.instance
    }
    
    func show() {
        if isInitialized {
            showWindow()
        } else {
            initWindow()
        }
    }
    
    func hide() {
        if hostMainWindow == nil {
            hostMainWindow = UIApplication.shared.windows.first
        }
        hostMainWindow?.makeKeyAndVisible()
    }
    
    func initWindow() {
        if isInitialized {
            showWindow()
            return
        }
        
        hostMainWindow = UIApplication.shared.windows.first
        
        let bundlePath: String = Bundle.main.path(forResource: "UnityFramework", ofType: "framework", inDirectory: "Frameworks") ?? ""
        
        let bundle = Bundle(path: bundlePath )
        if bundle != nil {
            let ufw = bundle?.principalClass?.getInstance()
            if ufw != nil {
                self.ufw = ufw!
                self.ufw.setDataBundleId("com.unity3d.framework")
                self.ufw.register(self)
                
                NSClassFromString("FrameworkLibAPI")?.registerAPIforNativeCalls(self)
                
                self.ufw.runEmbedded(withArgc: CommandLine.argc, argv: CommandLine.unsafeArgv, appLaunchOpts: nil)
            }
        }
    }
    
    private func showWindow() {
        if isInitialized {
            ufw.showUnityWindow()
        }
    }
    
    func getUnityView() -> UIView? {
        return ufw.appController()?.rootViewController?.view
    }
    
    // UnityFrameworkListener
    func unityDidUnload(_ notification: Notification!) {
        ufw.unregisterFrameworkListener(self)
        ufw = nil
        hostMainWindow?.makeKeyAndVisible()
    }
}

// Native API for Unity to call back to Swift
@objc public class FrameworkLibAPI: NSObject {
    @objc public static func registerAPIforNativeCalls(_ api: UnityEmbeddedSwift) {
        // Register callbacks from Unity
    }
    
    @objc public static func showHostMainWindow(_ message: String) {
        UnityEmbeddedSwift.getInstance().hide()
    }
}