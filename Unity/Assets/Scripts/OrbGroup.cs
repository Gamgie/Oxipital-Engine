using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;
using UnityEngine.VFX.SDF;


namespace Oxipital
{
    public class OrbGroup : DancerGroup<Orb>
    {
        [Header("Emission")]
        [InBuffer(0)]
        [Range(0, 40)]
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
        [Range(0, 1)]
        public float noisyDragFrequency = 0;

        [InBuffer(17)]
        public bool activateCollision = false;

        [Header("Debug")]
        public bool showMesh = false;

        public string meshName = "";
        string lastMeshName = "";
        Mesh emitterMesh;

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
                StartCoroutine("loadMesh");
                lastMeshName = meshName;
            }

        }

        protected override void addItem()
        {
            base.addItem();
            items[items.Count - 1].setMesh(emitterMesh);
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


        //Mesh loading
        IEnumerator loadMesh()
        {
            setMesh();
            yield return null;
        }

        async void setMesh()
        {
            if (meshName == "") return;
            //var gltf = new GLTFast.GltfAsset();
            //gltf.StreamingAsset = true;
            //var success = await gltf.Load("emitters/"+meshName+".gltf");

            var gltfI = new GLTFast.GltfImport();
            var success = await gltfI.Load(Application.streamingAssetsPath + "/emitters/" + meshName + ".gltf");

            if (success)
            {
                var meshes = gltfI.GetMeshes();
                Debug.Log("GLTFImport Loaded " + meshes.Length + " meshes");

                if (meshes.Length > 0)
                {
                    Mesh m = meshes[0];
                    if (m == null)
                    {
                        Debug.LogWarning(meshName + " not found in StreamingAssets");
                        return;
                    }

                    emitterMesh = m;
                    foreach (Orb orb in items) orb.setMesh(m);
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


