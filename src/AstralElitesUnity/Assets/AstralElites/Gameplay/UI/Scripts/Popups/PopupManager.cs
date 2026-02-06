using System;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    public GameObjectPool<Popup>[] Popups;

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < Popups.Length; i++)
        {
            Popups[i].Flush();
        }
    }

    public T GetPopup<T>()
        where T : Popup
    {
        for (int i = 0; i < Popups.Length; i++)
        {
            var popup = Popups[i];

            if (popup.Template.GetType() == typeof(T))
            {
                return (T)popup.Grab(transform);
            }
        }
        return null;
    }

    public GameObjectPool<Popup> GetPopupPool(Type type)
    {
        for (int i = 0; i < Popups.Length; i++)
        {
            var popup = Popups[i];

            if (popup.Template.GetType() == type)
            {
                return popup;
            }
        }
        return null;
    }
}
