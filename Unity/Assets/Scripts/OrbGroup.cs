using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;
using UnityEngine.VFX.SDF;
using System.IO;
using Augmenta;
using UnityEngine.Rendering;

namespace Oxipital
{
    public class MeshLoader
    {
        public static bool isLoaded = false;

        public struct MeshTextured
        {
            public Texture2D texture;
            public Mesh mesh;
        }
            

        static Dictionary<string, MeshTextured> shapeMeshMap;
        public MeshLoader()
        {
        }

        async public static void loadMeshes()
        {
            if (shapeMeshMap != null) return;
            shapeMeshMap = new Dictionary<string, MeshTextured>();

            string emittersPath = Path.Combine(Application.streamingAssetsPath, "emitters");
            string[] files = Directory.GetFiles(emittersPath, "*.glb");

            foreach (string file in files)
            {
                var gltfI = new GLTFast.GltfImport();
                var success = await gltfI.Load(file);

                if (success)
                {
                    var meshes = gltfI.GetMeshes();
                    if (meshes.Length > 0)
                    {
                        Mesh m = meshes[0];
                        if (m == null)
                        {
                            Debug.LogWarning(file + " loaded but no mesh found inside");
                            return;
                        }

                        var text = gltfI.GetTexture(0);

                        MeshTextured mt = new MeshTextured();
                        mt.mesh = m;
                        mt.texture = text;

                        var fileWithoutExtension = Path.GetFileNameWithoutExtension(file);
                        shapeMeshMap.Add(fileWithoutExtension.ToLower(), mt);
                        Debug.Log("Loaded mesh " + fileWithoutExtension);
                    }
                    else
                    {
                        Debug.LogWarning("No mesh found in glTF file");
                    }
                }
                else
                {
                    Debug.LogError("Loading glTF failed!");
                }
            }

            Debug.Log("Loaded all meshes");
            isLoaded = true;
        }

        public static MeshTextured getMesh(string name)
        {
            if(name == "")
            {
                return shapeMeshMap["sphere"];
            }
            else 
            {
                return shapeMeshMap[name];
            }
        }

    }

    [RequireComponent(typeof(VisualEffect))]
    public class OrbGroup : DancerGroup<Dancer>
    {
        [Header("Emission")]
        [InBuffer(0)]
        [Range(0, 60)]
        public float life = 20;

        public enum EmitterShape { Sphere, Plane, Torus, Cube, Pipe, Egg, Line, Circle, Merkaba, Pyramid, Custom, Augmenta }
        public enum RenderType { UnlitOpaque, UnlitAdditive, LitQuad, LitMesh }

        [InBuffer(1)]
        public EmitterShape emitterShape;
        EmitterShape lastEmitterShape;

        [InBuffer(2)]
        [Range(0, 1)]
        public float emitterSurfaceFactor = 0;

        [InBuffer(3)]
        [Range(0, 1)]
        public float emitterVolumeFactor = 0;

        [InBuffer(4)]
        [Range(0, 1)]
        public float emitterPositionNoise = 0;

        [InBuffer(5)]
        [Range(0, 5)]
        public float emitterPositionNoiseFrequency = 1;

        [InBuffer(6)]
        [Range(0, 1)]
        public float emitterPositionNoiseRadius = 1;

        [Header("Appearance")]
        [InBuffer(7)]
        [ColorUsage(true, true)]
        public Color color = Color.white;

        public Color colorLife = Color.black;
        [Range(0, 1)]
        public float colorLifeRange = 1;
        public Gradient colorOverLife;
		[InBuffer(10)]
		[Range(0, 1)]
		public float colorLifeBlend = 0;

		public Color colorSpeed = Color.black;
		[Range(0, 1)]
		public float colorSpeedRange = 1;
		public Gradient colorOverSpeed;
		[InBuffer(11)]
		[Range(0, 1)]
		public float colorSpeedBlend = 0;

		[InBuffer(12)]
		[Range(0, 1)]
		public float colorMaxSpeed = 1;

		[InBuffer(13)]
        [Range(0, 1)]
        public float alpha = .5f;

        [InBuffer(14)]
        [Range(0, 1)]
        public float hdrMultiplier = 1;

        [InBuffer(15)]
        [Range(0, 1)]
        public float alphaSpeedThreshold = 0;

        [InBuffer(16)]
        [Range(0, 1)]
        public float textureOpacity = 0;

        [InBuffer(17)]
        [Range(0, 1)]
        public float particleSize = 0;

