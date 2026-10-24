using System;
using System.Collections.Generic;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Tracks wheel contact with marked boundary trigger volumes. Prototype, not official scoring.</summary>
    public sealed class BoundaryDetector : MonoBehaviour
    {
        [SerializeField] private string wheelTag = "PlayerWheel";
        [SerializeField] private float rearmSeconds = 1.5f;
        private readonly Dictionary<EntityId, float> lastSeen = new Dictionary<EntityId, float>();
        public event Action<string> BoundaryHit;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(wheelTag)) return;
            EntityId id = other.GetEntityId();
            float now = Time.time;
            if (lastSeen.TryGetValue(id, out var previous) && now - previous < rearmSeconds) return;
            lastSeen[id] = now;
            BoundaryHit?.Invoke(gameObject.name);
        }
    }
}


