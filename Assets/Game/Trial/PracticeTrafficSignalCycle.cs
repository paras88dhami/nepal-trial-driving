using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Timed demonstration signal. Durations/colors are not official Nepal exam settings.</summary>
    public sealed class PracticeTrafficSignalCycle : MonoBehaviour
    {
        [SerializeField] private CarTrafficSignalSensor stopLine;
        [SerializeField] private Renderer lamp;
        [SerializeField, Min(1f)] private float redSeconds = 8f;
        [SerializeField, Min(1f)] private float greenSeconds = 12f;
        private float elapsed;
        private TrafficLightColor last = TrafficLightColor.Yellow;

        private void Update()
        {
            float period = redSeconds + greenSeconds;
            if (period <= 0f) return;
            elapsed = (elapsed + Time.deltaTime) % period;
            TrafficLightColor light = elapsed < redSeconds ? TrafficLightColor.Red : TrafficLightColor.Green;
            if (light == last) return;
            last = light;
            if (stopLine != null) stopLine.SetLight(light);
            if (lamp != null) lamp.material.color = light == TrafficLightColor.Green ? Color.green : Color.red;
        }
    }
}
