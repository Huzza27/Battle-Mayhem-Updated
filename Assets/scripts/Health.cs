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

    public TextMeshProUGUI livesDisplay;
    int lives;
    public CameraMove camera;

    public SpriteRenderer[] sprites;

    public bool isDead;

    [SerializeField] float healthBarAninDuraition = 0.2f;

    [Header("Dash UI")]
    public Image dashBar;
    private TextMeshProUGUI coolDownText;

    private void Start()
    {
        isDead = false;
        view = GetComponent<PhotonView>();
        respawnArea = GameObject.FindGameObjectWithTag("RespawnArea").transform;
        view.RPC("SetUpLivesDisplay", RpcTarget.All);
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
        livesDisplay.text = GameManager.Instance.StartingLives.ToString();
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
        SpawnPlayers spawnManager = GameObject.FindGameObjectWithTag("SpawnPlayer").GetComponent<SpawnPlayers>();

        if (targetPhotonView != null)
        {
            player = targetPhotonView.gameObject;

            // Activate the health bar for the player
            spawnManager.healthBarList[playerCount - 1].SetActive(true);

            // Assign health bar fill image
            player.GetComponent<Health>().fillImage = spawnManager.healthBarList[playerCount - 1].transform.GetChild(1).GetComponent<Image>();

            // Assign the icon
            icon = spawnManager.healthBarList[playerCount - 1].transform.GetChild(3).GetComponent<Image>();

            // Retrieve the "PlayerColor" property for customization
            if (targetPhotonView.Owner.CustomProperties.TryGetValue("PlayerColor", out colorChoice))
            {
                int colorIndex = (int)colorChoice;
                icon.sprite = coloredIcons[colorIndex];
            }
            else
            {
                icon.sprite = coloredIcons[DetermineDefaultColorFromActorNumber(PhotonNetwork.LocalPlayer.ActorNumber)];
            }

            // Assign the lives display UI
            livesDisplay = spawnManager.healthBarList[playerCount - 1].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            AssignDashUI(spawnManager.healthBarList[playerCount - 1].transform.GetChild(2).GetComponent<Image>());
        }
    }

    private int DetermineDefaultColorFromActorNumber(int actorNumber)
    {
        return actorNumber == 1 ? 0 : 2;
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
    public void UpdateLifeCounterOnAllClients()
    {
        lives--;
        livesDisplay.text = lives.ToString();

        Hashtable props = new Hashtable
        {
            {"Lives", lives}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (lives <= 0 && PhotonNetwork.IsMasterClient)
        {
            view.RPC("DisableGame", RpcTarget.AllBuffered);
            Player lastAlive = FindLastAlivePlayer();
            if (lastAlive != null)
            {
                Hashtable winnerProps = new Hashtable
                {
                    {"Winner", lastAlive.ActorNumber}
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(winnerProps);
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
