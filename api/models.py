from pydantic import BaseModel
from typing import Optional, Dict, Any, List
from enum import Enum

class ProcessingStatus(str, Enum):
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"

class Avatar3DModel(BaseModel):
    """3D Avatar model data structure"""
    
    # Basic info
    id: str
    created_at: str
    
    # Mesh data
    vertices: List[List[float]]
    faces: List[List[int]]
    textures: Dict[str, str]  # Base64 encoded textures
    
    # Animation data
    blend_shapes: Dict[str, List[float]]
    skeleton: Optional[Dict[str, Any]]
    animations: List[Dict[str, Any]]
    
    # Rendering properties
    materials: Dict[str, Dict[str, Any]]
    lighting_params: Dict[str, float]
    
    # Metadata
    source_image_hash: str
    generation_params: Dict[str, Any]
    
class ProcessingRequest(BaseModel):
    """Photo processing request"""
    image_data: str  # Base64 encoded image
    user_preferences: Optional[Dict[str, Any]] = None
    quality_level: str = "high"  # low, medium, high
    
class ProcessingResponse(BaseModel):
    """Processing response"""
    process_id: str
    status: ProcessingStatus
    progress: int
    message: str
    avatar_data: Optional[Avatar3DModel] = None
    estimated_time: Optional[int] = None  # seconds
    
class AvatarMetrics(BaseModel):
    """Avatar quality metrics"""
    geometry_quality: float
    texture_resolution: int
    animation_smoothness: float
    rendering_performance: float
    
class UserFeedback(BaseModel):
    """User feedback on avatar quality"""
    process_id: str
    rating: int  # 1-5 stars
    comments: Optional[str]
    issues: List[str]
