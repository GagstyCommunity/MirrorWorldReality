# MirrorWorld - Photo to 3D Avatar System

A comprehensive cross-platform application that converts user photos into realistic 3D avatars using AI facial landmark detection with MediaPipe.

## Features

- **Real Photo Processing**: Uses MediaPipe to detect 468 facial landmarks from uploaded photos
- **Authentic 3D Generation**: Creates personalized 3D mesh geometry from actual facial structure
- **Cross-Platform**: Web interface, iOS Swift app, and Unity integration
- **Real-Time Rendering**: Three.js web viewer and Unity 3D park environment
- **Photo Texture Mapping**: Applies user's actual photo as avatar texture

## Architecture

### Backend (Python/FastAPI)
- **main.py**: FastAPI server with photo upload and processing endpoints
- **api/processing_real.py**: MediaPipe-based facial landmark detection and 3D mesh generation
- **config.py**: Application settings and environment configuration

### Web Interface
- **static/index.html**: Progressive web app with photo upload interface
- **static/scene.js**: Three.js 3D rendering and avatar display
- **static/app.js**: UI controls and processing workflow

### iOS Swift App
- **ContentView.swift**: SwiftUI interface for photo capture and avatar creation
- **AvatarProcessor.swift**: Network layer for communicating with Python backend
- **ImagePicker.swift**: Camera and photo library integration
- **UnityViewWrapper.swift**: Unity framework integration for 3D viewing

### Unity Integration
- **AvatarManager.cs**: Receives avatar data from iOS and creates 3D mesh
- **Avatar3DModel.cs**: Data structures for cross-platform avatar representation
- **CameraController.cs**: Touch controls and camera movement in 3D environment

## Setup Instructions

### 1. Python Backend
```bash
# Install dependencies
pip install fastapi uvicorn mediapipe opencv-python-headless pillow numpy

# Run server
python main.py
```

### 2. Web Interface
The web interface is served by the FastAPI server. Access at `http://localhost:5000`

### 3. iOS Development
1. Open `iOS/MirrorWorld.xcodeproj` in Xcode
2. Update server URL in `AvatarProcessor.swift`
3. Build and run on iOS device or simulator

### 4. Unity Setup
1. Open `Unity/MirrorWorldPark` in Unity 2022.3 or later
2. Install Newtonsoft JSON package via Package Manager
3. Build as iOS framework for integration with Swift app

## Technical Implementation

### Photo to 3D Pipeline
1. **Photo Upload**: User captures or selects photo via web/iOS interface
2. **Facial Detection**: MediaPipe analyzes image and extracts 468 landmark points
3. **3D Mesh Generation**: Landmarks converted to 3D vertices with proper face topology
4. **Texture Creation**: Original photo processed and mapped to 3D geometry
5. **Rendering**: Avatar displayed in Three.js web viewer or Unity 3D environment

### Cross-Platform Data Flow
```
iOS Photo Capture → Python Processing → 3D Avatar Data → Unity Rendering
     ↓                      ↓                    ↓              ↓
SwiftUI Interface    MediaPipe Analysis    JSON Serialization  3D Mesh Creation
```

### Key Technologies
- **MediaPipe**: Google's framework for facial landmark detection
- **FastAPI**: High-performance Python web framework
- **Three.js**: Web-based 3D graphics rendering
- **SwiftUI**: Modern iOS user interface framework
- **Unity**: Cross-platform 3D engine and rendering

## File Structure
```
/
├── main.py                          # FastAPI server entry point
├── config.py                        # Application configuration
├── api/
│   ├── models.py                    # Data models and schemas
│   ├── processing_real.py           # MediaPipe facial processing
│   └── processing_simple.py         # Fallback processing
├── static/
│   ├── index.html                   # Web interface
│   ├── app.js                       # Frontend logic
│   ├── scene.js                     # Three.js 3D rendering
│   └── style.css                    # Web styling
├── iOS/
│   ├── MirrorWorld.xcodeproj/       # Xcode project
│   └── MirrorWorld/
│       ├── ContentView.swift        # Main iOS interface
│       ├── AvatarProcessor.swift    # Network processing
│       ├── ImagePicker.swift        # Photo capture
│       ├── UnityViewWrapper.swift   # Unity integration
│       └── Info.plist               # iOS permissions
└── Unity/
    └── MirrorWorldPark/
        ├── Assets/Scripts/
        │   ├── AvatarManager.cs     # 3D avatar creation
        │   ├── Avatar3DModel.cs     # Data structures
        │   └── CameraController.cs  # 3D camera controls
        └── ProjectSettings/         # Unity configuration
```

## Usage

### Web Interface
1. Access the web app at `http://localhost:5000`
2. Click "Choose Photo" or use camera to capture image
3. Wait for AI processing (10-15 seconds)
4. View your personalized 3D avatar in the virtual park

### iOS App
1. Launch MirrorWorld app on iOS device
2. Take photo or select from library
3. Tap "Create 3D Avatar" to process
4. View in integrated Unity 3D environment

## Development Notes

- Server binds to `0.0.0.0:5000` for cross-platform access
- MediaPipe provides 468 precise facial landmarks for realistic geometry
- Photos are processed server-side to maintain quality and security
- Unity integration allows for advanced 3D interactions and animations
- All processing uses authentic facial data rather than generic models

## Deployment

The system is designed for deployment on GitHub with subsequent testing in Xcode. The FastAPI backend can be deployed to cloud services, while the iOS app integrates Unity for 3D rendering capabilities.

Ready for GitHub push and Xcode testing.