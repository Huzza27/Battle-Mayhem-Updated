using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimationManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Button button;
    Vector2 origianlScale;
    [SerializeField] public static float scaleFactor = 1.1f;
    [SerializeField] public static float animationDuration = 0.2f;

    private void Start()
    {
        button = GetComponent<Button>(); 
        origianlScale = transform.localScale;
    }

    // Triggered when the mouse enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered the button.");
        // Add your code here, such as changing button color or playing a sound
        HoverEnterAnimation();
    }

    void HoverEnterAnimation()
    {
        LeanTween.scale(button.gameObject, origianlScale * scaleFactor, animationDuration).setEase(LeanTweenType.easeOutBack);
    }

    void HoverExitAnimation()
    {
        LeanTween.scale(button.gameObject, origianlScale, animationDuration).setEase(LeanTweenType.easeInBack);
    }

    // Optional: Triggered when the mouse exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited the button.");
        // Add your code here to reset effects
        HoverExitAnimation();
    }
}
