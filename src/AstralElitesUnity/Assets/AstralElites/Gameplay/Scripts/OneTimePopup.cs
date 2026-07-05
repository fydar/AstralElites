using UnityEngine;

public class OneTimePopup : MonoBehaviour
{
    public string ID = "Popup ID";

    private void Awake()
    {
        if (PlayerPrefs.GetInt(ID, 0) == 1
            || Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Close();
        }
    }

    public void DontShowAgain()
    {
        PlayerPrefs.SetInt(ID, 1);
        Close();
    }

    public void RemindMeLater()
    {
        Close();
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
