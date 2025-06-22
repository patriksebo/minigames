using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetInteraction : NetworkBehaviour
{
    public StartSoccer sc;
    public bool isTeamA;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            Debug.Log("ball in net");
            sc.CmdAddScore(isTeamA);
        }
    }
}
