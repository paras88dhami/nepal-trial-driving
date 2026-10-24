using NepalTrial.Vehicles.Car;
using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Samples WheelCollider contact positions to evaluate prototype track limits.
    /// Track boundaries are intentionally approximate and do not represent government measurements.
    /// </summary>
    public sealed class PracticeTrialRuntime : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        [SerializeField] private WheelCollider[] wheels;
        [SerializeField] private Transform[] checkpoints;
        [SerializeField, Min(0.25f)] private float checkpointRadius = 2f;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text violationsLabel;
        [SerializeField] private Text progressLabel;
        [Header("Provisional circular course")]
        [SerializeField] private Vector2 leftCenter = new Vector2(-7f, 0f);
        [SerializeField] private Vector2 rightCenter = new Vector2(7f, 0f);
        [SerializeField] private float radius = 7f;
        [SerializeField] private float roadHalfWidth = 2.1f;
        private readonly TrialBoundaryState boundaryState = new TrialBoundaryState();
        private readonly BoundaryIncidentCounter incidentCounter = new BoundaryIncidentCounter();
        private int nextCheckpoint;
        private bool complete;

        public void Configure(CarDriveController vehicle, WheelCollider[] vehicleWheels, Transform[] gates, Text status, Text violations, Text progress)
        {
            car = vehicle;
            wheels = vehicleWheels;
            checkpoints = gates;
            statusLabel = status;
            violationsLabel = violations;
            progressLabel = progress;
            ResetAttempt();
        }

        public int BoundaryContacts => boundaryState.ViolationCount;
        public int BoundaryIncidents => incidentCounter.IncidentCount;
        public int CheckpointsReached => nextCheckpoint;
        public bool Complete => complete;

        private void FixedUpdate()
        {
            if (wheels == null || car == null || complete) return;
            bool anyWheelOutside = false;
            for (int i = 0; i < wheels.Length; i++)
            {
                var wheel = wheels[i];
                if (wheel == null) continue;
                Vector3 position;
                if (wheel.GetGroundHit(out WheelHit hit))
                    position = hit.point;
                else
                    position = wheel.transform.position - wheel.transform.up * wheel.radius;

                var point = new Vector2(position.x, position.z);
                bool onLeft = FigureEightTrackMath.IsWithinAnnulus(point, leftCenter, radius, roadHalfWidth);
                bool onRight = FigureEightTrackMath.IsWithinAnnulus(point, rightCenter, radius, roadHalfWidth);
                string wheelId = i.ToString();
                if (!onLeft && !onRight)
                {
                    anyWheelOutside = true;
                    boundaryState.Enter(wheelId, "outside-provisional-track");
                }
                else
                {
                    boundaryState.Exit(wheelId, "outside-provisional-track");
                }
            }

            incidentCounter.Update(anyWheelOutside);

            if (checkpoints != null && nextCheckpoint < checkpoints.Length)
            {
                var cp = checkpoints[nextCheckpoint];
                if (cp != null && Vector3.Distance(
                    new Vector3(car.transform.position.x, 0, car.transform.position.z),
                    new Vector3(cp.position.x, 0, cp.position.z)) <= checkpointRadius)
                {
                    nextCheckpoint++;
                    if (nextCheckpoint == checkpoints.Length) complete = true;
                }
            }
            RefreshHud();
        }

        private void Start() => RefreshHud();

        public void ResetAttempt()
        {
            boundaryState.Reset();
            incidentCounter.Reset();
            nextCheckpoint = 0;
            complete = false;
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (violationsLabel != null) violationsLabel.text = "Line incidents: " + BoundaryIncidents;
            if (progressLabel != null) progressLabel.text = "Checkpoints: " + nextCheckpoint +
                "/" + (checkpoints != null ? checkpoints.Length : 0);
            if (statusLabel != null)
                statusLabel.text = complete ? "Practice complete" : "Practice: approximate track only";
        }
    }
}
