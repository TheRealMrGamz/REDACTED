using UnityEngine;

public class ObjectRotator1 : MonoBehaviour
{
    public float rotationSpeed = 90f; // Adjust this to control rotation speed
    private bool shouldRotate = false;
    private float currentRotation = 0f;
    private float targetAngle = 15f; // Changed to positive 90 degrees to rotate upward

    public void StartRotation()
    {
        shouldRotate = true;
        currentRotation = 0f;
    }

    private void Update()
    {
        if (shouldRotate && currentRotation < targetAngle) // Changed to < since we're rotating to positive
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime; // Positive to rotate upward
            
            if (currentRotation + rotationThisFrame > targetAngle)
            {
                rotationThisFrame = targetAngle - currentRotation;
                shouldRotate = false;
            }
            
            transform.Rotate(Vector3.right, rotationThisFrame); // Still using Vector3.right, but now rotating positively
            currentRotation += rotationThisFrame;
        }
    }
}