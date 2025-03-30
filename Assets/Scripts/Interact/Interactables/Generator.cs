using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class FunctionCall
{
    public string functionName;
    public UnityEvent functionEvent;
}

public class Generator : InteractableBase
{
    [SerializeField] private List<FunctionCall> functionCalls = new List<FunctionCall>();
    [SerializeField] private bool executeAllFunctions = true;
    [SerializeField] private int functionIndexToCall = 0;
    
    public override void OnInteract(PSXFirstPersonController player)
    {
        base.OnInteract(player);
        
        if (functionCalls.Count == 0)
            return;
            
        if (executeAllFunctions)
        {
            // Execute all functions in the list
            foreach (var functionCall in functionCalls)
            {
                functionCall.functionEvent?.Invoke();
            }
        }
        else
        {
            // Execute only the specified function
            if (functionIndexToCall >= 0 && functionIndexToCall < functionCalls.Count)
            {
                functionCalls[functionIndexToCall].functionEvent?.Invoke();
            }
        }
    }
}