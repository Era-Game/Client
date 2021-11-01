using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamLobbyPlayer : MonoBehaviour
{

    [SerializeField] GameObject kickUI;

    public void EnableKickUI(Vector2 touchPos)
    {
        kickUI.SetActive(true);
        kickUI.transform.position = touchPos;
    }

    public void DisableKickUI()
    {
        kickUI.SetActive(false);
        KickLeaveTeam.instance.DisableAskKick();
    }
}
