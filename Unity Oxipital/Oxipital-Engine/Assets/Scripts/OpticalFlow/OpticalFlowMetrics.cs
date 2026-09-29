using UnityEngine;
using UnityEngine.Rendering;

namespace Oxipital
{
    // Global motion metrics, read from the 1x1 mip of the processed flow (non blocking readback).
    // Output fields are public so they show up in OSCQuery and can drive other parameters from Chataigne.
    public class OpticalFlowMetrics : MonoBehaviour
    {
        [Header("Settings")]
        [Range(0, .99f)]
        public float smoothing = .8f;
        [Range(0, 50)]
        public float motionGain = 10;
        // Sudden gesture : motion crossing this value upwards
        [Range(0, 1)]
        public float gestureThreshold = .5f;
        [Range(0, 2)]
        public float gestureCooldown = .5f;

        [Header("Output")]
        [Range(0, 1)]
        public float motionAmount;
        public Vector2 averageDirection;
        public Vector3 averageDirectionWorld;
        public bool gesture;
        public int gestureCount;

        bool pending;
        float rawMotion;
        Vector2 rawDirection;
        float lastInstantMotion;
        float cooldownTimer;
        float gestureHoldTimer;

        internal void Execute(RenderTexture flow, FlowPlane plane)
        {
            if (!pending && flow != null)
            {
                pending = true;
                AsyncGPUReadback.Request(flow, flow.mipmapCount - 1, TextureFormat.RGBAFloat, OnReadback);
            }

            float dt = Time.unscaledDeltaTime;
            float k = 1 - Mathf.Pow(smoothing, dt * 60);

            float instantMotion = Mathf.Clamp01(rawMotion * motionGain);
            Vector2 instantDirection = Vector2.ClampMagnitude(rawDirection * motionGain, 1);

            motionAmount = Mathf.Lerp(motionAmount, instantMotion, k);
            averageDirection = Vector2.Lerp(averageDirection, instantDirection, k);
            averageDirectionWorld = plane != null ? plane.Right * averageDirection.x + plane.Up * averageDirection.y : Vector3.zero;

            cooldownTimer -= dt;
            gestureHoldTimer -= dt;
            if (cooldownTimer <= 0 && instantMotion > gestureThreshold && lastInstantMotion <= gestureThreshold)
            {
                gestureCount++;
                cooldownTimer = gestureCooldown;
                gestureHoldTimer = .2f; // long enough to be seen over OSC
            }
            gesture = gestureHoldTimer > 0;
            lastInstantMotion = instantMotion;
        }

        void OnReadback(AsyncGPUReadbackRequest request)
        {
            if (this == null) return;
            pending = false;
            if (request.hasError) return;

            Vector4 average = request.GetData<Vector4>()[0];
            rawDirection = new Vector2(average.x, average.y);
            rawMotion = average.z;
        }

        void OnDisable()
        {
            pending = false;
        }
    }
}
