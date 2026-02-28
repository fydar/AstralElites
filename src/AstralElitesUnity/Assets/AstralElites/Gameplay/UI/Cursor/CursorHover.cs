using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public BunnyReference<SfxGroup> HoverSoundAsset;
    public BunnyReference<SfxGroup> ClickSoundAsset;

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

        if (HoverSoundAsset != BunnyReference<SfxGroup>.None)
        {
            AudioManager.Play(HoverSoundAsset);
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

        if (ClickSoundAsset != BunnyReference<SfxGroup>.None)
        {
            AudioManager.Play(ClickSoundAsset);
        }
    }
}
