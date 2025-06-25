# MirrorWorld - 3D Avatar Generation Platform

## Overview

MirrorWorld is a FastAPI-based web application that transforms user photos into photorealistic 3D avatars living in virtual park environments. The system combines AI-powered image processing with 3D rendering to create immersive avatar experiences. The application features a progressive web interface that guides users through photo upload, processing, and 3D avatar visualization.

## System Architecture

### Backend Architecture
- **Framework**: FastAPI with Python 3.11
- **Processing Engine**: Custom PhotoProcessor using PIL for image manipulation
- **API Design**: RESTful endpoints with async/await patterns
- **File Handling**: Multipart form data support for image uploads
- **Static Assets**: Served via FastAPI's StaticFiles mounting

### Frontend Architecture
- **UI Framework**: Bootstrap 5 for responsive design
- **3D Rendering**: Three.js for WebGL-based 3D scene management
- **JavaScript Architecture**: Class-based ES6 modules (MirrorWorldApp, SceneManager)
- **Progressive Interface**: Screen-based navigation system with smooth transitions
- **Interactive Elements**: Drag-and-drop file upload with visual feedback

### Configuration Management
- **Environment-based**: Centralized Settings class using environment variables
- **Flexible Deployment**: Support for both local and cloud configurations
- **Performance Tuning**: Configurable GPU acceleration, batch processing, and quality settings

## Key Components

### Core Processing Pipeline
1. **Image Validation**: Content-type checking and file size limits (10MB max)
2. **Photo Enhancement**: PIL-based preprocessing including noise reduction and color correction
3. **3D Model Generation**: Avatar mesh creation with vertices, faces, and texture mapping
4. **Animation System**: Blend shapes and skeletal animation for lifelike movement
5. **Environment Integration**: Virtual park scene with lighting and environmental effects

### Data Models
- **Avatar3DModel**: Comprehensive 3D avatar data structure with mesh, textures, and animations
- **ProcessingStatus**: Enum-based status tracking (processing, completed, failed)
- **ProcessingRequest/Response**: API contract definitions for photo upload workflow

### File Management
- **Upload Storage**: Temporary file handling in `/uploads` directory
- **Model Storage**: Generated 3D models stored in `/models` directory
- **Output Management**: Processed results in `/outputs` with configurable retention (7 days default)
- **Cloud Integration**: Optional AWS S3 support for production deployments

## Data Flow

1. **Photo Upload**: User selects/drops image → Client validates → Server receives multipart data
2. **Processing Pipeline**: Image enhancement → Face detection → 3D mesh generation → Texture mapping
3. **Avatar Generation**: Mesh creation → Animation rigging → Quality optimization → Storage
4. **Visualization**: 3D scene setup → Avatar loading → Park environment → Interactive controls
5. **User Interaction**: Real-time avatar manipulation → Animation playback → Export options

## External Dependencies

### Python Dependencies
- **FastAPI**: Web framework and API development
- **Uvicorn**: ASGI server for production deployment
- **Pillow**: Image processing and manipulation
- **NumPy**: Numerical computations for 3D operations
- **Pydantic**: Data validation and serialization

### Frontend Dependencies
- **Bootstrap 5**: UI components and responsive grid system
- **Three.js**: 3D graphics rendering and WebGL abstraction
- **Feather Icons**: Lightweight icon system for UI elements

### System Dependencies (via Nix)
- **Graphics Libraries**: freetype, libjpeg, libwebp, libtiff for image format support
- **Compression**: zlib for efficient data handling
- **UI Libraries**: tcl, tk for potential GUI extensions

## Deployment Strategy

### Development Environment
- **Replit Integration**: Configured for seamless cloud development
- **Hot Reload**: Automatic server restart on code changes
- **Port Management**: FastAPI on port 8000, exposed via port 80

### Production Considerations
- **Container Ready**: Pip-based dependency installation for Docker compatibility
- **Environment Configuration**: Comprehensive settings management for different deployment targets
- **Monitoring Support**: Optional metrics collection on port 9090
- **Storage Flexibility**: Local filesystem or cloud storage (AWS S3) options

### Performance Optimizations
- **Concurrent Processing**: Configurable job limits (4 concurrent by default)
- **GPU Acceleration**: Optional CUDA/MPS support for faster processing
- **Model Precision**: Configurable FP16/FP32 for memory/quality trade-offs
- **Batch Processing**: Configurable batch sizes for efficient resource utilization

## Changelog
- June 22, 2025. Initial setup - Complete MirrorWorld platform with web prototype, iOS app, and Unity 3D integration

## Recent Changes
- Created complete Swift iOS app with SwiftUI interface for photo capture and 3D avatar viewing
- Built Unity 3D park environment with photorealistic rendering and avatar animation system
- Generated full Xcode project structure with proper iOS/Unity Framework integration
- Implemented comprehensive Swift-Unity communication bridge for seamless native experience
- Developed FastAPI backend already running and tested with photo processing pipeline

## User Preferences

Preferred communication style: Simple, everyday language.
Project focus: Generate complete Swift + Unity codebase for testing in Xcode and Unity Editor.
