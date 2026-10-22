using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Approximate practice geometry only; NOT official Nepal trial dimensions.
    /// Two visibly marked circles. Exact course geometry awaits verified rules.
    /// </summary>
    public sealed class ProceduralFigureEight : MonoBehaviour
    {
        [SerializeField, Min(3f)] private float radius = 7f;
        [SerializeField, Min(0.25f)] private float lineWidth = 0.2f;
        [SerializeField, Range(24, 240)] private int segments = 96;
        [SerializeField] private Material lineMaterial;

        [ContextMenu("Generate Practice Track")]
        public void Generate()
        {
            // Regeneration is explicit; don't delete any hand-authored scene objects.
            var previous = transform.Find("GeneratedCourse");
            if (previous != null)
            {
                if (Application.isPlaying) Destroy(previous.gameObject);
                else DestroyImmediate(previous.gameObject);
            }

            var root = new GameObject("GeneratedCourse").transform;
            root.SetParent(transform, false);
            CreateLoop(root, "LeftLoop", new Vector3(-radius, 0.03f, 0f));
            CreateLoop(root, "RightLoop", new Vector3(radius, 0.03f, 0f));
        }

        private void CreateLoop(Transform parent, string name, Vector3 center)
        {
            var line = new GameObject(name).AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false);
            line.useWorldSpace = false;
            line.positionCount = segments + 1;
            line.loop = false;
            line.widthMultiplier = lineWidth;
            line.sharedMaterial = lineMaterial;
            line.startColor = line.endColor = Color.yellow;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                line.SetPosition(i, center + new Vector3(
                    Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }
    }
}
