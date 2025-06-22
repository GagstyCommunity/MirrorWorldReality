using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MirrorWorld
{
    public class ParkEnvironmentManager : MonoBehaviour
    {
        [Header("Environment Settings")]
        public Material skyboxMaterial;
        public Color fogColor = new Color(0.5f, 0.7f, 0.9f, 1f);
        public float fogDensity = 0.01f;
        public bool enableFog = true;
        
        [Header("Lighting")]
        public Light sunLight;
        public Light fillLight;
        public AnimationCurve sunIntensityCurve = AnimationCurve.Linear(0, 0.8f, 1, 1.2f);
        public Gradient sunColorGradient;
        public float dayDuration = 300f; // 5 minutes for full day cycle
        
        [Header("Ground")]
        public GameObject groundPrefab;
        public Material grassMaterial;
        public float groundSize = 20f;
        
        [Header("Trees")]
        public GameObject[] treePrefabs;
        public int treeCount = 8;
        public float treeSpawnRadius = 12f;
        public Vector2 treeScaleRange = new Vector2(0.8f, 1.2f);
        
        [Header("Bench")]
        public GameObject benchPrefab;
        public Vector3 benchPosition = new Vector3(0, 0, -1.5f);
        
        [Header("Flowers")]
        public GameObject[] flowerPrefabs;
        public int flowerCount = 25;
        public float flowerSpawnRadius = 10f;
        
        [Header("Particles")]
        public ParticleSystem windParticles;
        public ParticleSystem leafParticles;
        public int maxParticles = 50;
        
        [Header("Audio")]
        public AudioSource ambientAudioSource;
        public AudioClip[] birdSounds;
        public AudioClip windSound;
        public float ambientVolume = 0.3f;
        
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private float currentTimeOfDay = 0.5f; // Start at midday
        private bool isDayNightCycleEnabled = true;
        
        public static ParkEnvironmentManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            StartCoroutine(InitializeEnvironment());
        }
        
        private IEnumerator InitializeEnvironment()
        {
            Debug.Log("Initializing MirrorWorld Park Environment...");
            
            SetupLighting();
            yield return StartCoroutine(CreateGround());
            yield return StartCoroutine(CreateTrees());
            yield return StartCoroutine(CreateBench());
            yield return StartCoroutine(CreateFlowers());
            SetupParticles();
            SetupAudio();
            SetupSkybox();
            SetupFog();
            
            Debug.Log("Park Environment initialized successfully!");
        }
        
        private void SetupLighting()
        {
            // Find or create sun light
            if (sunLight == null)
            {
                GameObject sunLightObj = new GameObject("Sun Light");
                sunLight = sunLightObj.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            }
            
            sunLight.shadows = LightShadows.Soft;
            sunLight.shadowStrength = 0.8f;
            sunLight.shadowResolution = LightShadowResolution.High;
            
            // Find or create fill light
            if (fillLight == null)
            {
                GameObject fillLightObj = new GameObject("Fill Light");
                fillLight = fillLightObj.AddComponent<Light>();
                fillLight.type = LightType.Directional;
                fillLight.transform.rotation = Quaternion.Euler(-30f, -45f, 0f);
            }
            
            fillLight.intensity = 0.3f;
            fillLight.color = new Color(0.5f, 0.7f, 1f);
            fillLight.shadows = LightShadows.None;
            
            // Setup color gradient if not set
            if (sunColorGradient.colorKeys.Length == 0)
            {
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(new Color(1f, 0.8f, 0.6f), 0f); // Dawn
                colorKeys[1] = new GradientColorKey(Color.white, 0.5f); // Midday
                colorKeys[2] = new GradientColorKey(new Color(1f, 0.6f, 0.4f), 1f); // Dusk
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);
                
                sunColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
        
        private IEnumerator CreateGround()
        {
            if (groundPrefab != null)
            {
                GameObject ground = Instantiate(groundPrefab);
                ground.name = "Park Ground";
                ground.transform.localScale = Vector3.one * groundSize;
                spawnedObjects.Add(ground);
            }
            else
            {
                // Create procedural ground
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Park Ground";
                ground.transform.localScale = Vector3.one * groundSize;
                
                if (grassMaterial != null)
                {
                    ground.GetComponent<MeshRenderer>().material = grassMaterial;
                }
                else
                {
                    // Create default grass material
                    Material grass = new Material(Shader.Find("Standard"));
                    grass.color = new Color(0.2f, 0.6f, 0.2f);
                    grass.SetFloat("_Metallic", 0f);
                    grass.SetFloat("_Glossiness", 0.1f);
                    ground.GetComponent<MeshRenderer>().material = grass;
                }
                
                spawnedObjects.Add(ground);
            }
            
            yield return null;
        }
        
        private IEnumerator CreateTrees()
        {
            for (int i = 0; i < treeCount; i++)
            {
                // Generate random position on circle
                float angle = i * (360f / treeCount) + Random.Range(-30f, 30f);
                float radius = Random.Range(treeSpawnRadius * 0.7f, treeSpawnRadius);
                
                Vector3 position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );
                
                GameObject tree;
                
                if (treePrefabs != null && treePrefabs.Length > 0)
                {
                    GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    tree = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                }
                else
                {
                    tree = CreateProceduralTree(position);
                }
                
                // Random scale
                float scale = Random.Range(treeScaleRange.x, treeScaleRange.y);
                tree.transform.localScale = Vector3.one * scale;
                
                tree.name = $"Tree_{i}";
                spawnedObjects.Add(tree);
                
                if (i % 2 == 0) yield return null; // Spread across frames
            }
        }
        
        private GameObject CreateProceduralTree(Vector3 position)
        {
            GameObject tree = new GameObject("Procedural Tree");
            tree.transform.position = position;
            
            // Create trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
            trunk.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
            
            Material trunkMaterial = new Material(Shader.Find("Standard"));
            trunkMaterial.color = new Color(0.4f, 0.2f, 0.1f);
            trunk.GetComponent<MeshRenderer>().material = trunkMaterial;
            
            // Create foliage
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.transform.SetParent(tree.transform);
            foliage.transform.localPosition = new Vector3(0, 3.5f, 0);
            foliage.transform.localScale = Vector3.one * Random.Range(1.5f, 2.2f);
            
            Material foliageMaterial = new Material(Shader.Find("Standard"));
            foliageMaterial.color = new Color(0.1f, 0.5f, 0.1f);
            foliageMaterial.SetFloat("_Metallic", 0f);
            foliageMaterial.SetFloat("_Glossiness", 0.1f);
            foliage.GetComponent<MeshRenderer>().material = foliageMaterial;
            
            return tree;
        }
        
        private IEnumerator CreateBench()
        {
            if (benchPrefab != null)
            {
                GameObject bench = Instantiate(benchPrefab, benchPosition, Quaternion.identity);
                bench.name = "Park Bench";
                spawnedObjects.Add(bench);
            }
            else
            {
                GameObject bench = CreateProceduralBench();
                bench.transform.position = benchPosition;
                spawnedObjects.Add(bench);
            }
            
            yield return null;
        }
        
        private GameObject CreateProceduralBench()
        {
            GameObject bench = new GameObject("Procedural Bench");
            
            Material woodMaterial = new Material(Shader.Find("Standard"));
            woodMaterial.color = new Color(0.6f, 0.4f, 0.2f);
            woodMaterial.SetFloat("_Metallic", 0f);
            woodMaterial.SetFloat("_Glossiness", 0.2f);
            
            // Bench seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(bench.transform);
            seat.transform.localPosition = new Vector3(0, 0.5f, 0);
            seat.transform.localScale = new Vector3(2f, 0.1f, 0.5f);
            seat.GetComponent<MeshRenderer>().material = woodMaterial;
            
            // Bench back
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.transform.SetParent(bench.transform);
            back.transform.localPosition = new Vector3(0, 0.9f, -0.2f);
            back.transform.localScale = new Vector3(2f, 0.8f, 0.1f);
            back.GetComponent<MeshRenderer>().material = woodMaterial;
            
            // Bench legs
            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(bench.transform);
                
                float x = (i % 2 == 0) ? -0.8f : 0.8f;
                float z = (i < 2) ? -0.15f : 0.15f;
                
                leg.transform.localPosition = new Vector3(x, 0.25f, z);
                leg.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
                leg.GetComponent<MeshRenderer>().material = woodMaterial;
            }
            
            return bench;
        }
        
        private IEnumerator CreateFlowers()
        {
            for (int i = 0; i < flowerCount; i++)
            {
                // Generate random position
                Vector2 randomCircle = Random.insideUnitCircle * flowerSpawnRadius;
                Vector3 position = new Vector3(randomCircle.x, 0.1f, randomCircle.y);
                
                GameObject flower;
                
                if (flowerPrefabs != null && flowerPrefabs.Length > 0)
                {
                    GameObject prefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
                    flower = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                }
                else
                {
                    flower = CreateProceduralFlower(position);
                }
                
                flower.name = $"Flower_{i}";
                spawnedObjects.Add(flower);
                
                if (i % 5 == 0) yield return null; // Spread across frames
            }
        }
        
        private GameObject CreateProceduralFlower(Vector3 position)
        {
            GameObject flower = new GameObject("Procedural Flower");
            flower.transform.position = position;
            
            GameObject flowerHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flowerHead.transform.SetParent(flower.transform);
            flowerHead.transform.localPosition = Vector3.up * 0.1f;
            flowerHead.transform.localScale = Vector3.one * 0.15f;
            
            Color[] flowerColors = {
                Color.red, Color.blue, Color.yellow, Color.magenta, 
                new Color(1f, 0.5f, 0f), new Color(0.8f, 0.2f, 0.8f)
            };
            
            Material flowerMaterial = new Material(Shader.Find("Standard"));
            flowerMaterial.color = flowerColors[Random.Range(0, flowerColors.Length)];
            flowerMaterial.SetFloat("_Metallic", 0f);
            flowerMaterial.SetFloat("_Glossiness", 0.3f);
            flowerHead.GetComponent<MeshRenderer>().material = flowerMaterial;
            
            return flower;
        }
        
        private void SetupParticles()
        {
            // Wind particles
            if (windParticles == null)
            {
                GameObject windParticlesObj = new GameObject("Wind Particles");
                windParticles = windParticlesObj.AddComponent<ParticleSystem>();
            }
            
            var windMain = windParticles.main;
            windMain.startLifetime = 8f;
            windMain.startSpeed = 2f;
            windMain.startSize = 0.1f;
            windMain.startColor = new Color(1f, 1f, 1f, 0.3f);
            windMain.maxParticles = maxParticles;
            
            var windShape = windParticles.shape;
            windShape.shapeType = ParticleSystemShapeType.Box;
            windShape.scale = new Vector3(25f, 1f, 25f);
            
            var windVelocity = windParticles.velocityOverLifetime;
            windVelocity.enabled = true;
            windVelocity.space = ParticleSystemSimulationSpace.Local;
            windVelocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            windVelocity.y = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            windVelocity.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            
            // Leaf particles
            if (leafParticles == null)
            {
                GameObject leafParticlesObj = new GameObject("Leaf Particles");
                leafParticles = leafParticlesObj.AddComponent<ParticleSystem>();
            }
            
            var leafMain = leafParticles.main;
            leafMain.startLifetime = 12f;
            leafMain.startSpeed = 1f;
            leafMain.startSize = 0.2f;
            leafMain.startColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
            leafMain.maxParticles = maxParticles / 2;
            
            var leafShape = leafParticles.shape;
            leafShape.shapeType = ParticleSystemShapeType.Circle;
            leafShape.radius = 15f;
            leafShape.position = new Vector3(0, 10f, 0);
            
            var leafGravity = leafParticles.forceOverLifetime;
            leafGravity.enabled = true;
            leafGravity.y = new ParticleSystem.MinMaxCurve(-2f, -0.5f);
        }
        
        private void SetupAudio()
        {
            if (ambientAudioSource == null)
            {
                GameObject audioObj = new GameObject("Ambient Audio");
                ambientAudioSource = audioObj.AddComponent<AudioSource>();
            }
            
            ambientAudioSource.loop = true;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.spatialBlend = 0f; // 2D sound
            
            if (windSound != null)
            {
                ambientAudioSource.clip = windSound;
                ambientAudioSource.Play();
            }
            
            // Start bird sound coroutine
            if (birdSounds != null && birdSounds.Length > 0)
            {
                StartCoroutine(PlayRandomBirdSounds());
            }
        }
        
        private IEnumerator PlayRandomBirdSounds()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(10f, 30f));
                
                if (birdSounds.Length > 0)
                {
                    AudioClip clip = birdSounds[Random.Range(0, birdSounds.Length)];
                    AudioSource.PlayClipAtPoint(clip, Vector3.zero, ambientVolume * 0.7f);
                }
            }
        }
        
        private void SetupSkybox()
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
            }
            else
            {
                // Create procedural skybox
                Material proceduralSky = new Material(Shader.Find("Skybox/Procedural"));
                proceduralSky.SetFloat("_SunSize", 0.04f);
                proceduralSky.SetFloat("_SunSizeConvergence", 5f);
                proceduralSky.SetFloat("_AtmosphereThickness", 1f);
                proceduralSky.SetColor("_SkyTint", new Color(0.5f, 0.5f, 0.5f, 1f));
                proceduralSky.SetColor("_GroundColor", new Color(0.369f, 0.349f, 0.341f, 1f));
                
                RenderSettings.skybox = proceduralSky;
            }
            
            DynamicGI.UpdateEnvironment();
        }
        
        private void SetupFog()
        {
            RenderSettings.fog = enableFog;
            if (enableFog)
            {
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = fogDensity;
            }
        }
        
        private void Update()
        {
            if (isDayNightCycleEnabled)
            {
                UpdateDayNightCycle();
            }
        }
        
        private void UpdateDayNightCycle()
        {
            currentTimeOfDay += Time.deltaTime / dayDuration;
            if (currentTimeOfDay >= 1f) currentTimeOfDay = 0f;
            
            // Update sun light
            if (sunLight != null)
            {
                float intensity = sunIntensityCurve.Evaluate(currentTimeOfDay);
                Color color = sunColorGradient.Evaluate(currentTimeOfDay);
                
                sunLight.intensity = intensity;
                sunLight.color = color;
                
                // Rotate sun
                float angle = currentTimeOfDay * 360f - 90f; // Start at horizon
                sunLight.transform.rotation = Quaternion.Euler(angle, 30f, 0f);
            }
            
            // Update ambient lighting
            RenderSettings.ambientLight = Color.Lerp(
                new Color(0.2f, 0.2f, 0.3f), // Night
                new Color(0.4f, 0.4f, 0.45f), // Day
                sunIntensityCurve.Evaluate(currentTimeOfDay)
            );
        }
        
        public void SetTimeOfDay(float normalizedTime)
        {
            currentTimeOfDay = Mathf.Clamp01(normalizedTime);
        }
        
        public void EnableDayNightCycle(bool enable)
        {
            isDayNightCycleEnabled = enable;
        }
        
        public void ClearEnvironment()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            spawnedObjects.Clear();
        }
        
        private void OnDestroy()
        {
            ClearEnvironment();
        }
    }
}