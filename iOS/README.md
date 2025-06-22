# MirrorWorld iOS App

Complete iOS application for MirrorWorld - transforms user photos into photorealistic 3D avatars in Unity park scenes.

## Project Structure

```
iOS/
├── MirrorWorld.xcodeproj/          # Xcode project configuration
├── MirrorWorld/                    # Main app source code
│   ├── MirrorWorldApp.swift        # App entry point and state management
│   ├── ContentView.swift           # Main view controller with navigation
│   ├── Views/                      # SwiftUI view components
│   │   ├── WelcomeView.swift       # Onboarding welcome screen
│   │   ├── OnboardingView.swift    # Feature explanation flow
│   │   ├── CameraView.swift        # Photo capture and selection
│   │   ├── ProcessingView.swift    # AI processing progress display
│   │   └── UnityView.swift         # 3D avatar scene integration
│   ├── Services/                   # Backend and processing services
│   │   ├── APIService.swift        # Backend API communication
│   │   └── ImageProcessor.swift    # Image validation and enhancement
│   ├── Models/                     # Data structures and types
│   │   └── Models.swift            # Avatar, processing, and API models
│   ├── Assets.xcassets            # App icons and images
│   └── Info.plist                 # App configuration and permissions
└── README.md                      # This file
```

## Features

### 🎯 Core Functionality
- **Photo Capture**: Camera integration with PhotoKit support
- **AI Processing**: Real-time backend communication for 3D avatar generation
- **Unity Integration**: Embedded Unity scenes with native SwiftUI interface
- **3D Avatar Display**: Photorealistic avatars with breathing, blinking animations

### 📱 User Experience
- **Progressive Onboarding**: Smooth explanation of features and permissions
- **Real-time Feedback**: Processing progress with detailed status updates
- **Interactive 3D Scene**: Touch controls for camera movement and avatar interaction
- **Screenshot Capture**: Save avatar photos to device gallery

### 🔒 Privacy & Security
- **Permission Management**: Camera and photo library access with clear explanations
- **Secure Processing**: Photos processed securely without permanent storage
- **Local Processing**: Image validation and enhancement on-device

## Technical Implementation

### SwiftUI Architecture
- **State Management**: Centralized AppState with ObservableObject pattern
- **Navigation**: Screen-based navigation with smooth transitions
- **Reactive UI**: Combine framework for real-time status updates
- **Modern iOS**: SwiftUI with iOS 17+ features and APIs

### Unity Integration
- **UnityFramework**: Native Unity integration within iOS app
- **Bidirectional Communication**: Swift ↔ Unity messaging system
- **Performance Optimized**: Efficient rendering with pause/resume lifecycle
- **Memory Management**: Proper cleanup and resource management

### Backend Integration
- **FastAPI Communication**: RESTful API calls for photo processing
- **Async Processing**: Background photo upload and status monitoring
- **Error Handling**: Comprehensive error states with user-friendly messages
- **Retry Logic**: Automatic retry for network failures

## Build Requirements

### Development Environment
- **Xcode 15.0+**: Latest Xcode with iOS 17 SDK
- **Swift 5.9+**: Modern Swift language features
- **iOS 17.0+**: Minimum deployment target
- **Unity 2023.2+**: Unity version with iOS build support

### Dependencies
- **SwiftUI**: Native iOS UI framework
- **Combine**: Reactive programming for state management
- **PhotosUI**: Photo selection and camera integration
- **AVFoundation**: Camera capture functionality
- **Vision**: Face detection and image analysis
- **CoreImage**: Image processing and enhancement
- **UnityFramework**: Unity 3D engine integration

### Permissions Required
- **Camera Access**: For photo capture functionality
- **Photo Library**: For photo selection and saving screenshots
- **Network Access**: For backend API communication

## Setup Instructions

### 1. Xcode Project Setup
```bash
# Open the Xcode project
open iOS/MirrorWorld.xcodeproj

# Configure development team and bundle identifier
# Build Settings > Signing & Capabilities
```

### 2. Unity Framework Integration
```bash
# Build Unity project for iOS
# Copy UnityFramework.framework to iOS project
# Add framework to Xcode project dependencies
```

### 3. Backend Configuration
```swift
// Update APIService.swift with your backend URL
private let baseURL = "https://your-backend-url.com"
// For local development: "http://localhost:5000"
```

### 4. Build and Run
```bash
# Build for device or simulator
# Ensure camera permissions are configured for device testing
```

## App Flow

### 1. Welcome & Onboarding
- App introduction with animated logo
- Feature explanation with step-by-step guide
- Privacy information and permission requests

### 2. Photo Capture
- Camera integration with live preview
- Photo library selection option
- Image validation and quality checking
- Face detection verification

### 3. AI Processing
- Secure photo upload to backend
- Real-time progress monitoring
- Processing stage visualization
- Error handling with retry options

### 4. 3D Avatar Scene
- Unity scene initialization
- Avatar loading and animation setup
- Interactive camera controls
- Screenshot capture functionality

## Key Files Explained

### MirrorWorldApp.swift
- App entry point with SwiftUI App protocol
- Centralized state management with AppState class
- Navigation logic between screens
- Error handling and user feedback

### APIService.swift
- Backend communication layer
- Photo upload with multipart form data
- Processing status polling
- Avatar data retrieval

### UnityView.swift
- Unity framework integration
- Swift ↔ Unity messaging bridge
- Scene management and controls
- Performance optimization

### Models.swift
- Data structures for avatar processing
- API request/response models
- Unity communication types
- Error definitions

## Performance Considerations

### Memory Management
- Proper image compression before upload
- Unity scene cleanup on navigation
- Automatic resource deallocation
- Background task management

### Network Optimization
- Efficient image compression
- Request timeout handling
- Retry logic for failed uploads
- Offline state management

### UI Responsiveness
- Async/await for all network calls
- Background processing queues
- Smooth animation transitions
- Progress feedback for long operations

## Testing

### Unit Testing
- API service functionality
- Image processing validation
- State management logic
- Error handling scenarios

### Integration Testing
- Unity framework communication
- Backend API integration
- Camera and photo library access
- End-to-end user flows

### Device Testing
- Camera functionality on physical devices
- Performance across different iOS versions
- Memory usage optimization
- Network condition handling

## Deployment

### App Store Preparation
- Configure app metadata and screenshots
- Set up provisioning profiles
- Enable required capabilities
- Review App Store guidelines compliance

### Distribution
- Archive build for distribution
- Upload to App Store Connect
- Configure TestFlight for beta testing
- Submit for App Store review

## Troubleshooting

### Common Issues
- **Unity Framework**: Ensure framework is properly embedded
- **Permissions**: Verify Info.plist contains usage descriptions
- **Network**: Check backend URL configuration
- **Camera**: Test on physical device (simulator limitations)

### Debug Tips
- Enable verbose logging in Debug builds
- Use Xcode debugger for SwiftUI state
- Monitor Unity console for 3D scene issues
- Check network requests in Xcode logs

## Future Enhancements

### Planned Features
- Multiple avatar styles and customization
- Social sharing integration
- AR preview mode
- Advanced animation controls
- Cloud avatar storage

### Technical Improvements
- Core ML integration for on-device processing
- Metal shaders for enhanced rendering
- Background app refresh for processing
- Widget extension for quick access