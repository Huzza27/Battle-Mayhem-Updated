using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class SpawnCrate : MonoBehaviour
{
    [SerializeField] private GameObject cratePrefab;
    [SerializeField] private float maxSpawnTime;
    [SerializeField] private float minSpawnTime;
    [Header("Spawning Range")]
    [SerializeField] private float maxX = 28 ; //Default of 28
    [SerializeField] private float maxY = 10; //Default of 10
    [SerializeField] private float minX = -15; //Default of -15
    [SerializeField] private float minY = -7; //Default of -7

    public void StartCrateSpawningCourotine()
    { 
        StartCoroutine(CrateSpawnCourotine());
    }

    private IEnumerator CrateSpawnCourotine()
    {
        float randomSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
        yield return new WaitForSeconds(randomSpawnTime);
        SpawnNewCrate();
    }

    public void SpawnNewCrate()
    {
        GameObject newCrate = PhotonNetwork.Instantiate(cratePrefab.name, GenerateRandomVector2(minX, maxX, minY, maxY), Quaternion.identity);
        SubscribeManagerToDestroyCrateEvent(newCrate.GetComponent<CrateFunctionality>());

    }

    public void SubscribeManagerToDestroyCrateEvent(CrateFunctionality crate)
    {
        crate.OnCrateDestroyed += StartCrateSpawningCourotine;
    }

    public static Vector3 GenerateRandomVector2(float minX, float maxX, float minY, float maxY)
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector3(x, y, 12);
    }













    /*public Item[] items;
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

        if (canSpawn && !(GameManager.Instance.CurrentState == GameManager.GameState.GameOver))
        {
            canSpawn = false;
            StartCoroutine("crateSpawnTimer");
        }
    }

    public IEnumerator crateSpawnTimer()
    {
        delay = Random.Range(10, 15);
        yield return new WaitForSeconds(delay);
        Spawn();
    }

    public void Spawn()
    {
        PhotonNetwork.Destroy(newCrate);

            newCrate = PhotonNetwork.Instantiate(crate.name, GenerateRandomVector2(-15, 28, -7, 10), Quaternion.identity);
            functionality = newCrate.GetComponent<CrateFunctionality>();
            functionality.spawner = this.gameObject.GetComponent<SpawnCrate>();
            StartCoroutine(crateLifeTimer()); 
    }

    private IEnumerator crateLifeTimer()
    {
        yield return new WaitForSeconds(lifeSpan);
        PhotonNetwork.Destroy(newCrate);
        newCrate = null;
        canSpawn = true;
    }



    

    [PunRPC]
    public void SetSpawnFlag(bool flag)
    {
        canSpawn = flag;
    }
    */
}
