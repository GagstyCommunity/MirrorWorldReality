// Global variables
let currentScreen = 'upload';
let processId = null;
let processingInterval = null;

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    initializeFeatherIcons();
    initializeFileUpload();
    initializeEventListeners();
});

function initializeFeatherIcons() {
    // Replace feather icons
    if (typeof feather !== 'undefined') {
        feather.replace();
    }
}

function initializeFileUpload() {
    const photoInput = document.getElementById('photo-input');
    if (photoInput) {
        photoInput.addEventListener('change', handlePhotoSelection);
    }
}

function initializeEventListeners() {
    // Set up other event listeners
    const uploadButton = document.getElementById('upload-btn');
    if (uploadButton) {
        uploadButton.addEventListener('click', () => {
            document.getElementById('photo-input').click();
        });
    }
}

// Add missing startOnboarding function
function startOnboarding() {
    console.log('Starting onboarding process...');
    showScreen('onboarding');
}

// Add missing requestPermissions function
function requestPermissions() {
    console.log('Requesting permissions...');
    showScreen('upload');
}

function showScreen(screenId) {
    // Hide all screens
    const screens = document.querySelectorAll('.screen');
    screens.forEach(screen => {
        screen.style.display = 'none';
    });

    // Show target screen
    const targetScreen = document.getElementById(screenId + '-screen');
    if (targetScreen) {
        targetScreen.style.display = 'block';
        currentScreen = screenId;
    }
}

function handlePhotoSelection(event) {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
        alert('Please select a valid image file.');
        return;
    }

    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
        alert('File size must be less than 10MB.');
        return;
    }

    // Show preview
    const reader = new FileReader();
    reader.onload = function(e) {
        const preview = document.getElementById('photo-preview');
        const previewImg = document.getElementById('preview-image');

        if (preview && previewImg) {
            previewImg.src = e.target.result;
            preview.style.display = 'block';
        }
    };
    reader.readAsDataURL(file);

    // Upload and process the photo
    uploadPhoto(file);
}

async function uploadPhoto(file) {
    try {
        showScreen('processing');

        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch('/api/upload-photo', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Upload failed: ${response.statusText}`);
        }

        const result = await response.json();
        processId = result.process_id;

        // Start monitoring processing status
        monitorProcessing();

    } catch (error) {
        console.error('Upload error:', error);
        showError('Upload failed: ' + error.message);
    }
}

function monitorProcessing() {
    if (!processId) return;

    processingInterval = setInterval(async () => {
        try {
            const response = await fetch(`/api/status/${processId}`);
            const status = await response.json();

            updateProcessingUI(status);

            if (status.status === 'completed') {
                clearInterval(processingInterval);
                loadAvatar(status.avatar_data);
            } else if (status.status === 'failed') {
                clearInterval(processingInterval);
                showError(status.message);
            }

        } catch (error) {
            console.error('Status check error:', error);
            clearInterval(processingInterval);
            showError('Failed to check processing status');
        }
    }, 1000);
}

function updateProcessingUI(status) {
    const progressBar = document.getElementById('progress-bar');
    const processingMessage = document.getElementById('processing-message');

    if (progressBar) {
        progressBar.style.width = status.progress + '%';
        progressBar.setAttribute('aria-valuenow', status.progress);
    }

    if (processingMessage) {
        processingMessage.textContent = status.message || 'Processing...';
    }

    // Update processing step indicators
    const steps = document.querySelectorAll('.step-badge');
    const currentStep = Math.floor((status.progress / 100) * steps.length);
    
    steps.forEach((step, index) => {
        if (index <= currentStep) {
            step.classList.add('completed');
        } else {
            step.classList.remove('completed');
        }
    });
}

function loadAvatar(avatarData) {
    try {
        showScreen('scene');

        // Initialize 3D scene with avatar data
        if (window.SceneManager) {
            window.SceneManager.loadAvatar(avatarData);
        } else {
            console.warn('3D scene manager not available');
        }

    } catch (error) {
        console.error('Avatar loading error:', error);
        showError('Failed to load avatar: ' + error.message);
    }
}

// Add missing functions
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
    resetApplication();
    showScreen('welcome');
}

function showError(message) {
    const errorDiv = document.getElementById('error-message');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
    } else {
        alert(message);
    }
}

function resetApplication() {
    // Clean up
    if (processingInterval) {
        clearInterval(processingInterval);
    }

    if (processId) {
        fetch(`/api/cleanup/${processId}`, { method: 'DELETE' })
            .catch(err => console.error('Cleanup error:', err));
    }

    // Reset state
    processId = null;
    processingInterval = null;

    // Reset UI
    const photoInput = document.getElementById('photo-input');
    if (photoInput) {
        photoInput.value = '';
    }

    const preview = document.getElementById('photo-preview');
    if (preview) {
        preview.style.display = 'none';
    }

    showScreen('upload');
}