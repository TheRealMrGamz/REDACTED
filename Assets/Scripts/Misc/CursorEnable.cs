using UnityEngine;

public class CursorEnable : MonoBehaviour
{
    [SerializeField] private bool showCursor = true;
    [SerializeField] private CursorLockMode lockMode = CursorLockMode.None;

    private void Start()
    {
        Cursor.visible = showCursor;
        Cursor.lockState = lockMode;
    }

    public void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
    }

    public void SetCursorLockState(CursorLockMode mode)
    {
        Cursor.lockState = mode;
    }
}