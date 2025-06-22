using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartSoccer : NetworkBehaviour
{
    public GameObject teleportLocation;
    private float minDist;
    public bool isMinigameRunning;
    public int alivePlayers;
    public List<GameObject> playersActive;
    public GameObject minigameCamera;
    public bool isSinglePlayer = true;
    public bool isInitialited = false;

    public List<GameObject> teamA;
    public List<GameObject> teamB;

    public int scoreTeamA;
    public int scoreTeamB;

    // Start is called before the first frame update
    void Start()
    {
        minDist = 2.0f;
        playersActive = new List<GameObject>();
        teamA = new List<GameObject>();
        teamB = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float dist = Vector3.Distance(players[0].transform.position, transform.position);
        if (dist < minDist)
        {
            Debug.Log("STARTING GAME SOCCER");
            CmdInitSoccer();
        }

        if (isMinigameRunning)
        {
        }
    }





    [Command(requiresAuthority = false)]
    public void CmdInitSoccer()
    {
        if (isInitialited)
            return;

        playersActive.Clear();
        teamA.Clear();
        teamB.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 1)
            isSinglePlayer = true;
        else
            isSinglePlayer = false;


        if (players.Length % 2 != 0 && isSinglePlayer == false)
        {
            playersActive.AddRange(players);
            playersActive.RemoveAt(playersActive.Count - 1);
        }
        else
        {
            playersActive.AddRange(players);
        }

        foreach (GameObject p in playersActive)
        {
            int team = Random.Range(0, 2);
            Debug.Log("Player: " + p + " Team: " + team);
            //RpcAssignTeam(p, team);

            //assign team A
            if (team == 0 && teamA.Count < playersActive.Count / 2)
            {
                teamA.Add(p);
                p.GetComponent<Movement>().RpcSetNameTagColor(Color.blue);
                //p.GetComponent<Movement>().Teleport(teleportLocation.transform.position);
            }
            //assign team B
            else
            {
                if (teamB.Count < playersActive.Count / 2)
                {
                    teamB.Add(p);
                    p.GetComponent<Movement>().RpcSetNameTagColor(Color.red);
                    //p.GetComponent<Movement>().Teleport(teleportLocation.transform.position);
                }
                else
                {
                    teamA.Add(p);
                    p.GetComponent<Movement>().RpcSetNameTagColor(Color.blue);
                    //p.GetComponent<Movement>().Teleport(teleportLocation.transform.position);
                }
            }
        }

        isInitialited = true;
        RpcInitSoccer(playersActive, teamA, teamB);
    }
    [ClientRpc]
    public void RpcInitSoccer(List<GameObject> _playersAlive, List<GameObject> _teamA, List<GameObject> _teamB)
    {
        playersActive.Clear();
        teamA.Clear();
        teamB.Clear();


        playersActive = _playersAlive;
        teamA = _teamA;
        teamB = _teamB;

        scoreTeamA = 0;
        scoreTeamB = 0;

        GameObject ball = GameObject.FindGameObjectsWithTag("Ball")[0];
        ball.transform.position = teleportLocation.transform.position;

        GameObject lp = NetworkClient.localPlayer.gameObject;
        if (playersActive.Contains(lp))
        {
            lp.GetComponent<Movement>().Teleport(teleportLocation.transform.position);
        }
        
    }







    [Command(requiresAuthority = false)]
    public void CmdAssignTeam(GameObject player)
    {
        int team = Random.Range(0, 2);
        Debug.Log("GENERATED: " + team);
        RpcAssignTeam(player, team);
    }
    [ClientRpc]
    public void RpcAssignTeam(GameObject player, int team)
    {

    }


    [Command(requiresAuthority = false)]
    public void CmdAddScore(bool isTeamA)
    {
        RpcAddScore(isTeamA);
    }
    [ClientRpc]
    public void RpcAddScore(bool isTeamA)
    {
        bool isGood;
        GameObject lp = NetworkClient.localPlayer.gameObject;

        if (isTeamA)
        {
            scoreTeamA++;
            if (teamA.Contains(lp))
                isGood = true;
            else
                isGood = false;
        }
        else
        {
            scoreTeamB++;
            if (teamB.Contains(lp))
                isGood = true;
            else
                isGood = false;
        }

        lp.GetComponent<Movement>().Goal(isGood);

        GameObject ball = GameObject.FindGameObjectsWithTag("Ball")[0];
        ball.transform.position = teleportLocation.transform.position;

        if (scoreTeamA >= 2)
            EndMiniGame(true);

        if (scoreTeamB >= 2)
            EndMiniGame(false);
    }


    public void EndMiniGame(bool isTeamA)
    {
        bool isGood;
        GameObject lp = NetworkClient.localPlayer.gameObject;
        lp.GetComponent<Movement>().E(new Vector3(0.0f, 2.0f, 0.0f));

        if (isTeamA)
        {
            if (teamA.Contains(lp))
                isGood = true;
            else
                isGood = false;
        }
        else
        {
            if (teamB.Contains(lp))
                isGood = true;
            else
                isGood = false;
        }

        isInitialited = false;
        lp.GetComponent<Movement>().WinLose(isGood);
    }
}
