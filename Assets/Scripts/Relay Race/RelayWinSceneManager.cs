using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RelayWinSceneManager : MonoBehaviour
{
    [Header("Relay Win Scene Manager")]
    public Text winTeamText;
    public Text FirstUserNameText;
    public Text FirstTimeText;
    public Text SecondUserNameText;
    public Text SecondTimeText;
    public Text ThirdUserNameText;
    public Text ThirdTimeText;

    public GameObject statsUI;
    public GameObject[] MVP_Ranks;
    public GameObject[] MVP_Players;

    public GameObject[] clones = new GameObject[3];
    private Animator[] animators = new Animator[3];

    [Header("Stats UI")]
    public Text teamName;
    public GameObject[] stat_ranks;
    public Text[] stat_Username_Texts;
    public Text[] stat_time_Texts;

    string winTeam;
    string winTeamName;
    public string[] mvpNames = new string[10];
    public string[] mvpImageURL = new string[10];
    public double[] mvpTimes = new double[10];
    public int[] mvpSkins = new int[10];

    string[] teamNames = new string[10];
    double[] teamTimes = new double[10];
    int[] winCoins = {600, 570, 540, 510, 480, 450, 420, 390, 360, 330};

    bool apiNeedsUpdate = true;
    bool selfExists = true;
    int teamLocalVersion = -1;
    int teamFBVersion = 0;
    int gameLocalVersion = -1;
    int gameFBVersion = 0;

    TeamData teamData;
    InGameData inGameData;

    int memberCount = 0;

    private void Reset_Variables()
    {
        clones = new GameObject[3];
        animators = new Animator[3];
        winTeam = "placeholder";
        winTeamName = "";
        mvpNames = new string[10];
        mvpTimes = new double[10];
        mvpImageURL = new string[10];
        mvpSkins = new int[10];
        teamNames = new string[10];
        teamTimes = new double[10];

        teamLocalVersion = -1;
        teamFBVersion = 0;
        gameLocalVersion = -1;
        gameFBVersion = 0;

        apiNeedsUpdate = true;
        selfExists = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Reset_Variables();
        StartCoroutine(start_Coroutine());
    }

    private IEnumerator start_Coroutine()
    {
        statsUI.SetActive(false);

        for (int i = 0; i < stat_ranks.Length; ++i)
        {
            stat_ranks[i].SetActive(false);
        }

        teamData = API.instance.getTeamData();
        inGameData = API.instance.getInGameData();

        memberCount = teamData.memberCount;

        //FirebaseManager.instance.updateGameResult(inGameData.gameID);
        //yield return new WaitForSeconds(1f);
        winTeam = inGameData.winnerTeamID;

        FirebaseManager.instance.updateWinnerMVPNames(inGameData.gameID, winTeam);
        yield return new WaitForSeconds(1f);
        FirebaseManager.instance.updateTeamNames(inGameData.gameID, teamData.teamCode);
        yield return new WaitForSeconds(1f);

        mvpNames = FirebaseManager.instance.getWinnerMVPName();
        winTeamName = FirebaseManager.instance.getWinTeamName();
        //Debug.Log("Win Scene got Team Name: " + winTeamName);
        mvpTimes = FirebaseManager.instance.getWinnerMVPTimes();
        mvpSkins = FirebaseManager.instance.getWinnerMVPSkinIDs();
        mvpImageURL = FirebaseManager.instance.getWinnerMVPImageURL();

        teamNames = FirebaseManager.instance.getTeamNames();
        teamTimes = FirebaseManager.instance.getTeamTimes();

        //Load data to MVP UI
        if (winTeam != "placeholder")
        {
            winTeamText.text = winTeamName;
            if (mvpNames[0] != "placeholder" && mvpNames[0] != null && mvpTimes[0] < 100)
            {
                //FirstUserNameText.text = mvpNames[0];

                if (mvpTimes[0] > 500)
                {
                    FirstTimeText.text = "Didn't Run";
                }
                else
                {
                    FirstTimeText.text = System.Math.Round(mvpTimes[0], 2).ToString();
                }
                
                MVP_Ranks[0].SetActive(true);
                MVP_Players[0].SetActive(true);

                clones[0] = Instantiate(SkinManager.instance.getSkinByID(mvpSkins[0]), MVP_Players[0].transform.position, Quaternion.identity);
                clones[0].transform.parent = MVP_Players[0].transform;
                clones[0].transform.Rotate(0, 90f, 0);

                clones[0].GetComponent<PlaneTexture>().setImage(mvpImageURL[0]);
                clones[0].GetComponent<PlaneTexture>().setName(mvpNames[0]);

                animators[0] = clones[0].GetComponent<Animator>();
                animators[0].SetInteger("state", 3);


            }
            else
            {
                FirstUserNameText.text = "";
                FirstTimeText.text = "";
                MVP_Ranks[0].SetActive(false);
                MVP_Players[0].SetActive(false);
            }

            if (mvpNames[1] != "placeholder" && mvpNames[1] != null && mvpTimes[1] < 100)
            {
                SecondUserNameText.text = mvpNames[1];

                if (mvpTimes[0] > 500)
                {
                    SecondTimeText.text = "Didn't Run";
                }
                else
                {
                    SecondTimeText.text = System.Math.Round(mvpTimes[1], 2).ToString();
                }
                
                MVP_Ranks[1].SetActive(true);
                MVP_Players[1].SetActive(true);

                clones[1] = Instantiate(SkinManager.instance.getSkinByID(mvpSkins[1]), MVP_Players[1].transform.position, Quaternion.identity);
                clones[1].transform.parent = MVP_Players[1].transform;
                clones[1].transform.Rotate(0, 90f, 0);

                clones[1].GetComponent<PlaneTexture>().setImage(mvpImageURL[1]);
                clones[1].GetComponent<PlaneTexture>().setName(mvpNames[1]);

                animators[1] = clones[1].GetComponent<Animator>();
                animators[1].SetInteger("state", 3);


            }
            else
            {
                SecondUserNameText.text = "";
                SecondTimeText.text = "";
                MVP_Ranks[1].SetActive(false);
                MVP_Players[1].SetActive(false);
            }

            if (mvpNames[2] != "placeholder" && mvpNames[2] != null && mvpTimes[2] < 100)
            {
                ThirdUserNameText.text = mvpNames[2];

                if (mvpTimes[0] > 500)
                {
                    ThirdTimeText.text = "Didn't Run";
                }
                else
                {
                    ThirdTimeText.text = System.Math.Round(mvpTimes[2], 2).ToString();
                }

                
                MVP_Ranks[2].SetActive(true);
                MVP_Players[2].SetActive(true);

                clones[2] = Instantiate(SkinManager.instance.getSkinByID(mvpSkins[2]), MVP_Players[2].transform.position, Quaternion.identity);
                clones[2].transform.parent = MVP_Players[2].transform;
                clones[2].transform.Rotate(0, 90f, 0);

                clones[2].GetComponent<PlaneTexture>().setImage(mvpImageURL[2]);
                clones[2].GetComponent<PlaneTexture>().setName(mvpNames[2]);

                animators[2] = clones[2].GetComponent<Animator>();
                animators[2].SetInteger("state", 3);


            }
            else
            {
                ThirdUserNameText.text = "";
                ThirdTimeText.text = "";
                MVP_Ranks[2].SetActive(false);
                MVP_Players[2].SetActive(false);
            }



            //Load data to Team Stats UI
            teamName.text = teamData.teamname;

            int index = 0;
            for (int i = 0; i < teamNames.Length; ++i)
            {
                if (teamNames[i] != "placeholder" && teamNames[i] != "" && teamNames[i] != null && teamTimes[i] <= 500 && teamTimes[i] > 0)
                {
                    stat_Username_Texts[index].text = teamNames[i];

                    stat_time_Texts[index].text = System.Math.Round(teamTimes[i], 2).ToString() + " (s)";
                    stat_ranks[index].SetActive(true);
                    index++;
                }
                else
                {
                    stat_ranks[i].SetActive(false);
                }

            }

            LevelLoader.instance.ClearCrossFade();

            if (teamName.text == winTeamName)
            {
                string user_name = PlayerManager.instance.getData("username");
                for (int i = 0; i < mvpNames.Length; i++)
                {
                    if (mvpNames[i].ToString() == user_name)
                    {
                        PlayerManager.instance.addCoins(winCoins[i].ToString());
                    }
                }
            }

            PlayerManager.instance.setGameStatus("Custom Relay Game", "In Lobby");
            FirebaseManager.instance.clearGameData(teamData.teamCode, inGameData.gameID);
            inGameData = API.instance.resetInGameData();
        }
        else
        {
            PlayerManager.instance.setGameStatus("Custom Relay Game", "In Lobby");
            FirebaseManager.instance.clearGameData(teamData.teamCode, inGameData.gameID);
            inGameData = API.instance.resetInGameData();
            LevelLoader.instance.loadScene("Team Lobby");
        }
        

        
    }

    public void open_stat_UI()
    {
        statsUI.SetActive(true);
    }

    public void close_Button()
    {
        statsUI.SetActive(false);
    }
    public void JumpToTeamLobby()
    {
        PlayerManager.instance.setGameStatus("Custom Relay Game", "In Lobby");
        FirebaseManager.instance.clearGameData(teamData.teamCode, inGameData.gameID);
        inGameData = API.instance.resetInGameData();
        LevelLoader.instance.loadScene("Team Lobby");
    }

    private void Update()
    {
        if (apiNeedsUpdate)
        {
            StartCoroutine(update_API());
        }
    }
    private IEnumerator update_API()
    {
        apiNeedsUpdate = false;
        yield return new WaitForEndOfFrame();

        FirebaseManager.instance.updateTeamVersion(teamData.teamCode);
        if (teamData.gameID != "placeholder")
        {
            FirebaseManager.instance.updateGameVersion(inGameData.gameID);
        }

        yield return new WaitForSeconds(0.5f);
        teamFBVersion = FirebaseManager.instance.getTeamVersion();

        if (teamData.gameID != "placeholder")
        {
            gameFBVersion = FirebaseManager.instance.getGameVersion();
        }

        if (teamFBVersion != teamLocalVersion)
        {
            API.instance.Update_Team_Data(teamData.teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;


            if (_statusCode == 200)
            {
                teamData = API.instance.getTeamData();
                //API.instance.logTeamData();
            }
            else
            {
                selfExists = false;
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }

            teamLocalVersion = teamFBVersion;
        }

        if (teamData.gameID != "placeholder")
        {

            API.instance.Update_InGame_Data(teamData.teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;
            Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");

            inGameData = API.instance.getInGameData();

            API.instance.Update_Queue_Data(inGameData.gameID);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;
            Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");

            gameLocalVersion = gameFBVersion;
        }

        if (!selfExists)
        {
            selfExists = true;
            PlayerManager.instance.setGameStatus("placeholder", "placeholder");
            LevelLoader.instance.loadScene("Lobby");
        }

        if (inGameData.gameStart && !inGameData.gameEnd)
        {
            LevelLoader.instance.loadScene("Relay Game");
        }

        apiNeedsUpdate = true;
    }

    private void OnApplicationPause(bool pause)
    {
        /*
        if (pause)
        {
            PlayerManager.instance.setIsOnline("False");
            API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), false, false);
        }
        else
        {
            PlayerManager.instance.setIsOnline("True");
            API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), true, false);
        }
        */
        Debug.Log("Application Pause Called in TeamLobby");
        TeamLobbyManager.instance.leave_Button();

    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit Called in TeamLobby");
        API.instance.Leave_Team(teamData.teamCode, PlayerManager.instance.getData("uid"));
    }
}
