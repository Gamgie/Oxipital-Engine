using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;
using UnityEngine.VFX.SDF;
using System.IO;
using Augmenta;

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
            return shapeMeshMap[name];
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
        public enum RenderType { UnlitOpaque, UnlitAdditive, LitQuad, LitMesh}

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

        [InBuffer(10)]
        [Range(0, 1)]
        public float alpha = .5f;

        [InBuffer(11)]
        [Range(0, 1)]
        public float hdrMultiplier = 1;

        [InBuffer(12)]
        [Range(0, 1)]
        public float alphaSpeedThreshold = 0;

        [InBuffer(13)]
        [Range(0, 1)]
        public float textureOpacity = 0;

        [InBuffer(14)]
        [Range(0, 1)]
        public float particleSize = 0;

        [InBuffer(15)]
        public RenderType renderType = RenderType.UnlitAdditive;

        [InBuffer(16)]
        [Range(0, 1)]
        public float meshOpacity = 0;
        [Range(0, 1)]
        public float meshColorIntensity = 0;
        [Range(0, 1)]
        public float lightIntensity = 0;


        [Header("Physics")]
        [InBuffer(17)]
        [Range(0, 1)]
        public float forceWeight = 1;

        [InBuffer(18)]
        [Range(0, 1)]
        public float drag = .5f;

        [InBuffer(19)]
        [Range(0, 1)]
        public float velocityDrag = 0;

        [InBuffer(20)]
        [Range(0, 1)]
        public float noisyDrag = 0;

        [InBuffer(21)]
        [Range(0, 5)]
        public float noisyDragFrequency = 0;

        [InBuffer(22)]
        public bool activateCollision = false;

        

        public string customMeshName = string.Empty;

        [OSCQuery.DoNotExpose]
        public string meshName = "";
        string lastMeshName = "";

        Mesh currentMesh;
        internal VisualEffect vfx;

        PCLToGraphicsBuffer pclGraphics;
        public AugmentaObject augmentaObject;

        protected override void OnEnable()
        {
            base.OnEnable();
            MeshLoader.loadMeshes();
            vfx = GetComponent<VisualEffect>();
            pclGraphics = GetComponent<PCLToGraphicsBuffer>();
        }

        protected override void Update()
        {
            base.Update();

            bool isDying = killProgress > 0;
            vfx.SetFloat("Emitter Intensity", isDying ? 0 : dancerIntensity);
            vfx.SetGraphicsBuffer("Orb Buffer", buffer);

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
            if(emitterShape == EmitterShape.Augmenta)
            {
                if (augmentaObject != null)
                {
                    pclGraphics.pObject = augmentaObject;
                }
            }

            // Load custom mesh
            if(emitterShape == EmitterShape.Custom)
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
                        setColor(Texture2D.whiteTexture);   
                    }
                    else
                    {
                        setColor(texture);
                    }
                    
                    lastMeshName = meshName;
                }
            }

            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer r in renderers)
			{
                Color c = r.material.color;
                c = Color.Lerp(Color.black, color, meshColorIntensity);
                c.a = meshOpacity;
                r.material.color = c;
            }

            Light[] lights = GetComponentsInChildren<Light>();
            foreach(Light l in lights)
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

        internal void setColor(Texture2D texture)
        {
            if (vfx == null) return;
            if (texture == null) return;

            if(!vfx.HasTexture("Emitter Color"))
            {
                Debug.LogWarning("Emitter Color not found in VFX");
                return;
            }
            vfx.SetTexture("Emitter Color", texture);
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
	}

}


