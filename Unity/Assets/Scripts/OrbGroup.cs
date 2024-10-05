using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;
using UnityEngine.VFX.SDF;
using System.IO;


namespace Oxipital
{
    public class MeshLoader
    {
        static MeshLoader instance;

        static Dictionary<string, Mesh> shapeMeshMap;
        public MeshLoader()
        {
            instance = this;
        }

        public static MeshLoader getInstance()
        {
            if(instance == null) instance = new MeshLoader();
            return instance;
        }

        async public static void loadMeshes()
        {
            if(shapeMeshMap != null) return;
            shapeMeshMap = new Dictionary<string, Mesh>();

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

                        var fileWithoutExtension = Path.GetFileNameWithoutExtension(file);
                        shapeMeshMap.Add(fileWithoutExtension, m);
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
        }

        public static Mesh getMesh(string name)
        {
            return shapeMeshMap[name];
        }
    }

    public class OrbGroup : DancerGroup<Orb>
    {
        [Header("Emission")]
        [InBuffer(0)]
        [Range(0, 60)]
        public float life = 20;

        public enum EmitterShape { Sphere, Plane, Torus, Cube, Pipe, Egg, Line, Circle, Merkaba, Pyramid, Custom }

        [InBuffer(1)]
        public EmitterShape emitterShape;
        EmitterShape lastEmitterShape;

        [InBuffer(2)]
        [Range(0, 1)]
        public float emitterSurfaceFactor = 0;

        [InBuffer(3)]
        [Range(0, 1)]
        public float emitterVolumeFactor = 0;

        [Header("Appearance")]
        [InBuffer(4)]
        public Color color = Color.white;

        [InBuffer(7)]
        [Range(0, 1)]
        public float alpha = .5f;

        [InBuffer(8)]
        [Range(0, 1)]
        public float hdrMultiplier = 1;

        [InBuffer(9)]
        [Range(0, 1)]
        public float alphaSpeedThreshold = 0;

        [InBuffer(10)]
        [Range(0, 1)]
        public float textureOpacity = 0;

        [InBuffer(11)]
        [Range(0, 1)]
        public float particleSize = 0;

        [Header("Physics")]
        [InBuffer(12)]
        [Range(0, 1)]
        public float forceWeight = 1;

        [InBuffer(13)]
        [Range(0, 1)]
        public float drag = .5f;

        [InBuffer(14)]
        [Range(0, 1)]
        public float velocityDrag = 0;

        [InBuffer(15)]
        [Range(0, 1)]
        public float noisyDrag = 0;

        [InBuffer(16)]
        [Range(0, 5)]
        public float noisyDragFrequency = 0;

        [InBuffer(17)]
        public bool activateCollision = false;

        [Header("Debug")]
        public bool showMesh = false;

        public string meshName = "";
        string lastMeshName = "";

        Mesh currentMesh;

        protected override void OnEnable()
        {
            base.OnEnable();
            MeshLoader.loadMeshes();
        }

        protected override void Update()
        {
            base.Update();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].setOrbBuffer(buffer, i);
            }

            if(emitterShape != lastEmitterShape)
            {
                if(emitterShape != EmitterShape.Line && emitterShape != EmitterShape.Circle && emitterShape != EmitterShape.Custom)
                {
                    string shape = emitterShape.ToString().ToLower();
                    meshName = shape;
                }
                
                lastEmitterShape = emitterShape;
            }

            if (meshName != lastMeshName)
            {
                currentMesh = MeshLoader.getMesh(meshName.ToLower());
                if (currentMesh == null) Debug.LogWarning("Could not find mesh for " + meshName);
                else foreach(Orb orb in items) orb.setMesh(currentMesh);
                lastMeshName = meshName;
            }

        }

        protected override void addItem()
        {
            base.addItem();
            items[items.Count - 1].setMesh(currentMesh);
        }

        public void setForceBuffers(Dictionary<string, GraphicsBuffer> forceBuffers)
        {
            foreach (Orb orb in items)
            {
                orb.setForceBuffers(forceBuffers);
            }
        }
        override protected Type getGroupType()
        {
            return GetType();
        }


        override protected void killLastItem()
        {
            (items[items.Count - 1] as Orb).kill(getKillTime());
        }
        public override void kill(float time = 0)
        {
            base.kill(time);
            count = 0;
        }

        protected override float getKillTime()
        {
            return life;
        }
    }

}


