using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBRetrieval : MonoBehaviour
{
    public static DBRetrieval instance;
    public CosmeticDatabase CosmeticDatabase;                           

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            DontDestroyOnLoad(instance);
        }
    }
}
