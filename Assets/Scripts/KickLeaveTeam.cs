using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickLeaveTeam : MonoBehaviour
{

    public static KickLeaveTeam instance;

    [SerializeField] GameObject askKickGameObject;

    private void Awake()
    {
        instance = this;
    }

    public void EnableAskKick()
    {
        askKickGameObject.SetActive(true);
    }

    public void DisableAskKick()
    {
        askKickGameObject.SetActive(false);
    }

    public void Kick()
    {
        askKickGameObject.SetActive(false);
        var parentname = TouchPhaseDetector.instance.hitPlayer.name;
        if (parentname == "player (1)")
        {
            TeamEditorHandler.instance.remove_Player(1);
        }
        else if (parentname == "player (2)")
        {
            TeamEditorHandler.instance.remove_Player(2);
        }
        else if (parentname == "player (3)")
        {
            TeamEditorHandler.instance.remove_Player(3);
        }
        else if (parentname == "player (4)")
        {
            TeamEditorHandler.instance.remove_Player(4);
        }
        else if (parentname == "player (5)")
        {
            TeamEditorHandler.instance.remove_Player(5);
        }
        else if (parentname == "player (6)")
        {
            TeamEditorHandler.instance.remove_Player(6);
        }
        else if (parentname == "player (7)")
        {
            TeamEditorHandler.instance.remove_Player(7);
        }
        else if (parentname == "player (8)")
        {
            TeamEditorHandler.instance.remove_Player(8);
        }
        else if (parentname == "player (9)")
        {
            TeamEditorHandler.instance.remove_Player(9);
        }

        gameObject.SetActive(false);
    }

    public void NoKick()
    {
        askKickGameObject.SetActive(false);
    }
}