        [InBuffer(18)]
        public RenderType renderType = RenderType.UnlitAdditive;

        [InBuffer(19)]
        [Range(0, 1)]
        public float meshOpacity = 0;
        [Range(0, 1)]
        public float meshColorIntensity = 0;
        [Range(0, 1)]
        public float lightIntensity = 0;
        public string textureFolderPath = "";
        public string textureName = "";
        public bool useSpoutTexture = false;
        public RenderTexture spoutTexture;

        public Material transparentMaterial;
        public Material opaqueMaterial;


		[Header("Physics")]
        [InBuffer(20)]
        [Range(0, 1)]
        public float forceWeight = 1;

        [InBuffer(21)]
        [Range(0, 1)]
        public float drag = .5f;

        [InBuffer(22)]
        [Range(0, 1)]
        public float velocityDrag = 0;

        [InBuffer(23)]
        [Range(0, 1)]
        public float noisyDrag = 0;

        [InBuffer(24)]
        [Range(0, 5)]
        public float noisyDragFrequency = 0;

        [InBuffer(25)]
        public bool activateCollision = false;



        public string customMeshName = string.Empty;

        [OSCQuery.DoNotExpose]
        public string meshName = "";
        string lastMeshName = "";

        Mesh currentMesh;
        internal VisualEffect vfx;

        PCLToGraphicsBuffer pclGraphics;
        public AugmentaObject augmentaObject;

        private bool textureLoaded = false;
        private string lastTexturePath = "";
        private Texture2D spoutTexture2D;

        protected override void OnEnable()
        {
            base.OnEnable();
            MeshLoader.loadMeshes();
            vfx = GetComponent<VisualEffect>();
            pclGraphics = GetComponent<PCLToGraphicsBuffer>(); 
            textureLoaded = false;
			spoutTexture2D = new Texture2D(spoutTexture.width, spoutTexture.height, TextureFormat.RGBAFloat, false);
		}

        protected override void Update()
        {
            base.Update();

            colorOverLife = CreateGradient(color, colorLife, 0, 1, 0, colorLifeRange);
            colorOverSpeed = CreateGradient(Color.black, colorSpeed, 1, 1, (1-colorSpeedRange), 1);

			bool isDying = killProgress > 0;
            vfx.SetFloat("Emitter Intensity", isDying ? 0 : dancerIntensity);
            vfx.SetGraphicsBuffer("Orb Buffer", buffer);
            vfx.SetGradient("Color Over Life", colorOverLife);
            vfx.SetGradient("Color Over Speed", colorOverSpeed);
            vfx.SetVector3("Source Position", transform.position);

			foreach (var item in items) item.debugColor = color;

            if (emitterShape != lastEmitterShape)
            {
                if (emitterShape != EmitterShape.Line && emitterShape != EmitterShape.Circle && emitterShape != EmitterShape.Custom && emitterShape != EmitterShape.Augmenta)
                {
                    string shape = emitterShape.ToString().ToLower();
                    meshName = shape;
                }

                lastEmitterShape = emitterShape;
            }

            // Load Augmenta Point Clouds
            if (emitterShape == EmitterShape.Augmenta)
            {
                if (augmentaObject != null)
                {
                    pclGraphics.pObject = augmentaObject;
                }
            }

            // Load custom mesh
            if (emitterShape == EmitterShape.Custom)
            {
                meshName = customMeshName;
            }

            if (MeshLoader.isLoaded)
            {
                // Update mesh in case we change it
                if (meshName != lastMeshName)
                {
                    currentMesh = MeshLoader.getMesh(meshName.ToLower()).mesh;
                    Texture2D texture = MeshLoader.getMesh(meshName.ToLower()).texture;

                    if (currentMesh == null) Debug.LogWarning("Could not find mesh for " + meshName);
                    else setMesh(currentMesh);

                    if (texture == null)
                    {
                        Debug.LogWarning("Could not find texture for " + meshName);
                        setColor(Texture2D.whiteTexture, "Emitter Mesh Color");
                    }
                    else
                    {
                        setColor(texture, "Emitter Mesh Color");
                    }

                    lastMeshName = meshName;
                }
            }

			// Load 2D Texture
			string fullPath = Path.Combine(textureFolderPath, textureName);
            if (fullPath != lastTexturePath) textureLoaded = false;

			if (textureFolderPath != "" && textureName != "" && textureLoaded == false)
            {
                Texture2D tex = LoadTextureFromPath(fullPath);
                if (tex != null)
                {
                    setColor(tex, "Emitter Color");
                    textureLoaded = true;
                    lastTexturePath = fullPath;

				}
			}

            if(useSpoutTexture)
            {
				Graphics.ConvertTexture(spoutTexture, spoutTexture2D);
				//Texture2D newTexture = toTexture2D(spoutTexture);
				setColor(spoutTexture2D, "Emitter Color");
			}
            else
            {
                setColor(Texture2D.whiteTexture, "Emitter Color");
            }


            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in renderers)
            {
                if (meshOpacity == 1)
                {
                    if (r.material.renderQueue > (int)RenderQueue.GeometryLast)
                    {
                        r.material = opaqueMaterial;
                    }
                }
                else
                {
                    if (r.material.renderQueue < (int)RenderQueue.GeometryLast)
                    {
                        r.material = transparentMaterial;
                    }
                }

                Color c = r.material.color;
                c = Color.Lerp(Color.black, color, meshColorIntensity);
                c.a = meshOpacity;
                r.material.color = c;

                if (useSpoutTexture)
                {
                    r.material.mainTexture = spoutTexture;
				}
                else
                {
                    r.material.mainTexture = null;
				}

            }

