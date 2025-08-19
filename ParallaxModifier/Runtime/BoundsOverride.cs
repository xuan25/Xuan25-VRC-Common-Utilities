
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
[ExecuteInEditMode]
public class BoundsOverride : UdonSharpBehaviour
{
    [SerializeField]
    private Vector3 center = Vector3.zero;

    [SerializeField]
    private Vector3 extents = Vector3.one;

    private void AdjustBounds()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
            return;
        renderer.bounds = new Bounds(center, extents * 2);
    }

    void Start()
    {
        AdjustBounds();
    }

#if UNITY_EDITOR

    void OnValidate()
    {
        AdjustBounds();
    }

    private Bounds GetCurrentBounds()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
            return new Bounds();

        return renderer.bounds;
    }

    private Bounds GetOriginalBounds()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
            return new Bounds();

        Bounds temp = renderer.bounds;
        renderer.ResetBounds();
        Bounds originalBounds = renderer.bounds;
        renderer.bounds = temp;

        return originalBounds;
    }

    // Draws a wireframe box around the selected object,
    // indicating world space bounding volume.
    public void OnDrawGizmosSelected()
    {
        Bounds currentBounds = GetCurrentBounds();
        Bounds originalBounds = GetOriginalBounds();

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(originalBounds.center, originalBounds.extents * 2);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(currentBounds.center, currentBounds.extents * 2);

    }

#endif

}
