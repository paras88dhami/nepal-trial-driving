using UnityEngine;

namespace NepalTrial.CameraSystem
{
    /// <summary>Basic smooth chase camera until Cinemachine integration is tested.</summary>
    public sealed class VehicleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -8f);
        [SerializeField, Min(0.1f)] private float followSpeed = 7f;
        [SerializeField, Min(0.1f)] private float turnSpeed = 8f;

        public void SetTarget(Transform value) => target = value;

        private void LateUpdate()
        {
            if (target == null) return;
            var destination = target.TransformPoint(offset);
            float followFactor = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, destination, followFactor);
            var targetRotation = Quaternion.LookRotation(target.position + Vector3.up - transform.position);
            float turnFactor = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnFactor);
        }
    }
}
