using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.VFX;

namespace Oxipital
{
    [RequireComponent(typeof(VisualEffect))]
    public class Orb : Dancer
    {

        VisualEffect vfx;

        public string meshName = "";
        string lastMeshName = "";

        protected void OnEnable()
        {
            if (vfx == null) vfx = GetComponent<VisualEffect>();
        }

        override protected void Update()
        {
            base.Update();

            if (meshName != lastMeshName)
            {
                StartCoroutine("loadMesh");
                lastMeshName = meshName;
            }

            //Update intensity here as we need to pass it outside the GraphicsBuffer
            //Discussion : https://discussions.unity.com/t/spawn-a-variable-amount-of-particles-from-graphics-buffer/899049/2
            vfx.SetFloat("Emitter Intensity", intensity);
        }

        IEnumerator loadMesh()
        {
            setMesh();
            yield return null;
        }
       
        async void setMesh()
        {
            if (vfx == null) return;

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

                    if (!vfx.HasMesh("Emitter Mesh"))
                    {
                        Debug.LogWarning(meshName + " not found in VFX");
                        return;
                    }

                    vfx.SetMesh("Emitter Mesh", m);
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
            }
        }

        internal void setOrbBuffer(GraphicsBuffer buffer, int index)
        {
            if (vfx == null) return;

            if (!vfx.HasGraphicsBuffer("Orb Buffer"))
            {
                Debug.LogWarning("Orb Buffer not found in VFX");
                return;
            }

            vfx.SetInt("Orb Index", index);
            vfx.SetGraphicsBuffer("Orb Buffer", buffer);
        }
    }
}