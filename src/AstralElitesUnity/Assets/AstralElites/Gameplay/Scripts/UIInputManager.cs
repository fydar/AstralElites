using UnityEngine;
using UnityEngine.EventSystems;

public class UIInputManager : MonoBehaviour
{
    private void Update()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
