using UnityEngine;
using UnityEngine.Events;

public class TriggerValueController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maximum value that can be reached")]
    public float maxValue = 1.0f;
    
    [Tooltip("How fast the value increases when in trigger")]
    public float increaseSpeed = 0.5f;
    
    [Tooltip("How fast the value decreases when outside trigger")]
    public float decreaseSpeed = 0.5f;
    
    [Tooltip("Current value (read-only at runtime)")]
    [SerializeField] private float currentValue = 0.0f;
    
    private bool playerInTrigger = false;
    private float previousValue = 0f;
    
    // Property to access the current value
    public float CurrentValue => currentValue;
    
    // Event that fires whenever the value changes, passing the new value
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    
    [Header("Events")]
    public FloatEvent OnValueChanged = new FloatEvent();
    public UnityEvent OnPlayerEnter = new UnityEvent();
    public UnityEvent OnPlayerExit = new UnityEvent();
    
    // Action that can be set from other scripts (using C# delegates)
    public System.Action<float> externalValueChangeAction;
    
    private void Update()
    {
        previousValue = currentValue;
        
        // Increase value when player is in trigger
        if (playerInTrigger)
        {
            currentValue += increaseSpeed * Time.deltaTime;
            currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
        }
        // Decrease value when player is not in trigger
        else
        {
            currentValue -= decreaseSpeed * Time.deltaTime;
            currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
        }
        
        // If value changed, invoke events and call external function
        if (previousValue != currentValue)
        {
            // Unity Event for Inspector connections
            OnValueChanged.Invoke(currentValue);
            
            // C# Action for code-based connections
            externalValueChangeAction?.Invoke(currentValue);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player entering the trigger
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            OnPlayerEnter.Invoke();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if it's the player exiting the trigger
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            OnPlayerExit.Invoke();
        }
    }
    
    // Public method to manually set the value (optional)
    public void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0f, maxValue);
    }
}