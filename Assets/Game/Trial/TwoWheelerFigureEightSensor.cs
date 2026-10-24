using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Directional figure-eight gates and per-excursion wheel-boundary checking.</summary>
    public sealed class TwoWheelerFigureEightSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private WheelCollider frontWheel;
        [SerializeField] private WheelCollider rearWheel;
        [SerializeField] private Transform[] checkpoints;
        [SerializeField] private string stageId = "figure-eight";
        [SerializeField] private Vector2 leftCenter = new Vector2(-7f,0f);
        [SerializeField] private Vector2 rightCenter = new Vector2(7f,0f);
        [SerializeField, Min(1f)] private float radius = 7f;
        [SerializeField, Min(.2f)] private float halfWidth = 1.8f;
        [SerializeField, Min(.2f)] private float gateHalfWidth = 1.8f;
        private OrderedDrivingGateTracker tracker;
        private readonly BoundaryIncidentCounter boundary = new BoundaryIncidentCounter();
        private FigureEightContactMonitor lineMonitor;
        private Vector3 previousPosition;
        private bool known, finished;

        public int Reached => tracker == null ? 0 : tracker.NextIndex;
        public int Total => tracker == null ? 0 : tracker.Total;
        public int Incidents => boundary.IncidentCount;

        private void Awake()
        {
            if (checkpoints == null || checkpoints.Length == 0)
            {
                Debug.LogError("Assign ordered two-wheeler figure-eight gate transforms.",this);
                enabled=false;
                return;
            }
            Vector3[] points=new Vector3[checkpoints.Length];
            Vector3[] forward=new Vector3[checkpoints.Length];
            for (int i=0;i<checkpoints.Length;i++)
            {
                if (checkpoints[i] == null) { enabled=false; return; }
                points[i]=checkpoints[i].position;
                forward[i]=checkpoints[i].forward;
            }
            tracker=new OrderedDrivingGateTracker(points,forward,gateHalfWidth);
            lineMonitor=new FigureEightContactMonitor(radius,halfWidth);
        }

        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }

        public void ResetAttempt()
        {
            if (tracker != null) tracker.Reset();
            boundary.Reset();
            if(lineMonitor!=null)lineMonitor.Reset();
            known=false; finished=false;
        }

        private static Vector2 ContactPoint(WheelCollider wheel)
        {
            WheelHit hit;
            Vector3 p = wheel.GetGroundHit(out hit) ? hit.point :
                wheel.transform.position - wheel.transform.up * wheel.radius;
            return new Vector2(p.x,p.z);
        }

        private void FixedUpdate()
        {
            if (trial == null || vehicle == null || tracker == null || finished ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId)
            { known=false; return; }
            Vector3 current=vehicle.transform.position;
            if (known && tracker.Sample(previousPosition,current) && tracker.Finished)
            {
                finished=trial.ReportStage(stageId);
                if (finished) return;
            }
            known=true; previousPosition=current;
            if (tracker.NextIndex == 0 || frontWheel == null || rearWheel == null) return;
            Vector2 front=ContactPoint(frontWheel);
            Vector2 rear=ContactPoint(rearWheel);
            FigureEightContactEvent contact=lineMonitor.Sample(
                new[] {front,rear},leftCenter,rightCenter);
            if(contact==FigureEightContactEvent.Outside)
            {
                boundary.Update(true);
                trial.ReportViolation("figure-eight-boundary");
            }
            else if(contact==FigureEightContactEvent.LineTouch)
                trial.ReportViolation("figure-eight-line-touch");
            else boundary.Update(false);
        }
    }
}
