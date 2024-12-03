using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System.Collections;
using System.Xml.Serialization;

public class Health : MonoBehaviour
{
    PhotonView view;
    GameObject player;
    public Image icon;
    public CapsuleCollider2D bc;
    public CapsuleCollider2D bc2;
    public SpriteRenderer sr_body, sr_arms, sr_legs;
    public Rigidbody2D rb;
    public SpriteRenderer gun;
    private Transform respawnArea;
    public float healthAmount;
    [SerializeField] public Image fillImage;
    public Sprite[] coloredIcons;

    public float duration;
    public float magnitude;

    static int lives;
    public CameraMove camera;

    public SpriteRenderer[] sprites;

    public bool isDead;

    [SerializeField] float healthBarAninDuraition = 0.2f;

    [Header("Dash UI")]
    public Image dashBar;
    private TextMeshProUGUI coolDownText;

    [Header("Health UI")]
    [SerializeField] HealthBar playerHealthBar;
    public SpawnPlayers spawnManager;

    private void Start()
    {
        isDead = false;
        view = GetComponent<PhotonView>();
        respawnArea = GameObject.FindGameObjectWithTag("RespawnArea").transform;
        if (PhotonNetwork.IsMasterClient)
        {
            view.RPC("SetUpLivesDisplay", RpcTarget.All);
        }
    }

    private void AssignDashUI(Image image)
    {
        dashBar = image;
        coolDownText = image.GetComponentInChildren<TextMeshProUGUI>();
        if(!view.IsMine)
        {
            dashBar.enabled = false;
        }
    }

    [PunRPC]
    private void SetUpLivesDisplay()
    {
        Debug.Log($"SetUpLivesDisplay called by {view.Owner.NickName} at {Time.time}");

        // Fetch the StartingLives value from room properties
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartingLives", out object startingLives))
        {
             lives = (int)startingLives;
        }
        else
        {
            // Fallback if the property is missing
            lives = 10;
        }

        // Update the health bar displays for all players
        foreach (GameObject healthBar in spawnManager.healthBarList)
        {
            healthBar.GetComponent<HealthBar>().GetLivesDisplayView().RPC("SetLives", RpcTarget.AllBuffered, lives);
        }
    }



    private void Update()
    {
        if (!isDead)
        {
            CheckHealth();
        }
    }

    private void CheckHealth()
    {
        fillImage.fillAmount = healthAmount / 100;

        // Only the owner will trigger the death
        if (view.IsMine && healthAmount <= 0 && !isDead)
        {
            isDead = true;
            healthAmount = 0;
            fillImage.fillAmount = 0;
            view.RPC("Death", RpcTarget.All);
        }
    }





    [PunRPC]
    public void ReduceHealth(float amount)
    {
        // Reduce the health and make sure it does not go below zero
        healthAmount = Mathf.Max(healthAmount - amount, 0);

        // Broadcast the health update to all clients including self
        view.RPC("UpdateHealthUI", RpcTarget.Others, view.ViewID, (float)healthAmount);
    }

    [PunRPC]
    public void UpdateHealthUI(int targetViewID, float healthAmount)
    {
        if (view.ViewID == targetViewID)
        {
            Debug.Log($"Updating health for PlayerViewID: {targetViewID} with health amount: {healthAmount}");
            this.healthAmount = healthAmount; // Set the player's health amount to the new value
            view.RPC("HealthBarAnimation", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void HealthBarAnimation()
    {
        LeanTween.value(fillImage.gameObject, fillImage.fillAmount, healthAmount / 100, healthBarAninDuraition)
                .setOnUpdate((float value) => {
                    fillImage.fillAmount = value;
                })
                .setEaseInOutQuad();  // You can choose the easing type that best fits your game
    }

    [PunRPC]
    public void CallDashUIAnimationForView(int cooldown)
    {
        StartCoroutine("DashUIAnimation", cooldown);
    }


    public IEnumerator DashUIAnimation(int cooldown)
    {

        dashBar.color = new Color(0.35f, 0.35f, 0.35f); // RGB values for gray
        for (int i = cooldown; i > 0; i--) 
        { 
            coolDownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        dashBar.color = Color.white;
        coolDownText.text = null;
    }


    [PunRPC]
    public void AssignHealthBar(int playerViewID, int playerCount)
    {
        object colorChoice;
        PhotonView targetPhotonView = PhotonView.Find(playerViewID);
        spawnManager = GameObject.FindGameObjectWithTag("SpawnPlayer").GetComponent<SpawnPlayers>();

        if (targetPhotonView != null)
        {
            player = targetPhotonView.gameObject;

            // Activate the health bar for the player
            playerHealthBar = spawnManager.healthBarList[playerCount - 1].GetComponent<HealthBar>();
            playerHealthBar.gameObject.SetActive(true);

            // Assign health bar fill image
            player.GetComponent<Health>().fillImage = playerHealthBar.GetFillImage();

            // Assign the icon
            icon = playerHealthBar.GetIcon();
            //Assign dash ui
            AssignDashUI(playerHealthBar.GetDashUI());
            // Retrieve the "PlayerColor" property for customization
            if (targetPhotonView.Owner.CustomProperties.TryGetValue("PlayerColor", out colorChoice))
            {
                int colorIndex = (int)colorChoice;
                icon.sprite = coloredIcons[colorIndex];
            }
        }
    }
    IEnumerator FadeInSprites()
    {
        float counter = 0;
        while (counter < duration)
        {
            float alpha = Mathf.Lerp(0, 1, counter / duration);
            foreach (var sprite in sprites)
            {
                Color currentColor = sprite.color;
                currentColor.a = alpha;
                sprite.color = currentColor;
            }
            counter += Time.deltaTime;
            yield return null;
        }

        foreach (var sprite in sprites)
        {
            Color finalColor = sprite.color;
            finalColor.a = 1;
            sprite.color = finalColor;
        }
    }
    [PunRPC]
    public void UpdateLifeCounterOnAllClients(int playerViewID)
    {
        if (view.ViewID == playerViewID) // Ensure this code runs only for the player who owns the view
        {
            lives--;

            // Update the lives on all clients using the player's specific LifeCounterText
            playerHealthBar.GetLivesDisplayView().RPC("SetLives", RpcTarget.AllBuffered, lives);

            if (lives <= 0 && PhotonNetwork.IsMasterClient)
            {
                // Only the MasterClient should handle game-ending logic
                view.RPC("DisableGame", RpcTarget.AllBuffered);

                Player lastAlive = FindLastAlivePlayer();
                if (lastAlive != null)
                {
                    Hashtable winnerProps = new Hashtable
                {
                    { "Winner", lastAlive.ActorNumber }
                };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(winnerProps);
                }
            }
        }
    }




    [PunRPC]
    private void DisableGame()
    {
        StartCoroutine(timer());
    }

    IEnumerator timer()
    {
        yield return new WaitForSeconds(1.0f);
        Time.timeScale = 0f;
    }

    private Player FindLastAlivePlayer()
    {
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            Player lastAlive = null;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerLives;
                if (p.CustomProperties.TryGetValue("Lives", out playerLives) && (int)playerLives > 0)
                {
                    lastAlive = p;
                }
            }
            return lastAlive;
        }
        return PhotonNetwork.LocalPlayer;
    }
}
