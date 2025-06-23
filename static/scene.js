class SceneManager {
    constructor() {
        this.scene = null;
        this.camera = null;
        this.renderer = null;
        this.controls = null;
        this.avatarModel = null;
        this.parkEnvironment = null;
        this.animations = [];
        this.mixer = null;
        this.clock = new THREE.Clock();
        
        this.init();
    }
    
    async init() {
        // Wait for Three.js to be ready
        if (typeof THREE === 'undefined') {
            console.log('Waiting for Three.js to load...');
            setTimeout(() => this.init(), 100);
            return;
        }
        
        console.log('Initializing 3D scene...');
        this.setupScene();
        this.setupLighting();
        await this.createParkEnvironment();
        this.setupControls();
        this.startRenderLoop();
        console.log('3D scene initialized successfully');
    }
    
    setupScene() {
        // Create scene
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x87CEEB); // Sky blue
        
        // Create camera
        this.camera = new THREE.PerspectiveCamera(
            75,
            window.innerWidth / window.innerHeight,
            0.1,
            1000
        );
        this.camera.position.set(0, 1.6, 3);
        
        // Create renderer
        this.renderer = new THREE.WebGLRenderer({ 
            antialias: true,
            alpha: true 
        });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        this.renderer.outputColorSpace = THREE.SRGBColorSpace;
        
        // Append to container
        const container = document.getElementById('three-container');
        if (container) {
            container.appendChild(this.renderer.domElement);
            console.log('Renderer added to DOM');
        } else {
            console.error('three-container not found');
        }
        
        // Debug: Add a simple test cube to verify rendering works
        const testGeometry = new THREE.BoxGeometry(0.2, 0.2, 0.2);
        const testMaterial = new THREE.MeshPhongMaterial({ color: 0xff0000 });
        const testCube = new THREE.Mesh(testGeometry, testMaterial);
        testCube.position.set(1, 1, 0);
        this.scene.add(testCube);
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.renderer.toneMappingExposure = 1.2;
        
        // Add to DOM
        const container = document.getElementById('three-container');
        if (container) {
            container.appendChild(this.renderer.domElement);
        }
        
        // Handle window resize
        window.addEventListener('resize', () => this.onWindowResize());
    }
    
    setupLighting() {
        // Ambient light
        const ambientLight = new THREE.AmbientLight(0x404040, 0.4);
        this.scene.add(ambientLight);
        
        // Main directional light (sun)
        const sunLight = new THREE.DirectionalLight(0xffffff, 1.2);
        sunLight.position.set(10, 20, 5);
        sunLight.castShadow = true;
        sunLight.shadow.mapSize.width = 2048;
        sunLight.shadow.mapSize.height = 2048;
        sunLight.shadow.camera.near = 0.5;
        sunLight.shadow.camera.far = 50;
        sunLight.shadow.camera.left = -10;
        sunLight.shadow.camera.right = 10;
        sunLight.shadow.camera.top = 10;
        sunLight.shadow.camera.bottom = -10;
        this.scene.add(sunLight);
        
        // Fill light
        const fillLight = new THREE.DirectionalLight(0x87CEEB, 0.3);
        fillLight.position.set(-5, 10, -5);
        this.scene.add(fillLight);
        
        // Hemisphere light for natural sky lighting
        const hemisphereLight = new THREE.HemisphereLight(0x87CEEB, 0x228B22, 0.6);
        this.scene.add(hemisphereLight);
    }
    
    async createParkEnvironment() {
        // Ground
        const groundGeometry = new THREE.PlaneGeometry(20, 20);
        const groundMaterial = new THREE.MeshLambertMaterial({ 
            color: 0x228B22,
            side: THREE.DoubleSide
        });
        const ground = new THREE.Mesh(groundGeometry, groundMaterial);
        ground.rotation.x = -Math.PI / 2;
        ground.receiveShadow = true;
        this.scene.add(ground);
        
        // Trees
        await this.createTrees();
        
        // Bench
        this.createBench();
        
        // Flowers and details
        this.createFlowers();
        
        // Particles (leaves, etc.)
        this.createParticles();
    }
    
    async createTrees() {
        const treePositions = [
            { x: -8, z: -8 },
            { x: 8, z: -8 },
            { x: -6, z: 8 },
            { x: 6, z: 6 },
            { x: -10, z: 2 }
        ];
        
        treePositions.forEach(pos => {
            this.createTree(pos.x, pos.z);
        });
    }
    
    createTree(x, z) {
        // Tree trunk
        const trunkGeometry = new THREE.CylinderGeometry(0.2, 0.3, 3, 8);
        const trunkMaterial = new THREE.MeshLambertMaterial({ color: 0x8B4513 });
        const trunk = new THREE.Mesh(trunkGeometry, trunkMaterial);
        trunk.position.set(x, 1.5, z);
        trunk.castShadow = true;
        this.scene.add(trunk);
        
        // Tree foliage
        const foliageGeometry = new THREE.SphereGeometry(2, 12, 8);
        const foliageMaterial = new THREE.MeshLambertMaterial({ color: 0x228B22 });
        const foliage = new THREE.Mesh(foliageGeometry, foliageMaterial);
        foliage.position.set(x, 4, z);
        foliage.castShadow = true;
        this.scene.add(foliage);
        
        // Add some randomness to foliage
        foliage.scale.set(
            0.8 + Math.random() * 0.4,
            0.8 + Math.random() * 0.4,
            0.8 + Math.random() * 0.4
        );
    }
    
    createBench() {
        // Bench seat
        const seatGeometry = new THREE.BoxGeometry(2, 0.1, 0.5);
        const benchMaterial = new THREE.MeshLambertMaterial({ color: 0x8B4513 });
        const seat = new THREE.Mesh(seatGeometry, benchMaterial);
        seat.position.set(0, 0.5, -1);
        seat.castShadow = true;
        this.scene.add(seat);
        
        // Bench back
        const backGeometry = new THREE.BoxGeometry(2, 0.8, 0.1);
        const back = new THREE.Mesh(backGeometry, benchMaterial);
        back.position.set(0, 0.9, -1.2);
        back.castShadow = true;
        this.scene.add(back);
        
        // Bench legs
        const legGeometry = new THREE.BoxGeometry(0.1, 0.5, 0.1);
        const legPositions = [
            { x: -0.8, z: -0.8 },
            { x: 0.8, z: -0.8 },
            { x: -0.8, z: -1.2 },
            { x: 0.8, z: -1.2 }
        ];
        
        legPositions.forEach(pos => {
            const leg = new THREE.Mesh(legGeometry, benchMaterial);
            leg.position.set(pos.x, 0.25, pos.z);
            leg.castShadow = true;
            this.scene.add(leg);
        });
    }
    
    createFlowers() {
        const flowerColors = [0xFF69B4, 0xFF6347, 0xFFD700, 0x9370DB];
        
        for (let i = 0; i < 20; i++) {
            const flowerGeometry = new THREE.SphereGeometry(0.1, 6, 4);
            const flowerMaterial = new THREE.MeshLambertMaterial({ 
                color: flowerColors[Math.floor(Math.random() * flowerColors.length)]
            });
            const flower = new THREE.Mesh(flowerGeometry, flowerMaterial);
            
            // Random position around the scene
            flower.position.set(
                (Math.random() - 0.5) * 15,
                0.1,
                (Math.random() - 0.5) * 15
            );
            
            this.scene.add(flower);
        }
    }
    
    createParticles() {
        const particleCount = 50;
        const particles = new THREE.BufferGeometry();
        const positions = new Float32Array(particleCount * 3);
        const colors = new Float32Array(particleCount * 3);
        
        for (let i = 0; i < particleCount; i++) {
            const i3 = i * 3;
            
            // Position
            positions[i3] = (Math.random() - 0.5) * 20;
            positions[i3 + 1] = Math.random() * 10 + 2;
            positions[i3 + 2] = (Math.random() - 0.5) * 20;
            
            // Color (autumn leaves)
            const leafColors = [
                [1.0, 0.8, 0.2], // Yellow
                [1.0, 0.5, 0.0], // Orange
                [0.8, 0.3, 0.1], // Brown
                [0.2, 0.8, 0.2]  // Green
            ];
            const color = leafColors[Math.floor(Math.random() * leafColors.length)];
            colors[i3] = color[0];
            colors[i3 + 1] = color[1];
            colors[i3 + 2] = color[2];
        }
        
        particles.setAttribute('position', new THREE.BufferAttribute(positions, 3));
        particles.setAttribute('color', new THREE.BufferAttribute(colors, 3));
        
        const particleMaterial = new THREE.PointsMaterial({
            size: 0.1,
            vertexColors: true,
            transparent: true,
            opacity: 0.8
        });
        
        const particleSystem = new THREE.Points(particles, particleMaterial);
        this.scene.add(particleSystem);
        
        // Animate particles
        this.animateParticles(particleSystem);
    }
    
    animateParticles(particleSystem) {
        const animate = () => {
            const time = Date.now() * 0.0005;
            
            const positions = particleSystem.geometry.attributes.position.array;
            for (let i = 0; i < positions.length; i += 3) {
                // Gentle floating motion
                positions[i + 1] += Math.sin(time + i) * 0.01;
                positions[i] += Math.sin(time + i * 0.5) * 0.005;
                
                // Reset if too high
                if (positions[i + 1] > 12) {
                    positions[i + 1] = 2;
                }
            }
            
            particleSystem.geometry.attributes.position.needsUpdate = true;
            requestAnimationFrame(animate);
        };
        animate();
    }
    
    setupControls() {
        if (typeof THREE.OrbitControls !== 'undefined') {
            this.controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
            this.controls.enableDamping = true;
            this.controls.dampingFactor = 0.05;
            this.controls.minDistance = 1;
            this.controls.maxDistance = 10;
            this.controls.maxPolarAngle = Math.PI / 2;
            this.controls.target.set(0, 1, 0);
        }
    }
    
    async loadAvatar(avatarData) {
        if (!avatarData) {
            console.error('No avatar data provided');
            return;
        }
        
        // Create a simple avatar representation
        // In a real implementation, this would load the actual 3D model
        await this.createSimpleAvatar(avatarData);
        
        // Start avatar animations
        this.startAvatarAnimations();
    }
    
    async createSimpleAvatar(avatarData) {
        // Remove existing avatar
        if (this.avatarModel) {
            this.scene.remove(this.avatarModel);
        }
        
        // Create avatar group
        this.avatarModel = new THREE.Group();
        
        // Create realistic head from user's facial data
        let head;
        if (avatarData && avatarData.vertices && avatarData.faces) {
            head = this.createRealisticHead(avatarData);
        } else {
            // Fallback to simple head
            const headGeometry = new THREE.SphereGeometry(0.3, 16, 16);
            const headMaterial = new THREE.MeshPhongMaterial({ 
                color: 0xFFDBB3,
                shininess: 30
            });
            head = new THREE.Mesh(headGeometry, headMaterial);
        }
        
        head.position.y = 1.7;
        head.castShadow = true;
        this.avatarModel.add(head);
        
        // Eyes
        const eyeGeometry = new THREE.SphereGeometry(0.05, 8, 8);
        const eyeMaterial = new THREE.MeshPhongMaterial({ color: 0x000000 });
        
        const leftEye = new THREE.Mesh(eyeGeometry, eyeMaterial);
        leftEye.position.set(-0.1, 1.75, 0.25);
        this.avatarModel.add(leftEye);
        
        const rightEye = new THREE.Mesh(eyeGeometry, eyeMaterial);
        rightEye.position.set(0.1, 1.75, 0.25);
        this.avatarModel.add(rightEye);
        
        // Body
        const bodyGeometry = new THREE.CylinderGeometry(0.25, 0.3, 1.2, 12);
        const bodyMaterial = new THREE.MeshLambertMaterial({ color: 0x4169E1 });
        const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
        body.position.y = 1;
        body.castShadow = true;
        this.avatarModel.add(body);
        
        // Arms
        const armGeometry = new THREE.CylinderGeometry(0.08, 0.1, 0.8, 8);
        const armMaterial = new THREE.MeshLambertMaterial({ color: 0xFFDBB3 });
        
        const leftArm = new THREE.Mesh(armGeometry, armMaterial);
        leftArm.position.set(-0.4, 1.2, 0);
        leftArm.rotation.z = 0.3;
        leftArm.castShadow = true;
        this.avatarModel.add(leftArm);
        
        const rightArm = new THREE.Mesh(armGeometry, armMaterial);
        rightArm.position.set(0.4, 1.2, 0);
        rightArm.rotation.z = -0.3;
        rightArm.castShadow = true;
        this.avatarModel.add(rightArm);
        
        // Legs
        const legGeometry = new THREE.CylinderGeometry(0.1, 0.12, 1, 8);
        const legMaterial = new THREE.MeshLambertMaterial({ color: 0x2F4F4F });
        
        const leftLeg = new THREE.Mesh(legGeometry, legMaterial);
        leftLeg.position.set(-0.15, 0.1, 0);
        leftLeg.castShadow = true;
        this.avatarModel.add(leftLeg);
        
        const rightLeg = new THREE.Mesh(legGeometry, legMaterial);
        rightLeg.position.set(0.15, 0.1, 0);
        rightLeg.castShadow = true;
        this.avatarModel.add(rightLeg);
        
        // Position avatar prominently in scene
        this.avatarModel.position.set(0, 0.5, 0);
        
        // Add to scene
        this.scene.add(this.avatarModel);
        
        // Store references for animation
        this.avatarParts = {
            head,
            leftEye,
            rightEye,
            body,
            leftArm,
            rightArm
        };
    }
    
    createRealisticHead(avatarData) {
        try {
            console.log('Avatar data received:', avatarData);
            
            // Create geometry from real facial landmarks
            const geometry = new THREE.BufferGeometry();
            
            // Extract vertex data
            const vertices = avatarData.vertices;
            const faces = avatarData.faces;
            
            if (!vertices || !faces || vertices.length === 0) {
                console.error('Invalid avatar data - missing vertices or faces');
                throw new Error('Invalid avatar data');
            }
            
            console.log('Creating realistic head with', vertices.length, 'vertices and', faces.length, 'faces');
            
            // Convert vertices to Float32Array
            const positions = new Float32Array(vertices.length * 3);
            for (let i = 0; i < vertices.length; i++) {
                positions[i * 3] = vertices[i][0];
                positions[i * 3 + 1] = vertices[i][1];
                positions[i * 3 + 2] = vertices[i][2];
            }
            geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
            
            // Convert faces to indices
            const indices = [];
            for (let i = 0; i < faces.length; i++) {
                if (faces[i].length >= 3) {
                    // Ensure indices are within bounds
                    const v0 = Math.min(faces[i][0], vertices.length - 1);
                    const v1 = Math.min(faces[i][1], vertices.length - 1);
                    const v2 = Math.min(faces[i][2], vertices.length - 1);
                    indices.push(v0, v1, v2);
                }
            }
            console.log('Created', indices.length / 3, 'triangular faces');
            geometry.setIndex(indices);
            
            // Add UV coordinates if available
            if (avatarData.uvs) {
                const uvs = new Float32Array(avatarData.uvs.length * 2);
                for (let i = 0; i < avatarData.uvs.length; i++) {
                    uvs[i * 2] = avatarData.uvs[i][0];
                    uvs[i * 2 + 1] = avatarData.uvs[i][1];
                }
                geometry.setAttribute('uv', new THREE.BufferAttribute(uvs, 2));
            }
            
            // Calculate normals for proper lighting
            geometry.computeVertexNormals();
            geometry.computeBoundingBox();
            
            console.log('Geometry created with', positions.length/3, 'vertices and', indices.length/3, 'faces');
            console.log('Bounding box:', geometry.boundingBox);
            
            // Create material with user's photo texture
            let material;
            if (avatarData.textures && avatarData.textures.diffuse) {
                const textureLoader = new THREE.TextureLoader();
                const texture = textureLoader.load(
                    'data:image/png;base64,' + avatarData.textures.diffuse,
                    (tex) => {
                        console.log('Photo texture loaded successfully', tex.image.width, 'x', tex.image.height);
                        // Force render update when texture loads
                        if (this.renderer) this.renderer.render(this.scene, this.camera);
                    },
                    undefined,
                    (error) => console.error('Failed to load texture:', error)
                );
                texture.wrapS = THREE.ClampToEdgeWrapping;
                texture.wrapT = THREE.ClampToEdgeWrapping;
                texture.flipY = false;
                
                material = new THREE.MeshPhongMaterial({
                    map: texture,
                    shininess: 30,
                    side: THREE.DoubleSide,
                    transparent: false
                });
            } else {
                console.warn('No texture data found, using default material');
                material = new THREE.MeshPhongMaterial({
                    color: 0xFFDBB3,
                    shininess: 30,
                    side: THREE.DoubleSide
                });
            }
            
            const mesh = new THREE.Mesh(geometry, material);
            mesh.castShadow = true;
            mesh.receiveShadow = true;
            
            // Scale and position properly for visibility
            mesh.scale.set(3, 3, 3);
            mesh.position.set(0, 1.5, 0);
            
            console.log('Successfully created realistic head mesh');
            console.log('Mesh bounding box:', mesh.geometry.boundingBox);
            
            return mesh;
            
        } catch (error) {
            console.error('Failed to create realistic head:', error);
            
            // Fallback to simple sphere
            const headGeometry = new THREE.SphereGeometry(0.3, 16, 16);
            const headMaterial = new THREE.MeshPhongMaterial({ 
                color: 0xFFDBB3,
                shininess: 30
            });
            return new THREE.Mesh(headGeometry, headMaterial);
        }
    }
    
    startAvatarAnimations() {
        if (!this.avatarModel || !this.avatarParts) return;
        
        // Breathing animation
        this.breathingAnimation();
        
        // Blinking animation
        this.blinkingAnimation();
        
        // Subtle head movements
        this.headMovementAnimation();
    }
    
    breathingAnimation() {
        const animate = () => {
            if (!this.avatarParts.body) return;
            
            const time = Date.now() * 0.002;
            const breathScale = 1 + Math.sin(time) * 0.05;
            this.avatarParts.body.scale.set(breathScale, 1, breathScale);
            
            setTimeout(() => requestAnimationFrame(animate), 50);
        };
        animate();
    }
    
    blinkingAnimation() {
        const blink = () => {
            if (!this.avatarParts.leftEye || !this.avatarParts.rightEye) return;
            
            // Scale eyes down (blink)
            this.avatarParts.leftEye.scale.y = 0.1;
            this.avatarParts.rightEye.scale.y = 0.1;
            
            setTimeout(() => {
                // Scale eyes back up
                this.avatarParts.leftEye.scale.y = 1;
                this.avatarParts.rightEye.scale.y = 1;
            }, 100);
            
            // Random blink interval
            setTimeout(blink, 2000 + Math.random() * 3000);
        };
        
        // Start first blink after delay
        setTimeout(blink, 1000);
    }
    
    headMovementAnimation() {
        const animate = () => {
            if (!this.avatarParts.head) return;
            
            const time = Date.now() * 0.001;
            this.avatarParts.head.rotation.y = Math.sin(time * 0.5) * 0.2;
            this.avatarParts.head.rotation.x = Math.sin(time * 0.3) * 0.1;
            
            setTimeout(() => requestAnimationFrame(animate), 100);
        };
        animate();
    }
    
    startRenderLoop() {
        const animate = () => {
            requestAnimationFrame(animate);
            
            const delta = this.clock.getDelta();
            
            // Update controls
            if (this.controls) {
                this.controls.update();
            }
            
            // Update animations
            if (this.mixer) {
                this.mixer.update(delta);
            }
            
            // Render scene
            this.renderer.render(this.scene, this.camera);
        };
        animate();
    }
    
    onWindowResize() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }
    
    resetCamera() {
        if (this.controls) {
            this.controls.reset();
            this.camera.position.set(0, 1.6, 3);
            this.controls.target.set(0, 1, 0);
        }
    }
    
    captureScreenshot() {
        const link = document.createElement('a');
        link.download = 'mirrorworld-avatar.png';
        link.href = this.renderer.domElement.toDataURL();
        link.click();
    }
}

// Initialize scene manager when Three.js is ready
window.addEventListener('load', () => {
    if (typeof THREE !== 'undefined') {
        window.SceneManager = new SceneManager();
    }
});
