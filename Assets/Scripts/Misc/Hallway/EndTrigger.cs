using UnityEngine;

public class HallwayTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Color gizmoColor = Color.red;
    public Vector3 triggerSize = new Vector3(3, 3, 3f);
    
    void OnDrawGizmos()
    {
        // Draw visible gizmo in the editor
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, triggerSize);
    }
    
    void Start()
    {
        // Make sure we have a collider
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = triggerSize;
        }
    }
}