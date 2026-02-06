using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LinkButton : MonoBehaviour
{
    public string LinkLocation = "https://link.com/";

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(ClickListener);
    }

    private void ClickListener()
    {
        Application.OpenURL(LinkLocation);
    }
}
