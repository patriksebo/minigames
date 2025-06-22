using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartWipeout : NetworkBehaviour
{
    public GameObject teleportLocation;
    private float minDist;
    public bool isMinigameRunning;
    public int alivePlayers;

    public List<GameObject> playersActive;
    public bool isSinglePlayer = true;
    public GameObject fallZone;
    public GameObject minigameCamera;
    public GameObject playerCamera;

    public float speed;
    public GameObject wipeout;
    public bool isInvoked = false;

    public GameObject[] playerStands;


    void Start()
    {
        minDist = 2.0f;
        playersActive = new List<GameObject>();
    }
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float dist = Vector3.Distance(players[0].transform.position, transform.position);
        if (dist < minDist)
        {
            Debug.Log("STARTING GAME WIPEOUT");
            CmdInitWipeout();
            //CmdTeleport();
        }

        if (isMinigameRunning)
        {
            wipeout.transform.Rotate(0, speed * Time.deltaTime, 0);

            if (isInvoked == false)
            {
                Invoke("IncreaseSpeed", 5);
                isInvoked = true;
            }
        }
    }


    [Command(requiresAuthority = false)]
    public void CmdInitWipeout()
    {
        RpcInitWipeout();
    }
    [ClientRpc]
    public void RpcInitWipeout()
    {
        playersActive.Clear();
        int counter = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (counter < playerStands.Length)
            {
                playersActive.Add(p);

                if (p == NetworkClient.localPlayer.gameObject)
                {
                    p.GetComponent<Movement>().Teleport(new Vector3(playerStands[counter].transform.position.x, playerStands[counter].transform.position.y + 10.0f, playerStands[counter].transform.position.z));
                    //p.transform.name = counter.ToString();
                    //p.transform.position = new Vector3(playerStands[counter].transform.position.x, playerStands[counter].transform.position.y + 10.0f, playerStands[counter].transform.position.z);
                }
                //p.GetComponent<Movement>().E(new Vector3(playerStands[counter].transform.position.x, playerStands[counter].transform.position.y + 10.0f, playerStands[counter].transform.position.z));
                counter++;
            }
            else
            {
                if (p == NetworkClient.localPlayer.gameObject)
                {
                    p.GetComponent<Movement>().camera.SetActive(false);
                    minigameCamera.SetActive(true);
                }
            }
        }

        
        //playersActive.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        alivePlayers = playersActive.Count;
        speed = 10.0f;
        isMinigameRunning = true;
    }


    [Command(requiresAuthority = false)]
    public void CmdTeleport()
    {
        RpcTeleport();
    }
    [ClientRpc]
    private void RpcTeleport()
    {
        GameObject lp = NetworkClient.localPlayer.gameObject;
        lp.GetComponent<Movement>().E(new Vector3(teleportLocation.transform.position.x, teleportLocation.transform.position.y, teleportLocation.transform.position.z));
    }

    public void IncreaseSpeed()
    {
        speed = speed + 5;
        isInvoked = false;
    }



    [Command(requiresAuthority = false)]
    public void CmdPlayerDropped(GameObject go)
    {
        RpcPlayerDropped(go);
    }
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
        Debug.Log("END");
        GameObject lp = NetworkClient.localPlayer.gameObject;
        lp.GetComponent<Movement>().camera.SetActive(true);
        minigameCamera.SetActive(false);
        lp.GetComponent<Movement>().E(new Vector3(0.0f, 2.0f, 0.0f));
        fallZone.GetComponent<FallZoneWipeout>().isTriggered = false;

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
