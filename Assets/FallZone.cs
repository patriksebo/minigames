using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FallZone : NetworkBehaviour
{
    public StartMiniGame miniGame;
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
            //miniGame.CmdPlayerDropped();
            //other.GetComponent<Movement>().E();
            //miniGame.playerCamera = other.GetComponent<Movement>().camera;
            //miniGame.CmdHandleCamera();
            other.GetComponent<Movement>().camera.SetActive(false);
            eventCamera.SetActive(true);

            miniGame.CmdPlayerDropped(other.gameObject);
            isTriggered = true;
        }
    }
}
