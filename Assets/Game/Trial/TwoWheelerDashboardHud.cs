using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>Gear, speed, figure-eight checkpoint count and indicators for generated scenes.</summary>
    public sealed class TwoWheelerDashboardHud : MonoBehaviour
    {
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private TwoWheelerFigureEightSensor figureEight;
        [SerializeField] private Text dashboardText;

        private void Update()
        {
            if (vehicle == null || dashboardText == null) return;
            string gear = vehicle.Kind == TwoWheelerKind.Scooter ? "AUTO" : vehicle.CurrentGear.ToString();
            string checkpoints = figureEight != null
                ? " | Gates " + figureEight.Reached + "/" + figureEight.Total : "";
            dashboardText.text = vehicle.Kind + " | " + gear + " | " +
                Mathf.RoundToInt(vehicle.SpeedKph) + " km/h" +
                (vehicle.LeftIndicator ? " | Left signal" : vehicle.RightIndicator ? " | Right signal" : "") +
                checkpoints;
        }
    }
}
