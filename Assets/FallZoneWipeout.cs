using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FallZoneWipeout : NetworkBehaviour
{
    public StartWipeout miniGame;
    public bool isTriggered;
    public GameObject eventCamera;

    void Start()
    {
        isTriggered = false;
    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject);
        print(NetworkClient.localPlayer.gameObject);

        if (other.gameObject == NetworkClient.localPlayer.gameObject && isTriggered == false)
        {
            Debug.Log("Client entering collider");
            other.GetComponent<Movement>().camera.SetActive(false);
            eventCamera.SetActive(true);
            miniGame.CmdPlayerDropped(other.gameObject);
            isTriggered = true;
        }
    }
}
