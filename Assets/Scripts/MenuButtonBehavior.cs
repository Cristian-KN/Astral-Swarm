using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private float offsetAmount = 2f;

    private Vector2 originalPos;

    private void Awake()
    {
        if (textTransform == null) textTransform = transform.Find("Text") as RectTransform;
        if (textTransform != null) originalPos = textTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData) => ApplyOffset(true);
    public void OnPointerUp(PointerEventData eventData) => ApplyOffset(false);
    public void OnSelect(BaseEventData eventData) => ApplyOffset(true);
    public void OnDeselect(BaseEventData eventData) => ApplyOffset(false);

    private void ApplyOffset(bool pressed)
    {
        if (textTransform == null) return;
        textTransform.anchoredPosition = originalPos + (pressed ? new Vector2(0, offsetAmount) : Vector2.zero);
    }
}
