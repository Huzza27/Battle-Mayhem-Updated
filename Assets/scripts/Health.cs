using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System.Collections;
using System.Xml.Serialization;
using ExitGames.Client.Photon;
using System.Linq;
using System.Collections.Generic;

public class Health : MonoBehaviour
{
    private const byte EndGameEventCode = 1; // A unique byte value for your custom event
    private int numEliminatedPlayers = 0;
    public bool canRespawn = true;
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

    public int lives;
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



    private void Awake()
    {
        ResetHealthState();
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += ResetHealthState;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetHealthState;
    }

    private void ResetHealthState()
    {
        // Reset core gameplay variables
        isDead = false;
        canRespawn = true;
        healthAmount = 100f; // Assuming full health is 100

        // Reset view and components
        view = GetComponent<PhotonView>();

        // Reset lives to initial value
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartingLives", out object startingLives))
            {
                lives = (int)startingLives;
            }
            else
            {
                // Fallback if the property is missing
                lives = 10;
            }
        }

        // Reset UI elements
        if (fillImage != null)
        {
            fillImage.fillAmount = healthAmount / 100;
        }

        // Reset sprite visibility
        if (sprites != null)
        {
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    Color resetColor = sprite.color;
                    resetColor.a = 1f;
                    sprite.color = resetColor;
                }
            }
        }

        // Reset dash UI
        if (dashBar != null)
        {
            dashBar.color = Color.white;
        }
        if (coolDownText != null)
        {
            coolDownText.text = null;
        }

        // Reinitialize respawn area
        respawnArea = GameObject.FindGameObjectWithTag("RespawnArea").transform;

        // Reset colliders if needed
        if (bc != null) bc.enabled = true;
        if (bc2 != null) bc2.enabled = true;

        // If this is the master client, reset the lives display
        if (PhotonNetwork.IsMasterClient)
        {
            view.RPC("SetUpLivesDisplay", RpcTarget.All);
        }
    }

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

        Debug.Log("view.IsMine = " + view.IsMine);
        Debug.Log("healthAmount = " + healthAmount);
        Debug.Log("Is dead = " + isDead);
        // Only the owner will trigger the death
        if (view.IsMine && healthAmount <= 0 && !isDead)
        {
            Debug.Log("Correct view for health check!");
            Debug.Log("Health is below zero at " + healthAmount);
            isDead = true;
            Debug.Log("Player is now dead");
            healthAmount = 0;
            fillImage.fillAmount = 0;
            Debug.Log("Calling death method");
            view.RPC("Death", RpcTarget.All, view.ViewID);
        }
    }
    public bool playerWillDieAfterHit()
    {
        // Check if the player will die after the next hit
        return healthAmount <= 0;
    }

    [PunRPC]
    public void ReduceHealth(float amount, int shooterActorNumber, bool isBomb)

    {
        Debug.Log("Reducing health by " + amount);
        // Reduce the health and make sure it does not go below zero
        healthAmount = Mathf.Max(healthAmount - amount, 0);
        if (PhotonNetwork.IsMasterClient)
        {
            CheckForKillUpdate(shooterActorNumber, isBomb);
        }

        Debug.Log("Health = " + healthAmount);
        // Broadcast the health update to all clients including self
        view.RPC("UpdateHealthUI", RpcTarget.Others, view.ViewID, (float)healthAmount);
    }

    public void CheckForKillUpdate(int shooterActorNumber, bool isBomb)
    {
        if(playerWillDieAfterHit())
        {
            MatchStatsManager.Instance.RecordKill(shooterActorNumber.ToString());
            if (isBomb)
            {
                MatchStatsManager.Instance.RecordBombKill(shooterActorNumber.ToString());
            }
        }
    }


    [PunRPC]
    public void UpdateHealthUI(int targetViewID, float healthAmount)
    {
        Debug.Log("Made it to UpdateHealthUI");
        if (view.ViewID == targetViewID)
        {
            Debug.Log("Correct view");

            Debug.Log($"Updating health for PlayerViewID: {targetViewID} with health amount: {healthAmount}");
            this.healthAmount = healthAmount; // Set the player's health amount to the new value
            view.RPC("HealthBarAnimation", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void HealthBarAnimation()
    {
        LeanTween.value(fillImage.gameObject, fillImage.fillAmount, healthAmount / 100, healthBarAninDuraition)
                .setOnUpdate((float value) =>
                {
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
    public void AssignHealthBar(int playerViewID, int playerActorNumber)
    {

        object colorChoice;
        PhotonView targetPhotonView = PhotonView.Find(playerViewID);
        spawnManager = GameObject.FindGameObjectWithTag("SpawnPlayer").GetComponent<SpawnPlayers>();

        if (targetPhotonView != null)
        {
            player = targetPhotonView.gameObject;

            // Activate the health bar for the player
            playerHealthBar = spawnManager.healthBarList[playerActorNumber - 1].GetComponent<HealthBar>();
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
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            Health targetHealth = targetView.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.lives--;

                if (PhotonNetwork.IsMasterClient)
                {
                    targetHealth.playerHealthBar.GetLivesDisplayView().RPC("SetLives", RpcTarget.All, targetHealth.lives);
                }

                if (targetHealth.lives <= 0)
                {
                    targetHealth.canRespawn = false;
                    view.RPC("RequestRemovePlayerFromRoomList", RpcTarget.MasterClient, targetView.Owner.ActorNumber);
                }
            }
        }
    }

}