            Light[] lights = GetComponentsInChildren<Light>();
            foreach (Light l in lights)
            {
                l.intensity = Unity.Mathematics.math.remap(0, 1, 0, 1000, lightIntensity);
                l.shadowStrength = 1;
                l.color = color;
            }

        }

        internal void setForceBuffers(Dictionary<string, GraphicsBuffer> forceBuffers)
        {
            if (vfx == null) return;

            foreach (var b in forceBuffers)
            {
                if (!vfx.HasGraphicsBuffer(b.Key))
                {
                    Debug.LogWarning(b.Key + " not found in VFX");
                    continue;
                }

                vfx.SetGraphicsBuffer(b.Key, b.Value);

                if (!vfx.HasGraphicsBuffer(b.Key + "B"))
                {
                    Debug.LogWarning("B Buffer " + b.Key + " not found in VFX");
                    continue;
                }

                vfx.SetGraphicsBuffer(b.Key + "B", b.Value);
            }

            //Update intensity here as we need to pass it outside the GraphicsBuffer
            //Discussion : https://discussions.unity.com/t/spawn-a-variable-amount-of-particles-from-graphics-buffer/899049/2
        }

        override protected Dancer addItem()
        {
            Dancer d = base.addItem();
            d.GetComponent<MeshFilter>().sharedMesh = currentMesh;
            return d;
        }

        internal void setMesh(Mesh m)
        {
            if (vfx == null) return;
            if (m == null) return;

            if (!vfx.HasMesh("Emitter Mesh"))
            {
                Debug.LogWarning("Emitter Mesh not found in VFX");
                return;
            }

            vfx.SetMesh("Emitter Mesh", m);
            foreach (var item in items) item.GetComponent<MeshFilter>().sharedMesh = m;
        }

        internal void setColor(Texture2D texture, string name)
        {
            if (vfx == null) return;
            if (texture == null) return;

            if (!vfx.HasTexture(name))
            {
                Debug.LogWarning(name + " not found in VFX");
                return;
            }
            vfx.SetTexture(name, texture);
        }


        public override void kill(float time)
        {
            base.kill(time);
            if (vfx == null) return;
            vfx.SetFloat("Emitter Intensity", 0);
            count = 0;
        }

        override protected Type getGroupType()
        {
            return GetType();
        }

        protected override float getKillTime()
        {
            return life;
        }

        public void killAllParticle()
        {
            vfx.Reinit();
        }

        public new void ResetPattern()
        {
            base.ResetPattern();
        }

        public Gradient CreateGradient(Color colorA, Color colorB, float alphaKeyA, float alphaKeyB, float timeA, float timeB)
        {
            if (timeA == 0)
                timeA = 0.001f;

			Gradient result = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0].color = colorA * (float)Math.Pow(2,hdrMultiplier*5);
            colorKeys[0].time = timeA;
            colorKeys[1].color = colorB * (float)Math.Pow(2, hdrMultiplier * 5);
            colorKeys[1].time = timeB;
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = alphaKeyA;
            alphaKeys[0].time = timeA;
            alphaKeys[1].alpha = alphaKeyB;
            alphaKeys[1].time = timeB;
            result.SetKeys(colorKeys, alphaKeys);

            return result;
        }

        public Texture2D LoadTextureFromPath(string path)
        {
            if (File.Exists(path))
            {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(fileData))
                {
                    return tex;
                }
            }
            return null;
		}
	}

}


