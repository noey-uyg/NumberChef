using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private Vector2 _minAnchor;
    private Vector2 _maxAnchor;

    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        _minAnchor = Screen.safeArea.min;
        _maxAnchor = Screen.safeArea.max;
        
        _minAnchor.x /= Screen.width;
        _minAnchor.y /= Screen.height;

        _maxAnchor.x /= Screen.width;
        _maxAnchor .y /= Screen.height;

        rectTransform.anchorMin = _minAnchor;
        rectTransform.anchorMax = _maxAnchor;
    }
}
