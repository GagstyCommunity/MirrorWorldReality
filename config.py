import os
from typing import Optional

class Settings:
    """Application configuration settings"""
    
    # API Configuration
    API_HOST: str = "0.0.0.0"
    API_PORT: int = 8000
    DEBUG: bool = os.getenv("DEBUG", "false").lower() == "true"
    
    # AI Model Configuration
    MODEL_PATH: str = os.getenv("MODEL_PATH", "./models")
    DEVICE: str = os.getenv("DEVICE", "cpu")  # cpu, cuda, mps
    MAX_IMAGE_SIZE: int = int(os.getenv("MAX_IMAGE_SIZE", "2048"))
    
    # Processing Configuration
    MAX_CONCURRENT_JOBS: int = int(os.getenv("MAX_CONCURRENT_JOBS", "4"))
    PROCESSING_TIMEOUT: int = int(os.getenv("PROCESSING_TIMEOUT", "300"))  # seconds
    CLEANUP_INTERVAL: int = int(os.getenv("CLEANUP_INTERVAL", "3600"))  # seconds
    
    # Storage Configuration
    UPLOAD_PATH: str = os.getenv("UPLOAD_PATH", "./uploads")
    OUTPUT_PATH: str = os.getenv("OUTPUT_PATH", "./outputs")
    MAX_STORAGE_DAYS: int = int(os.getenv("MAX_STORAGE_DAYS", "7"))
    
    # Cloud Storage (Optional)
    AWS_ACCESS_KEY_ID: Optional[str] = os.getenv("AWS_ACCESS_KEY_ID")
    AWS_SECRET_ACCESS_KEY: Optional[str] = os.getenv("AWS_SECRET_ACCESS_KEY")
    AWS_BUCKET_NAME: Optional[str] = os.getenv("AWS_BUCKET_NAME")
    AWS_REGION: str = os.getenv("AWS_REGION", "us-west-2")
    
    # Security
    MAX_FILE_SIZE: int = int(os.getenv("MAX_FILE_SIZE", "10485760"))  # 10MB
    ALLOWED_EXTENSIONS: set = {".jpg", ".jpeg", ".png", ".webp"}
    CORS_ORIGINS: list = os.getenv("CORS_ORIGINS", "*").split(",")
    
    # Performance
    ENABLE_GPU: bool = os.getenv("ENABLE_GPU", "false").lower() == "true"
    MODEL_PRECISION: str = os.getenv("MODEL_PRECISION", "fp16")  # fp32, fp16
    BATCH_SIZE: int = int(os.getenv("BATCH_SIZE", "1"))
    
    # Monitoring
    LOG_LEVEL: str = os.getenv("LOG_LEVEL", "INFO")
    ENABLE_METRICS: bool = os.getenv("ENABLE_METRICS", "false").lower() == "true"
    METRICS_PORT: int = int(os.getenv("METRICS_PORT", "9090"))
    
    # Quality Settings
    DEFAULT_QUALITY: str = os.getenv("DEFAULT_QUALITY", "high")  # low, medium, high
    TEXTURE_RESOLUTION: int = int(os.getenv("TEXTURE_RESOLUTION", "512"))
    MESH_DETAIL_LEVEL: str = os.getenv("MESH_DETAIL_LEVEL", "medium")  # low, medium, high
    
    # Animation Settings
    ENABLE_FACIAL_ANIMATION: bool = os.getenv("ENABLE_FACIAL_ANIMATION", "true").lower() == "true"
    ANIMATION_FPS: int = int(os.getenv("ANIMATION_FPS", "30"))
    BLEND_SHAPE_COUNT: int = int(os.getenv("BLEND_SHAPE_COUNT", "52"))
    
    # Error Handling
    MAX_RETRY_ATTEMPTS: int = int(os.getenv("MAX_RETRY_ATTEMPTS", "3"))
    RETRY_DELAY: float = float(os.getenv("RETRY_DELAY", "1.0"))
    
    def __init__(self):
        # Create necessary directories
        os.makedirs(self.UPLOAD_PATH, exist_ok=True)
        os.makedirs(self.OUTPUT_PATH, exist_ok=True)
        os.makedirs(self.MODEL_PATH, exist_ok=True)
        
        # Validate settings
        self._validate_settings()
    
    def _validate_settings(self):
        """Validate configuration settings"""
        if self.MAX_IMAGE_SIZE <= 0:
            raise ValueError("MAX_IMAGE_SIZE must be positive")
        
        if self.MAX_CONCURRENT_JOBS <= 0:
            raise ValueError("MAX_CONCURRENT_JOBS must be positive")
        
        if self.PROCESSING_TIMEOUT <= 0:
            raise ValueError("PROCESSING_TIMEOUT must be positive")
        
        if self.DEFAULT_QUALITY not in ["low", "medium", "high"]:
            raise ValueError("DEFAULT_QUALITY must be low, medium, or high")
        
        if self.DEVICE not in ["cpu", "cuda", "mps"]:
            raise ValueError("DEVICE must be cpu, cuda, or mps")
    
    @property
    def use_cloud_storage(self) -> bool:
        """Check if cloud storage is configured"""
        return bool(self.AWS_ACCESS_KEY_ID and self.AWS_SECRET_ACCESS_KEY and self.AWS_BUCKET_NAME)
    
    @property
    def quality_settings(self) -> dict:
        """Get quality-specific settings"""
        quality_map = {
            "low": {
                "texture_resolution": 256,
                "mesh_vertices": 1000,
                "animation_fps": 15,
                "processing_timeout": 60
            },
            "medium": {
                "texture_resolution": 512,
                "mesh_vertices": 5000,
                "animation_fps": 24,
                "processing_timeout": 180
            },
            "high": {
                "texture_resolution": 1024,
                "mesh_vertices": 15000,
                "animation_fps": 30,
                "processing_timeout": 300
            }
        }
        return quality_map.get(self.DEFAULT_QUALITY, quality_map["medium"])
    
    def get_model_config(self) -> dict:
        """Get AI model configuration"""
        return {
            "device": self.DEVICE,
            "precision": self.MODEL_PRECISION,
            "batch_size": self.BATCH_SIZE,
            "enable_gpu": self.ENABLE_GPU,
            "max_image_size": self.MAX_IMAGE_SIZE
        }
    
    def get_storage_config(self) -> dict:
        """Get storage configuration"""
        config = {
            "upload_path": self.UPLOAD_PATH,
            "output_path": self.OUTPUT_PATH,
            "max_storage_days": self.MAX_STORAGE_DAYS,
            "max_file_size": self.MAX_FILE_SIZE,
            "allowed_extensions": self.ALLOWED_EXTENSIONS
        }
        
        if self.use_cloud_storage:
            config.update({
                "aws_access_key_id": self.AWS_ACCESS_KEY_ID,
                "aws_secret_access_key": self.AWS_SECRET_ACCESS_KEY,
                "aws_bucket_name": self.AWS_BUCKET_NAME,
                "aws_region": self.AWS_REGION
            })
        
        return config
