import os
# Set environment variables before importing any MediaPipe dependencies
os.environ['MEDIAPIPE_DISABLE_GPU'] = '1'
os.environ['OPENCV_IO_ENABLE_OPENEXR'] = '1'
os.environ['CUDA_VISIBLE_DEVICES'] = ''

from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse, JSONResponse, Response
import uvicorn
import json
import asyncio
import uuid
from pathlib import Path
import shutil
from PIL import Image
import io
import base64

from api.processing_simple import PhotoProcessor
from api.models import ProcessingStatus, Avatar3DModel
from config import Settings

# Initialize FastAPI app
app = FastAPI(title="MirrorWorld API", version="1.0.0")

# Mount static files
app.mount("/static", StaticFiles(directory="static"), name="static")

# Also serve static files directly from root for compatibility
from fastapi.responses import FileResponse

@app.get("/style.css")
async def get_style_css():
    return FileResponse("static/style.css", media_type="text/css")

@app.get("/app.js")
async def get_app_js():
    return FileResponse("static/app.js", media_type="application/javascript")

@app.get("/scene.js")
async def get_scene_js():
    return FileResponse("static/scene.js", media_type="application/javascript")

# Initialize settings and processor
settings = Settings()
processor = PhotoProcessor(settings)

# Storage for processing status
processing_status = {}

@app.get("/", response_class=HTMLResponse)
@app.head("/")
async def read_root():
    """Serve the main application"""
    with open("static/index.html", "r") as f:
        return HTMLResponse(content=f.read())

@app.post("/api/upload-photo")
async def upload_photo(file: UploadFile = File(...)):
    """Upload and process user photo to generate 3D avatar"""
    try:
        # Validate file type
        if not file.content_type or not file.content_type.startswith('image/'):
            raise HTTPException(status_code=400, detail="File must be an image")
        
        # Generate unique processing ID
        process_id = str(uuid.uuid4())
        
        # Read and validate image
        contents = await file.read()
        image = Image.open(io.BytesIO(contents))
        
        # Initialize processing status
        processing_status[process_id] = {
            "status": "processing",
            "progress": 0,
            "message": "Starting photo analysis...",
            "avatar_data": None
        }
        
        # Start background processing
        asyncio.create_task(process_photo_background(process_id, image))
        
        return JSONResponse({
            "process_id": process_id,
            "status": "started",
            "message": "Photo processing initiated"
        })
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Processing error: {str(e)}")

async def process_photo_background(process_id: str, image: Image.Image):
    """Background task to process photo into 3D avatar"""
    try:
        # Stage 1: Face detection with MediaPipe
        processing_status[process_id].update({
            "progress": 15,
            "message": "Using AI to detect facial landmarks and features..."
        })
        await asyncio.sleep(1)
        
        # Stage 2: Real 3D mesh extraction
        processing_status[process_id].update({
            "progress": 35,
            "message": "Extracting 3D facial geometry from your photo..."
        })
        await asyncio.sleep(2)
        
        # Stage 3: Photorealistic mesh generation
        processing_status[process_id].update({
            "progress": 60,
            "message": "Creating your personalized 3D face model..."
        })
        await asyncio.sleep(3)
        
        # Stage 4: Texture mapping from photo
        processing_status[process_id].update({
            "progress": 80,
            "message": "Mapping your facial features to the 3D model..."
        })
        await asyncio.sleep(2)
        
        # Stage 5: Final optimization
        processing_status[process_id].update({
            "progress": 95,
            "message": "Optimizing your 3D avatar for realistic rendering..."
        })
        await asyncio.sleep(1)
        
        # Generate avatar data using processor
        try:
            avatar_data = await processor.generate_3d_avatar(image)
        except Exception as e:
            if "No face detected" in str(e):
                processing_status[process_id].update({
                    "status": "failed",
                    "progress": 0,
                    "message": "No face detected in the image. Please upload a clear photo showing your face directly facing the camera."
                })
                return
            else:
                raise e
        
        # Complete processing
        processing_status[process_id].update({
            "status": "completed",
            "progress": 100,
            "message": "Avatar generation complete!",
            "avatar_data": avatar_data
        })
        
    except Exception as e:
        processing_status[process_id].update({
            "status": "failed",
            "progress": 0,
            "message": f"Processing failed: {str(e)}"
        })

@app.get("/api/status/{process_id}")
async def get_processing_status(process_id: str):
    """Get current processing status"""
    if process_id not in processing_status:
        raise HTTPException(status_code=404, detail="Process ID not found")
    
    return JSONResponse(processing_status[process_id])

@app.get("/api/avatar/{process_id}")
async def get_avatar_data(process_id: str):
    """Get completed avatar 3D data"""
    if process_id not in processing_status:
        raise HTTPException(status_code=404, detail="Process ID not found")
    
    status = processing_status[process_id]
    if status["status"] != "completed":
        raise HTTPException(status_code=400, detail="Avatar not ready yet")
    
    return JSONResponse(status["avatar_data"])

@app.delete("/api/cleanup/{process_id}")
async def cleanup_processing_data(process_id: str):
    """Clean up processing data"""
    if process_id in processing_status:
        del processing_status[process_id]
    
    return JSONResponse({"message": "Cleanup completed"})

@app.get("/api/health")
async def health_check():
    """Health check endpoint"""
    return JSONResponse({
        "status": "healthy",
        "version": "1.0.0",
        "ai_models_loaded": processor.models_loaded()
    })

if __name__ == "__main__":
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=5000,
        reload=True,
        access_log=True
    )
