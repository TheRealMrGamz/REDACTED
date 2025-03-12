using UnityEngine;

public class HallwayEndTrigger : MonoBehaviour
{
    [Header("Settings")]
    public bool isDoorTrigger = true;  // TRUE for door trigger, FALSE for other triggers
    public Color gizmoColor = Color.red;
    public Vector3 triggerSize = new Vector3(3, 3, 0.5f);

    void OnDrawGizmos()
    {
        Gizmos.color = isDoorTrigger ? Color.green : Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, triggerSize);
    }

    void Start()
    {
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = triggerSize;
            Debug.Log($"[HallwayEndTrigger] Added BoxCollider to {gameObject.name} with size {triggerSize}");
        }
    }
}