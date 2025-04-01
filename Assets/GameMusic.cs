using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMusic : MonoBehaviour
{
    AudioSource musicSource;

    private void Start()
    {
        musicSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (ESCMenuListener.isPaused || IsLoading() || GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            return;
        }

        if (musicSource != null && musicSource.enabled == false) 
        {
            musicSource.enabled = true;
        }
    }
    // Start is called before the first frame update
    public bool IsLoading()
    {
        // Check if the room has the "IsLoading" property
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsLoading", out object isLoadingValue))
        {
            // Return the value cast as a boolean
            return (bool)isLoadingValue;
        }

        // Default to false if the property is not set
        return false;
    }
}
