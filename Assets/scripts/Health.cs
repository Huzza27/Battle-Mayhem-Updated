using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System.Collections;

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

    public TMP_Text livesDisplay;
    int lives;
    public CameraMove camera;

    public SpriteRenderer[] sprites;

    public bool isDead;

    private void Start()
    {
        isDead = false;
        view = GetComponent<PhotonView>();
        respawnArea = GameObject.FindGameObjectWithTag("RespawnArea").transform;
        lives = (int)PhotonNetwork.CurrentRoom.CustomProperties["StartingLives"];
        livesDisplay.text = lives.ToString();
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
        if (view.IsMine && healthAmount <= 0)
        {
            this.GetComponent<PhotonView>().RPC("Dead", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void AssignHealthBar(int playerViewID, int playerCount)
    {
        PhotonView targetPhotonView = PhotonView.Find(playerViewID);
        SpawnPlayers spawnManager = GameObject.FindGameObjectWithTag("SpawnPlayer").GetComponent<SpawnPlayers>();
        if (targetPhotonView != null)
        {
            player = targetPhotonView.gameObject;
            spawnManager.healthBarList[playerCount - 1].SetActive(true);
            player.GetComponent<Health>().fillImage = spawnManager.healthBarList[playerCount - 1].transform.GetChild(1).GetComponent<Image>();
            icon = spawnManager.healthBarList[playerCount - 1].transform.GetChild(2).GetComponent<Image>();

            object colorChoice;
            if (targetPhotonView.Owner.CustomProperties.TryGetValue("PlayerColor", out colorChoice))
            {
                int colorIndex = (int)colorChoice;
                targetPhotonView.RPC("SetUIColor", RpcTarget.All, colorIndex);
            }
            livesDisplay = spawnManager.healthBarList[playerCount - 1].transform.GetChild(3).GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogError("Player GameObject not found for the given PhotonView ID.");
        }
    }

    [PunRPC]
    private void Dead()
    {
        if (!isDead)
        {
            isDead = true;
            healthAmount = 0;

            // Reset physics
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            camera.enabled = false;

            // Synchronize the respawn routine across all clients
            view.RPC("StartRespawnRoutine", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void StartRespawnRoutine()
    {
        // Start the fade-out, shake, and respawn process across all clients
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // Trigger fade-out and other visual effects
        sr_body.color = new Color(1f, 1f, 1f, 0.5f);
        gun.enabled = false;
        rb.isKinematic = true;
        bc.enabled = false;
        bc2.enabled = false;

        // Shake the camera for some visual impact (sync with all clients)
        yield return StartCoroutine(camera.Shake(duration, magnitude));

        yield return new WaitForSeconds(3); // Wait for respawn time

        // Call the respawn sync function via RPC
        view.RPC("RespawnPlayer", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RespawnPlayer()
    {
        // Move the player to the respawn area and reset state across all clients
        transform.position = respawnArea.position;
        StartCoroutine(FadeInSprites());
        ResetPlayerState();
    }

    void ResetPlayerState()
    {
        isDead = false;
        healthAmount = 100;
        camera.enabled = true;
        sr_body.color = Color.white;
        gun.enabled = true;
        rb.isKinematic = false;
        bc.enabled = true;
        bc2.enabled = true;

        fillImage.fillAmount = 1;

        // Sync the life counter update across all clients
        view.RPC("UpdateLifeCounterOnAllClients", RpcTarget.AllBuffered);
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
