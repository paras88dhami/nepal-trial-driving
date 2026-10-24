using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Two-circle practice course: validates wheel tracks and ordered one-direction checkpoints.
    /// The geometric dimensions are demonstration values, not official Nepal trial specifications.
    /// </summary>
    public sealed class CarFigureEightSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        [SerializeField] private WheelCollider[] wheels;
        [SerializeField] private Transform[] orderedGates;
        [SerializeField] private string stageId = "figure-eight";
        [SerializeField] private Vector2 leftCenter = new Vector2(-7f, 0f);
        [SerializeField] private Vector2 rightCenter = new Vector2(7f, 0f);
        [SerializeField, Min(1f)] private float ringRadius = 7f;
        [SerializeField, Min(.5f)] private float trackHalfWidth = 2.8f;
        [SerializeField, Min(.2f)] private float gateHalfWidth = 2.5f;
        private OrderedDrivingGateTracker gates;
        private BoundaryIncidentCounter incidents = new BoundaryIncidentCounter();
        private bool previousKnown;
        private bool finished;
        private Vector3 lastPosition;

        public int CheckpointsReached => gates == null ? 0 : gates.NextIndex;
        public int CheckpointCount => gates == null ? 0 : gates.Total;
        public int BoundaryIncidents => incidents.IncidentCount;

        private void Awake()
        {
            if (orderedGates == null || orderedGates.Length == 0)
            {
                Debug.LogError("Figure-eight needs ordered directional checkpoint transforms.", this);
                enabled = false;
                return;
            }
            var positions = new Vector3[orderedGates.Length];
            var directions = new Vector3[orderedGates.Length];
            for (int i = 0; i < orderedGates.Length; i++)
            {
                if (orderedGates[i] == null) { enabled = false; Debug.LogError("Null figure-eight gate.", this); return; }
                positions[i] = orderedGates[i].position;
                directions[i] = orderedGates[i].forward;
            }
            gates = new OrderedDrivingGateTracker(positions, directions, gateHalfWidth);
        }

        private void OnEnable()
        {
            if (trial != null) trial.AttemptRestarted += ResetAttempt;
        }

        private void OnDisable()
        {
            if (trial != null) trial.AttemptRestarted -= ResetAttempt;
        }

        public void ResetAttempt()
        {
            if (gates != null) gates.Reset();
            incidents.Reset();
            previousKnown = false;
            finished = false;
        }

        private void FixedUpdate()
        {
            if (trial == null || car == null || gates == null || finished) return;
            // Do not accrue penalties outside this course or when the exam is paused.
            if (trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId)
            {
                previousKnown = false;
                return;
            }
            Vector3 position = car.transform.position;
            if (previousKnown && gates.Sample(lastPosition, position) && gates.Finished)
            {
                finished = trial.ReportStage(stageId);
                if (finished) return;
            }
            lastPosition = position;
            previousKnown = true;

            if (wheels == null || wheels.Length == 0 || gates.NextIndex == 0) return;
            bool anyOutside = false;
            for (int i = 0; i < wheels.Length; i++)
            {
                WheelCollider wheel = wheels[i];
                if (wheel == null) continue;
                Vector3 contact;
                WheelHit hit;
                if (wheel.GetGroundHit(out hit)) contact = hit.point;
                else contact = wheel.transform.position - wheel.transform.up * wheel.radius;
                Vector2 p = new Vector2(contact.x, contact.z);
                if (!FigureEightTrackMath.IsWithinAnnulus(p, leftCenter, ringRadius, trackHalfWidth)
                    && !FigureEightTrackMath.IsWithinAnnulus(p, rightCenter, ringRadius, trackHalfWidth))
                    anyOutside = true;
            }
            if (incidents.Update(anyOutside))
                trial.ReportViolation("figure-eight-boundary");
        }
    }
}
