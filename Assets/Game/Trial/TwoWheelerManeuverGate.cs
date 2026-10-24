using System.Collections.Generic;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    [RequireComponent(typeof(Collider))]
    public sealed class TwoWheelerManeuverGate : MonoBehaviour
    {
        public enum Action { BeginUTurn, ExitUTurn, BeginRamp, ExitRamp }
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private TwoWheelerUTurnSensor uTurn;
        [SerializeField] private TwoWheelerRampSensor ramp;
        [SerializeField] private Action action;
        private readonly HashSet<Collider> overlaps=new HashSet<Collider>();

        private void Reset() => GetComponent<Collider>().isTrigger=true;
        private void OnEnable(){ if(trial!=null)trial.AttemptRestarted+=Clear; }
        private void OnDisable(){ if(trial!=null)trial.AttemptRestarted-=Clear; Clear(); }
        private void Clear()=>overlaps.Clear();

        private void OnTriggerEnter(Collider other)
        {
            if(vehicle==null ||
                other.GetComponentInParent<TwoWheelerRigidbodyController>()!=vehicle ||
                !overlaps.Add(other) || overlaps.Count!=1) return;
            switch(action)
            {
                case Action.BeginUTurn: if(uTurn!=null)uTurn.Begin(); break;
                case Action.ExitUTurn: if(uTurn!=null)uTurn.Exit(); break;
                case Action.BeginRamp: if(ramp!=null)ramp.Begin(); break;
                case Action.ExitRamp: if(ramp!=null)ramp.Exit(); break;
            }
        }
        private void OnTriggerExit(Collider other)=>overlaps.Remove(other);
    }
}
