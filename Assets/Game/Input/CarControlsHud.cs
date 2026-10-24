using NepalTrial.Vehicles.Car;
using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Input
{
    /// <summary>Prototype indicator for selected and actual transmission state.</summary>
    public sealed class CarControlsHud : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        [SerializeField] private PrototypeCarInput input;
        [SerializeField] private Text label;

        public void Configure(CarDriveController controller, PrototypeCarInput source, Text target)
        {
            car = controller;
            input = source;
            label = target;
            Refresh();
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            if (label == null || car == null) return;
            bool waitingForShift = input != null && input.SelectedGear != car.CurrentGear;
            label.text = "Gear: " + car.CurrentGear +
                (waitingForShift ? " (waiting for " + input.SelectedGear + ")" : "") +
                (car.HandbrakeEngaged ? " | Handbrake ON" : "");
        }
    }
}
