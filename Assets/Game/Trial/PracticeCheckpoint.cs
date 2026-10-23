using UnityEngine;

namespace NepalTrial.Trial
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PracticeCheckpoint : MonoBehaviour
    {
        [SerializeField, Min(0)] private int index;
        public int Index => index;
        private void Reset() { GetComponent<BoxCollider>().isTrigger = true; }
    }
}
