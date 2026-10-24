using UnityEngine;

namespace NepalTrial.Vehicles.Car
{
    /// <summary>
    /// Bind imported wheel meshes in Inspector. Copies WheelCollider steering,
    /// suspension and rolling pose without changing wheel physics.
    /// </summary>
    public sealed class CarWheelVisualSync : MonoBehaviour
    {
        [SerializeField] private WheelCollider frontLeftCollider;
        [SerializeField] private WheelCollider frontRightCollider;
        [SerializeField] private WheelCollider rearLeftCollider;
        [SerializeField] private WheelCollider rearRightCollider;
        [SerializeField] private Transform frontLeftMesh;
        [SerializeField] private Transform frontRightMesh;
        [SerializeField] private Transform rearLeftMesh;
        [SerializeField] private Transform rearRightMesh;

        private static void Sync(WheelCollider wheel,Transform visual)
        {
            if(wheel==null || visual==null)return;
            wheel.GetWorldPose(out Vector3 position,out Quaternion rotation);
            visual.SetPositionAndRotation(position,rotation);
        }

        private void LateUpdate()
        {
            Sync(frontLeftCollider,frontLeftMesh);
            Sync(frontRightCollider,frontRightMesh);
            Sync(rearLeftCollider,rearLeftMesh);
            Sync(rearRightCollider,rearRightMesh);
        }
    }
}
