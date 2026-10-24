using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Observes BOTH wheel contacts on an oriented narrow plank for a required distance.</summary>
    public sealed class TwoWheelerPlankSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private WheelCollider frontWheel;
        [SerializeField] private WheelCollider rearWheel;
        [SerializeField] private BoxCollider plankRegion;
        [SerializeField] private string stageId = "narrow-plank";
        [SerializeField, Min(.5f)] private float requiredDistanceMeters = 5f;
        [SerializeField, Min(1f)] private float maximumSeconds = 20f;
        private NarrowPlankValidator validator;
        private bool begun, finished;
        private Vector3 previousPosition;

        private void Awake() => validator=new NarrowPlankValidator(requiredDistanceMeters,maximumSeconds);
        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt() { begun=false; finished=false; }

        public static bool ContainsContact(BoxCollider volume,WheelCollider wheel)
        {
            if (volume == null || wheel == null || !wheel.isGrounded) return false;
            if (!wheel.GetGroundHit(out WheelHit hit)) return false;
            Vector3 p=volume.transform.InverseTransformPoint(hit.point)-volume.center;
            Vector3 half=volume.size*.5f;
            return Mathf.Abs(p.x) <= half.x && Mathf.Abs(p.z) <= half.z;
        }

        private void FixedUpdate()
        {
            if (trial == null || vehicle == null || validator == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId ||
                finished) return;
            bool onPlank=ContainsContact(plankRegion,frontWheel) &&
                         ContainsContact(plankRegion,rearWheel);
            if (!begun)
            {
                if (!onPlank) return;
                validator.Begin();
                previousPosition=vehicle.transform.position;
                begun=true;
                return;
            }
            Vector3 current=vehicle.transform.position;
            Vector3 travel=current-previousPosition;
            previousPosition=current;
            float signedForward=Vector3.Dot(travel,
                plankRegion == null ? transform.forward : plankRegion.transform.forward);
            CourseEvent result=validator.Sample(signedForward,Time.fixedDeltaTime,
                onPlank,vehicle.FootDown);
            if (result.Kind == CourseEventKind.None) return;
            finished=true;
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(result.Code);
        }
    }
}
