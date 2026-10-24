using NepalTrial.Input;
using NepalTrial.Vehicles.Car;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Trial
{
    /// <summary>Connects scene buttons and keyboard restart/pause to car physics and trial lifecycle.</summary>
    public sealed class TrialCarSessionActions : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        [SerializeField] private PrototypeCarInput input;
        private Rigidbody body;
        private Vector3 savedVelocity, savedAngularVelocity;
        private bool frozen;

        private void Awake() { if (car != null) body = car.GetComponent<Rigidbody>(); }

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

        public void TogglePause()
        {
            if (trial == null) return;
            SetPaused(!trial.IsPaused);
        }

        public void SetPaused(bool paused)
        {
            if (trial == null || !trial.SetPaused(paused)) return;
            if (body == null || frozen == paused) return;
            if (paused)
            {
                savedVelocity = body.linearVelocity;
                savedAngularVelocity = body.angularVelocity;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                frozen = true;
            }
            else
            {
                body.isKinematic = false;
                body.linearVelocity = savedVelocity;
                body.angularVelocity = savedAngularVelocity;
                frozen = false;
            }
        }

        public void Restart()
        {
            if (trial == null) return;
            if (frozen && body != null) { body.isKinematic = false; frozen = false; }
            if (input != null) input.ResetCar();
            else if (car != null) car.ResetVehicle();
            trial.RestartAttempt();
        }

        private void OnDisable()
        {
            if (frozen && body != null) body.isKinematic = false;
            frozen = false;
        }
    }
}
