using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Connected car L-back route: enter the longitudinal approach in Drive,
    /// turn toward the cross-leg, then reverse into the bay without leaving
    /// either lane. The separate parking-volume sensor verifies the final stop.
    /// </summary>
    public sealed class CarLBackPathSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        [SerializeField] private BoxCollider approachLeg;
        [SerializeField] private BoxCollider reverseLeg;
        [SerializeField] private WheelCollider[] wheels;
        [SerializeField, Min(.5f)] private float requiredForwardMeters = 3f;
        [SerializeField, Min(.5f)] private float requiredReverseMeters = 3f;
        [SerializeField, Min(.02f)] private float boundaryToleranceMeters = .12f;

        private bool enteredApproach, reversing, correctionInProgress, boundaryReported;
        private float forwardDistance, reverseDistance;
        private Vector3 previousPosition;
        private bool previousKnown;

        public bool CanComplete => enteredApproach && reversing &&
            forwardDistance >= requiredForwardMeters &&
            reverseDistance >= requiredReverseMeters;
        public bool HasReversed => reversing;

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
            enteredApproach=reversing=correctionInProgress=boundaryReported=false;
            forwardDistance=reverseDistance=0f;
            previousKnown=false;
        }

        private static bool InsideXZ(BoxCollider volume,Vector3 point,float margin)
        {
            if (volume==null)return false;
            Vector3 p=volume.transform.InverseTransformPoint(point)-volume.center;
            Vector3 half=volume.size*.5f;
            return Mathf.Abs(p.x)<=half.x+margin &&
                   Mathf.Abs(p.z)<=half.z+margin;
        }

        private void FixedUpdate()
        {
            if (trial==null || car==null || approachLeg==null || reverseLeg==null ||
                trial.State!=ExamStage.Running || trial.IsPaused ||
                trial.CurrentStage!="garage")
            {
                previousKnown=false;
                return;
            }

            Vector3 position=car.transform.position;
            if(!enteredApproach && InsideXZ(approachLeg,position,-.15f))
                enteredApproach=true;
            if(!enteredApproach){previousPosition=position;previousKnown=true;return;}

            if(previousKnown)
            {
                Vector3 delta=position-previousPosition;
                if(car.CurrentGear==DriveGear.Drive &&
                   InsideXZ(approachLeg,position,boundaryToleranceMeters) &&
                   !reversing)
                    forwardDistance+=Mathf.Max(0f,Vector3.Dot(delta,approachLeg.transform.forward));

                if(car.CurrentGear==DriveGear.Reverse &&
                   forwardDistance>=requiredForwardMeters &&
                   InsideXZ(reverseLeg,position,boundaryToleranceMeters))
                {
                    reversing=true;
                    correctionInProgress=false;
                    reverseDistance+=Mathf.Max(0f,Vector3.Dot(delta,-car.transform.forward));
                }
                else if(reversing && car.CurrentGear==DriveGear.Drive &&
                    !correctionInProgress && car.SpeedKph>.25f)
                {
                    correctionInProgress=true;
                    trial.ReportViolation("garage-forward-correction");
                }
            }
            previousKnown=true;
            previousPosition=position;

            if(boundaryReported || wheels==null || wheels.Length==0)return;
            foreach(WheelCollider wheel in wheels)
            {
                if(wheel==null)continue;
                WheelHit hit;
                Vector3 point=wheel.GetGroundHit(out hit)?hit.point:
                    wheel.transform.position-wheel.transform.up*wheel.radius;
                if(!InsideXZ(approachLeg,point,boundaryToleranceMeters) &&
                   !InsideXZ(reverseLeg,point,boundaryToleranceMeters))
                {
                    // A tire outside the permitted L-shaped lane is fatal.
                    boundaryReported=true;
                    trial.ReportViolation("garage-line-touch");
                    break;
                }
            }
        }
    }
}
