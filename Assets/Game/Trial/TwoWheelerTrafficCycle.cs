using UnityEngine;

namespace NepalTrial.Trial
{
    public sealed class TwoWheelerTrafficCycle : MonoBehaviour
    {
        [SerializeField] private TwoWheelerTrafficSignalSensor stopLine;
        [SerializeField] private Renderer lamp;
        [SerializeField, Min(1f)] private float redSeconds=8f;
        [SerializeField, Min(1f)] private float greenSeconds=10f;
        private float elapsed;
        private TrafficLightColor current=TrafficLightColor.Yellow;

        private void Update()
        {
            float cycle=Mathf.Max(2f,redSeconds+greenSeconds);
            elapsed=(elapsed+Time.deltaTime)%cycle;
            var color=elapsed<redSeconds ? TrafficLightColor.Red : TrafficLightColor.Green;
            if(color==current)return;
            current=color;
            if(stopLine!=null)stopLine.SetLight(color);
            if(lamp!=null)lamp.material.color=color==TrafficLightColor.Red?Color.red:Color.green;
        }
    }
}
