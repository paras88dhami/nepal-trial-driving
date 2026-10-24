using NepalTrial.Input;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Trial
{
    /// <summary>Pause/resume freezes the Rigidbody while keeping scores and timers synchronized.</summary>
    public sealed class TwoWheelerSessionActions : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private TwoWheelerInput input;
        private Rigidbody body;
        private bool frozen;
        private Vector3 savedVelocity, savedAngularVelocity;

        private void Awake() { if (vehicle != null) body = vehicle.GetComponent<Rigidbody>(); }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
                if (Keyboard.current.f5Key.wasPressedThisFrame) Restart();
            }
#endif
        }

        public void TogglePause() { if (trial != null) SetPaused(!trial.IsPaused); }
        public void SetPaused(bool value)
        {
            if (trial == null || !trial.SetPaused(value) || body == null || frozen == value) return;
            if (value)
            {
                savedVelocity = body.linearVelocity;
                savedAngularVelocity = body.angularVelocity;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                frozen = true;
                if (vehicle != null) vehicle.SetDriveEnabled(false);
            }
            else
            {
                body.isKinematic = false;
                body.linearVelocity = savedVelocity;
                body.angularVelocity = savedAngularVelocity;
                frozen = false;
                if (vehicle != null) vehicle.SetDriveEnabled(true);
            }
        }

        public void Restart()
        {
            if (trial == null) return;
            if (frozen && body != null) body.isKinematic = false;
            frozen = false;
            if (input != null) input.ResetVehicle();
            else if (vehicle != null) vehicle.ResetVehicle();
            trial.RestartAttempt();
        }

        private void OnDisable()
        {
            if (frozen && body != null) body.isKinematic = false;
            frozen = false;
            if (vehicle != null) vehicle.SetDriveEnabled(true);
        }
    }
}
