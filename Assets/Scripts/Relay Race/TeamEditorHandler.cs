using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamEditorHandler : MonoBehaviour
{
    public static TeamEditorHandler instance;

    [Header("Team Editor")]
    [SerializeField] GameObject[] members;
    //[SerializeField] Text[] usernames;

    private bool needsUpdate;

    TeamData teamData;
    private string[] usernamesByMemberIDs;
    private string[] memberIDs;
    private string teamCode;
    private bool leave_team_lock;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        needsUpdate = true;
        leave_team_lock = false;
        usernamesByMemberIDs = new string[10];
        //reset_UI();
    }


    void Update()
    {
        StartCoroutine(update_coroutine());
    }

    private IEnumerator update_coroutine()
    {
        needsUpdate = false;
        yield return new WaitForEndOfFrame();

        teamData = API.instance.getTeamData();
        usernamesByMemberIDs = teamData.nameList;
        memberIDs = teamData.memberIDs;
        teamCode = teamData.teamCode;

        //display();

        needsUpdate = true;
    }

    //private void display()
    //{
    //    for (int i = 0; i < usernamesByMemberIDs.Length; ++i)
    //    {
    //        //Debug.Log("usernamesByMemberIDs[" + i + "]: " + usernamesByMemberIDs[i]);
    //        if (usernamesByMemberIDs[i] != "placeholder" && usernamesByMemberIDs[i] != null)
    //        {
    //            usernames[i].text = usernamesByMemberIDs[i];
    //            members[i].SetActive(true);
    //        }
    //        else
    //        {
    //            usernames[i].text = "placeholder";
    //            members[i].SetActive(false);
    //        }
    //    }
    //}

    //private void reset_UI()
    //{
    //    for (int i = 0; i < members.Length; ++i)
    //    {
    //        members[i].SetActive(false);
    //        usernames[i].text = "placeholder";
    //    }

    //}

    //----------------------------------------------------Buttons-----------------------------------------------
    public void close_UI()
    {
        gameObject.SetActive(false);
    }

    public void open_UI()
    {
        gameObject.SetActive(true);
    }

    public void remove_2()
    {
        remove_Player(1);
    }

    public void remove_3()
    {
        remove_Player(2);
    }

    public void remove_4()
    {
        remove_Player(3);
    }

    public void remove_5()
    {
        remove_Player(4);
    }

    public void remove_6()
    {
        remove_Player(5);
    }

    public void remove_7()
    {
        remove_Player(6);
    }

    public void remove_8()
    {
        remove_Player(7);
    }

    public void remove_9()
    {
        remove_Player(8);
    }

    public void remove_10()
    {
        remove_Player(9);
    }

    public void remove_Player(int _memberID)
    {

        StartCoroutine(remove_Player_Coroutine(_memberID));
    }

    private IEnumerator unlock()
    {
        yield return new WaitForSecondsRealtime(8f);
        leave_team_lock = false;
    }
    private IEnumerator remove_Player_Coroutine(int _memberID)
    {
        if (!leave_team_lock)
        {
            leave_team_lock = true;

            string _UID = memberIDs[_memberID];

            API.instance.Leave_Team(teamCode, _UID);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                Debug.Log("Removed player(" + _UID + ")");
            }
            else
            {
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }

            StartCoroutine(unlock());
        }
    }
}
