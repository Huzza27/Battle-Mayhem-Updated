using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunAOE : MonoBehaviour
{
    public float tickInterval = 1f; // Time between damage ticks
    public float tickDamage = 5f; // Damage per tick
    public float duration = 7f; // Duration before the AOE disappears
    public PhotonView shooterView;

    private HashSet<PhotonView> affectedPlayers = new HashSet<PhotonView>();
    private Dictionary<PhotonView, Coroutine> activeCoroutines = new Dictionary<PhotonView, Coroutine>();
    [SerializeField] GameObject AOE;

    private void Start()
    {
        // Start the destroy timer
        StartCoroutine(DestroyAfterDuration());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView targetView = collision.GetComponent<PhotonView>();
            if (targetView != null && !affectedPlayers.Contains(targetView) && targetView != shooterView)
            {
                affectedPlayers.Add(targetView); // Add player to list of affected players

                SlowPlayer(targetView.gameObject); // Slow target down

                // Start and store the coroutine for this player
                Coroutine tickCoroutine = StartCoroutine(ApplyTickDamage(targetView));
                activeCoroutines[targetView] = tickCoroutine;
            }
        }
    }

    void SlowPlayer(GameObject player)
    {
        player.GetComponent<Movement>().SlowPlayer(2.5f); // Use helper method in movement script
    }

    void ResetMovement(GameObject player)
    {
        player.GetComponent<Movement>().ResetSpeed(); // Use helper method in movement script
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView targetView = collision.GetComponent<PhotonView>();
            if (targetView != null && affectedPlayers.Contains(targetView))
            {
                ResetMovement(targetView.gameObject);
                // Remove the player from the affected set
                affectedPlayers.Remove(targetView);

                // Stop the associated coroutine if it exists
                if (activeCoroutines.ContainsKey(targetView))
                {
                    StopCoroutine(activeCoroutines[targetView]);
                    activeCoroutines.Remove(targetView);
                }
            }
        }
    }

    private IEnumerator ApplyTickDamage(PhotonView targetView)
    {
        while (affectedPlayers.Contains(targetView))
        {
            if (targetView.IsMine)
            {
                // Apply damage only if the player owns this view
                targetView.RPC("ReduceHealth", RpcTarget.All, tickDamage);
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }

    private IEnumerator DestroyAfterDuration()
    {
        // Wait for the AOE to exist for the specified duration
        yield return new WaitForSeconds(duration);

        // Destroy the AOE object on the network
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(AOE);
        }
    }
}

