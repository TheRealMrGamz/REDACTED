using System.Collections;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject firstObject;
    [SerializeField] private GameObject secondObject;
    
    [Header("Movement Settings")]
    [SerializeField] private float firstObjectSpeed = 1.0f;
    [SerializeField] private float moveDownDuration = 2.0f;
    [SerializeField] private Vector3 targetPosition = new Vector3(0, 0, 0);
    [SerializeField] private float secondObjectSpeed = 3.0f;
    
    private bool isMovementInProgress = false;

    /// <summary>
    /// Call this function to move firstObject down slowly, then move secondObject to targetPosition
    /// </summary>
    public void MoveObjectsSequentially()
    {
        if (!isMovementInProgress)
        {
            StartCoroutine(MoveObjectsCoroutine());
        }
    }
    
    private IEnumerator MoveObjectsCoroutine()
    {
        isMovementInProgress = true;
        
        // First, move the first object down
        Vector3 startPosition = firstObject.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - moveDownDuration * firstObjectSpeed, startPosition.z);
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDownDuration)
        {
            firstObject.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDownDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the object reaches its exact destination
        firstObject.transform.position = endPosition;
        
        // Then, move the second object to the target position
        startPosition = secondObject.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float secondMoveDuration = distance / secondObjectSpeed;
        
        elapsedTime = 0f;
        
        while (elapsedTime < secondMoveDuration)
        {
            secondObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / secondMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the second object reaches its exact destination
        secondObject.transform.position = targetPosition;
        
        isMovementInProgress = false;
    }
    
    // You can call this function to change the target position at runtime
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }
}