class MirrorWorldApp {
    constructor() {
        this.currentScreen = 'welcome-screen';
        this.currentProcessId = null;
        this.processingInterval = null;
        this.selectedPhoto = null;
        
        this.init();
    }
    
    init() {
        // Initialize Feather icons
        feather.replace();
        
        // Set up event listeners
        this.setupEventListeners();
        
        // Initialize the app state
        this.showScreen('welcome-screen');
    }
    
    setupEventListeners() {
        // Photo input change
        document.getElementById('photo-input').addEventListener('change', (e) => {
            this.handlePhotoSelection(e);
        });
        
        // Drag and drop support
        const uploadArea = document.getElementById('upload-area');
        
        uploadArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            uploadArea.classList.add('drag-over');
        });
        
        uploadArea.addEventListener('dragleave', () => {
            uploadArea.classList.remove('drag-over');
        });
        
        uploadArea.addEventListener('drop', (e) => {
            e.preventDefault();
            uploadArea.classList.remove('drag-over');
            
            const files = e.dataTransfer.files;
            if (files.length > 0 && files[0].type.startsWith('image/')) {
                this.handlePhotoFile(files[0]);
            }
        });
        
        // Click to upload
        uploadArea.addEventListener('click', () => {
            document.getElementById('photo-input').click();
        });
    }
    
    showScreen(screenId) {
        // Hide current screen
        const currentScreenEl = document.querySelector('.screen.active');
        if (currentScreenEl) {
            currentScreenEl.classList.remove('active');
        }
        
        // Show new screen
        const newScreenEl = document.getElementById(screenId);
        if (newScreenEl) {
            setTimeout(() => {
                newScreenEl.classList.add('active');
            }, 100);
        }
        
        this.currentScreen = screenId;
        
        // Update Feather icons after screen change
        setTimeout(() => feather.replace(), 200);
    }
    
    handlePhotoSelection(event) {
        const file = event.target.files[0];
        if (file && file.type.startsWith('image/')) {
            this.handlePhotoFile(file);
        }
    }
    
    handlePhotoFile(file) {
        this.selectedPhoto = file;
        
        // Create preview
        const reader = new FileReader();
        reader.onload = (e) => {
            const previewImg = document.getElementById('preview-image');
            previewImg.src = e.target.result;
            
            // Show preview and process button
            document.getElementById('photo-preview').style.display = 'block';
            document.getElementById('process-btn').style.display = 'inline-flex';
            
            // Hide upload area
            document.getElementById('upload-area').style.display = 'none';
        };
        reader.readAsDataURL(file);
    }
    
    async processPhoto() {
        if (!this.selectedPhoto) {
            this.showError('Please select a photo first');
            return;
        }
        
        // Show processing screen
        this.showScreen('processing-screen');
        
        // Upload photo and start processing
        try {
            const formData = new FormData();
            formData.append('file', this.selectedPhoto);
            
            const response = await fetch('/api/upload-photo', {
                method: 'POST',
                body: formData
            });
            
            if (!response.ok) {
                throw new Error('Upload failed');
            }
            
            const result = await response.json();
            this.currentProcessId = result.process_id;
            
            // Start polling for status
            this.startStatusPolling();
            
        } catch (error) {
            this.showError('Failed to upload photo: ' + error.message);
            this.showScreen('upload-screen');
        }
    }
    
    startStatusPolling() {
        if (this.processingInterval) {
            clearInterval(this.processingInterval);
        }
        
        this.processingInterval = setInterval(async () => {
            await this.checkProcessingStatus();
        }, 1000);
    }
    
    async checkProcessingStatus() {
        if (!this.currentProcessId) return;
        
        try {
            const response = await fetch(`/api/status/${this.currentProcessId}`);
            if (!response.ok) {
                throw new Error('Status check failed');
            }
            
            const status = await response.json();
            this.updateProcessingUI(status);
            
            if (status.status === 'completed') {
                clearInterval(this.processingInterval);
                await this.loadAvatarScene(status.avatar_data);
            } else if (status.status === 'failed') {
                clearInterval(this.processingInterval);
                this.showError(status.message);
                this.showScreen('upload-screen');
            }
            
        } catch (error) {
            clearInterval(this.processingInterval);
            this.showError('Failed to check processing status');
            this.showScreen('upload-screen');
        }
    }
    
    updateProcessingUI(status) {
        // Update progress bar
        const progressBar = document.getElementById('progress-bar');
        progressBar.style.width = status.progress + '%';
        
        // Update message
        document.getElementById('processing-message').textContent = status.message;
        
        // Update step badges based on progress
        const steps = ['segmentation', 'depth', 'mesh', 'animation', 'optimization'];
        const currentStepIndex = Math.floor(status.progress / 20);
        
        steps.forEach((step, index) => {
            const badge = document.querySelector(`[data-step="${step}"]`);
            if (badge) {
                if (index <= currentStepIndex) {
                    badge.classList.add('active');
                } else {
                    badge.classList.remove('active');
                }
            }
        });
    }
    
    async loadAvatarScene(avatarData) {
        // Show 3D scene screen
        this.showScreen('scene-screen');
        
        // Initialize 3D scene
        if (window.SceneManager) {
            await window.SceneManager.loadAvatar(avatarData);
        }
    }
    
    showError(message) {
        // Create error notification
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message position-fixed top-0 start-50 translate-middle-x mt-3';
        errorDiv.style.zIndex = '9999';
        errorDiv.textContent = message;
        
        document.body.appendChild(errorDiv);
        
        // Remove after 5 seconds
        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.parentNode.removeChild(errorDiv);
            }
        }, 5000);
    }
    
    showSuccess(message) {
        // Create success notification
        const successDiv = document.createElement('div');
        successDiv.className = 'success-message position-fixed top-0 start-50 translate-middle-x mt-3';
        successDiv.style.zIndex = '9999';
        successDiv.textContent = message;
        
        document.body.appendChild(successDiv);
        
        // Remove after 3 seconds
        setTimeout(() => {
            if (successDiv.parentNode) {
                successDiv.parentNode.removeChild(successDiv);
            }
        }, 3000);
    }
    
    cleanup() {
        if (this.processingInterval) {
            clearInterval(this.processingInterval);
        }
        
        if (this.currentProcessId) {
            // Clean up processing data on server
            fetch(`/api/cleanup/${this.currentProcessId}`, {
                method: 'DELETE'
            }).catch(() => {
                // Ignore cleanup errors
            });
        }
    }
}

// Global functions for UI interactions
function startOnboarding() {
    app.showScreen('onboarding-screen');
}

function requestPermissions() {
    // Simulate permission request
    app.showSuccess('Permissions granted');
    setTimeout(() => {
        app.showScreen('upload-screen');
    }, 1000);
}

function processPhoto() {
    app.processPhoto();
}

function resetCamera() {
    if (window.SceneManager) {
        window.SceneManager.resetCamera();
    }
}

function captureScreenshot() {
    if (window.SceneManager) {
        window.SceneManager.captureScreenshot();
    }
}

function startOver() {
    app.cleanup();
    app.selectedPhoto = null;
    
    // Reset UI
    document.getElementById('photo-preview').style.display = 'none';
    document.getElementById('process-btn').style.display = 'none';
    document.getElementById('upload-area').style.display = 'block';
    document.getElementById('preview-image').src = '';
    document.getElementById('photo-input').value = '';
    
    // Go back to welcome
    app.showScreen('welcome-screen');
}

// Initialize app when DOM is loaded
let app;
document.addEventListener('DOMContentLoaded', () => {
    app = new MirrorWorldApp();
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (app) {
        app.cleanup();
    }
});
