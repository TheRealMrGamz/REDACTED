using UnityEngine;

public class ScreenTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    
    // Called by animation events
    public void SetFadeAlpha(float alpha)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = alpha;
        }
    }
}