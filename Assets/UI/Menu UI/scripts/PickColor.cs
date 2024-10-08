using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PickColor : MonoBehaviour
{
    public Animator container; // The container that holds the color buttons
    public Image player1Dis;   // Image for Player 1's display
    public Image player2Dis;   // Image for Player 2's display
    private Button swapButton;
    public Sprite[] colors;    // Array of available color sprites
    public Button[] colorButtons; //Array of color buttons

    public PhotonView view;    // PhotonView for networked actions
    private bool isAnimating = false; // Is an animation in progress?
    public Button blueButton;  // Reference to blue button

    private int previousColorIndex = 0;   // To track the previous color
    private int currentColorIndex = 0;    // To track the current color
    private bool isShowing = false;   // Is the color picker open?

    private void Start()
    {
        // Initialize with the first color (index 0)
        currentColorIndex = 0;
        previousColorIndex = 0;
        //SetUpListeners();
    }

    private void SetUpListeners()
    {
        Button button;
        colorButtons[0].onClick.AddListener(() => ToggleContainer());

        for (int i = 1; i < colorButtons.Length - 1; i++)
        {
            int capturedIndex = i;  // Capture the current value of i
            button = colorButtons[i];
            button.onClick.AddListener(() => SetColor(capturedIndex));  // Use the captured index
            button.onClick.AddListener(() => ToggleContainer());
        }
    }


    public void ToggleContainer()
    {
        // Prevent toggling while an animation is playing
        if (isAnimating)
        {
            blueButton.interactable = false;
            return;
        }

        if (isShowing)
        {
            StartCoroutine(PlayAnimation("Close"));
            isShowing = false;
        }
        else
        {
            StartCoroutine(PlayAnimation("Open"));
            isShowing = true;
        }
    }

    private IEnumerator PlayAnimation(string trigger)
    {
        // Set the animation trigger and mark as animating
        isAnimating = true;
        container.SetTrigger(trigger);

        // Get the current animation length
        AnimatorStateInfo stateInfo = container.GetCurrentAnimatorStateInfo(0);
        float animationDuration = stateInfo.length;

        // Wait for the animation to finish
        yield return new WaitForSeconds(animationDuration);

        // Allow further interaction after the animation completes
        blueButton.interactable = true;
        isAnimating = false;
    }

    public void SetColor(int index)
    {
        view.RPC("ChangeColor", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, index);
    }

    [PunRPC]
    public void ChangeColor(int actorNumber, int newColorIndex)
    {
        // Check if the player changing color is Player 1 or Player 2, based on the actor number passed in the RPC
        if (actorNumber == 1)
        {
            Debug.Log("Setting color value to " +  newColorIndex);
            player1Dis.sprite = colors[newColorIndex];  // Update Player 1's image 
        }
        else if (actorNumber == 2)
        {

            if(newColorIndex == 0)
            {
                newColorIndex = 2;
            }
            else if(newColorIndex == 2)
            {
                newColorIndex = 0;
            }

            player2Dis.sprite = colors[newColorIndex];  // Update Player 2's image color
        }
    }

    private void UpdateLastColorButton()
    {
        // This method updates the last button to reflect the player's previous color.
        swapButton.GetComponent<Image>().sprite = colors[previousColorIndex];
        swapButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = previousColorIndex.ToString();
        // Assign the last button to correctly pass the previous color's index

        if (swapButton != colorButtons[0])
        {
            swapButton.onClick.RemoveAllListeners();
            swapButton.onClick.AddListener(() => SetColor(previousColorIndex));
            swapButton.onClick.AddListener(() => ToggleContainer());
        }
    }
}
