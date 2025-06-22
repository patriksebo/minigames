using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartMiniGame : NetworkBehaviour
{
    public GameObject teleportLocation;
    private float minDist;
    public bool isMinigameRunning;
    public GameObject[] planes;
    public List<GameObject> planesActive;
    public List<GameObject> playersActive;

    private bool isInitialized = false;
    private bool isInvoked = false;
    private int counter = 0;
    public bool isSinglePlayer = true;

    public int alivePlayers;
    public int alivePlatforms;
    public GameObject fallZone;
    public GameObject minigameCamera;
    public GameObject playerCamera;

    [Server]
    void Start()
    {
        minDist = 2.0f;
        planesActive = new List<GameObject>();
        playersActive = new List<GameObject>();
    }

    [Client]
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float dist = Vector3.Distance(players[0].transform.position, transform.position);
        if (dist < minDist)
        {
            Debug.Log("STARTING GAME");
            CmdInitFallPlatform();
            CmdTeleport();
            isMinigameRunning = true;
        }

        //mini game started
        if(isMinigameRunning)
        {
            if(isInvoked == false)
            {
                Invoke("CmdFallPlatform", 5);
            }
            isInvoked = true;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdTeleport()
    {
        RpcTeleport();
    }

    [ClientRpc]
    private void RpcTeleport()
    {
        Debug.Log("message from server!");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        GameObject lp = NetworkClient.localPlayer.gameObject;
        //lp.transform.position = new Vector3(teleportLocation.transform.position.x, teleportLocation.transform.position.y, teleportLocation.transform.position.z);
        lp.GetComponent<Movement>().E(new Vector3(teleportLocation.transform.position.x, teleportLocation.transform.position.y, teleportLocation.transform.position.z));

        /*
        foreach (GameObject p in players)
        {
            p.transform.position = new Vector3(teleportLocation.transform.position.x, teleportLocation.transform.position.y, teleportLocation.transform.position.z);
        }
        */
    }

    [Command(requiresAuthority = false)]
    private void CmdInitFallPlatform()
    {
        Debug.Log("CmdInitFallPlatform");
        RpcInitFallPlatform();
    }
    [ClientRpc]
    private void RpcInitFallPlatform()
    {
        foreach (GameObject plane in planes)
        {
            plane.SetActive(true);
        }

        alivePlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        if (alivePlayers == 1)
        {
            isSinglePlayer = true;
        }
        else
        {
            isSinglePlayer = false;
        }

        planesActive.Clear();
        planesActive.AddRange(planes);

        playersActive.Clear();
        playersActive.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        alivePlatforms = planesActive.Count;
    }

    [Command(requiresAuthority = false)]
    private void CmdFallPlatform()
    {
        int index = Random.Range(0, planesActive.Count);
        RpcFallPlatform(index);
    }
    [ClientRpc]
    private void RpcFallPlatform(int index)
    {
        isInvoked = false;

        if (alivePlatforms <= 1)
        {
            isMinigameRunning = false;
            //RpcPlayerDropped(); ??
            EndMiniGame();
            return;
        }

        planesActive[index].SetActive(false);
        planesActive.RemoveAt(index);
        alivePlatforms = planesActive.Count;
    }

    [Client]
    public void HandleCamera()
    {
        playerCamera.SetActive(false);
        minigameCamera.SetActive(true);
    }
    [Command(requiresAuthority = false)]
    public void CmdHandleCamera()
    {
        RpcHandleCamera();
    }
    [ClientRpc]
    public void RpcHandleCamera()
    {
        playerCamera.SetActive(false);
        minigameCamera.SetActive(true);
    }


    [Command(requiresAuthority = false)]
    public void CmdPlayerDropped(GameObject go)
    {
        //go?
        RpcPlayerDropped(go);
    }
    // GAME ENDED
    [ClientRpc]
    public void RpcPlayerDropped(GameObject go)
    {
        alivePlayers--;
        playersActive.Remove(go);

        if (alivePlayers <= 1 || (alivePlayers <= 0 && isSinglePlayer))
        {
            EndMiniGame();
        }
    }

    public void EndMiniGame()
    {
        GameObject lp = NetworkClient.localPlayer.gameObject;
        lp.GetComponent<Movement>().camera.SetActive(true);
        minigameCamera.SetActive(false);
        lp.GetComponent<Movement>().E(new Vector3(0.0f, 2.0f, 0.0f));
        fallZone.GetComponent<FallZone>().isTriggered = false;

        if (playersActive.Contains(lp))
        {
            lp.GetComponent<Movement>().WinLose(true);
        }
        else
        {
            lp.GetComponent<Movement>().WinLose(false);
        }

        isMinigameRunning = false;
    }
}