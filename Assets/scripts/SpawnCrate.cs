using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class SpawnCrate : MonoBehaviour
{
    public Item[] items;
    public bool gameEnd = false;
    public GameObject crate;
    GameObject newCrate;
    int delay;
    CrateFunctionality functionality;
    public bool canSpawn = true;
    [SerializeField] float lifeSpan;
    public PhotonView view;


    private void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }

        if (canSpawn && !GameManager.Instance.gameOver)
        {
            canSpawn = false;
            StartCoroutine("crateSpawnTimer");
        }
    }

    public IEnumerator crateSpawnTimer()
    {
        delay = Random.Range(20, 40);
        yield return new WaitForSeconds(delay);
        Spawn();
    }

    public void Spawn()
    {
        if (newCrate == null)
        {
            newCrate = PhotonNetwork.Instantiate(crate.name, GenerateRandomVector2(-15, 28, -7, 10), Quaternion.identity);
            functionality = newCrate.GetComponent<CrateFunctionality>();
            functionality.spawner = this.gameObject.GetComponent<SpawnCrate>();
            StartCoroutine(crateLifeTimer());
        }
        
    }

    private IEnumerator crateLifeTimer()
    {
        yield return new WaitForSeconds(lifeSpan);
        PhotonNetwork.Destroy(newCrate);
        newCrate = null;
        canSpawn = true;
    }



    public static Vector2 GenerateRandomVector2(float minX, float maxX, float minY, float maxY)
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }

    [PunRPC]
    public void SetSpawnFlag(bool flag)
    {
        canSpawn = flag;
    }
}
