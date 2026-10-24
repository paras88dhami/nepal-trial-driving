using System.Collections.Generic;
using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Attach to entry and exit trigger volumes for U-turn and hill-start sections.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CarManeuverGate : MonoBehaviour
    {
        public enum Action { BeginUTurn, EndUTurn, BeginHill, EndHill }
        [SerializeField] private Action action;
        [SerializeField] private CarDriveController car;
        [SerializeField] private CarUTurnSensor uTurn;
        [SerializeField] private CarHillStartSensor hill;
        private readonly HashSet<Collider> overlaps = new HashSet<Collider>();

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (car == null || other.GetComponentInParent<CarDriveController>() != car
                || !overlaps.Add(other) || overlaps.Count != 1) return;
            switch (action)
            {
                case Action.BeginUTurn: if (uTurn != null) uTurn.Begin(); break;
                case Action.EndUTurn: if (uTurn != null) uTurn.RequestExit(); break;
                case Action.BeginHill: if (hill != null) hill.Begin(); break;
                case Action.EndHill: if (hill != null) hill.RequestExit(); break;
            }
        }

        private void OnTriggerExit(Collider other) => overlaps.Remove(other);
        private void OnDisable() => overlaps.Clear();
    }
}
