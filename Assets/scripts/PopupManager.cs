using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    public TextMeshProUGUI text;
    public Image popupContainer;
    bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowMessage(string message)
    {
        if(!isOpen)
        {
            popupContainer.gameObject.SetActive(true);
            isOpen = true;
            text.text = message;
        }
    }

    public void HideMessage()
    {
        popupContainer.gameObject.SetActive(false);
        isOpen = false;
    }
}
