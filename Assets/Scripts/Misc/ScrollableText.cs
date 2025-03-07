using UnityEngine;

public class ScrollableText : MonoBehaviour
{
    public Transform textTransform;
    public float scrollSpeed = 50f;
    public float upperLimit = 500f;
    public float lowerLimit = 0f;

    public bool autoScroll = true;

    void Update()
    {
        if (autoScroll)
        {
            ScrollText(Vector3.up);
        }
        else
        {
            HandleInput();
        }

        CheckBounds();
    }
    private void ScrollText(Vector3 direction)
    {
        textTransform.localPosition += direction * scrollSpeed * Time.deltaTime;
    }

    private void HandleInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0)
        {
            ScrollText(Vector3.up);
        }
        else if (scroll < 0)
        {
            ScrollText(Vector3.down);
        }
    }

    private void CheckBounds()
    {
        if (textTransform.localPosition.y > upperLimit)
        {
            textTransform.localPosition = new Vector3(
                textTransform.localPosition.x, 
                lowerLimit, 
                textTransform.localPosition.z);
        }
    }
}