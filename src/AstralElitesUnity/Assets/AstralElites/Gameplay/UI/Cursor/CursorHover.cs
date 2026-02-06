using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SfxGroup HoverSound;
    public SfxGroup ClickSound;

    private Button button;

    private bool isHovering;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnDisable()
    {
        if (isHovering)
        {
            CursorManager.SetCursor("Default");
            isHovering = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
        {
            return;
        }

        CursorManager.SetCursor("Hand");

        if (HoverSound)
        {
            AudioManager.Play(HoverSound);
        }

        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
        {
            return;
        }

        CursorManager.SetCursor("Default");

        isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
        {
            return;
        }

        if (ClickSound)
        {
            AudioManager.Play(ClickSound);
        }
    }
}
