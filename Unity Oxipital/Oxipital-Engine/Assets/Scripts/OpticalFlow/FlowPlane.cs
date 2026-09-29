using UnityEngine;

namespace Oxipital
{
    // Where the 2D optical flow lives in the 3D scene : a unit quad in the local XY plane.
    // Place it where the performer (or the screen) is, scale X / Y to the area seen by the Kinect,
    // keep scale Z = 1 so the depth band is in meters.
    public class FlowPlane : MonoBehaviour
    {
        public Matrix4x4 WorldToPlane => transform.worldToLocalMatrix;
        public Vector3 Right => transform.right;
        public Vector3 Up => transform.up;

        OpticalFlowManager manager;

        void OnDrawGizmos()
        {
            if (manager == null) manager = FindFirstObjectByType<OpticalFlowManager>();

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 1, 1, .8f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1, 1, 0));

            if (manager == null) return;

            Gizmos.color = new Color(0, 1, 1, .2f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1, 1, manager.depthBand * 2));

            OpticalFlowMetrics metrics = manager.GetComponent<OpticalFlowMetrics>();
            if (metrics != null && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = new Vector3(metrics.averageDirection.x, metrics.averageDirection.y, 0) * .5f;
                Gizmos.DrawLine(Vector3.zero, direction);
                Gizmos.DrawWireSphere(direction, .02f);
            }
        }
    }
}
