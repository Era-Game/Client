using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 

public class TeamLobbyManager : MonoBehaviour
{
    public static TeamLobbyManager instance;

    [Header("Team Lobby Manager")]
    public GameObject teamLeaderUtils;
    public GameObject loadingUI;
    public GameObject queueUI_Leader;
    public GameObject queueUI_Member;
    public Text queueUI_Leader_Text;
    public Text queueUI_Member_Text;
    public GameObject normalGameUI;
    public Text memberCountText;
    public Text teamCodeText;
    public Text teamNameText;
    public GameObject avatar;
    public GameObject listOfGames;
    public GameObject joinGameButton;
    public GameObject TimerLeader;
    public GameObject TimerMember;

    [Header("Custom Games")]
    public GameObject customGameUI;
    public GameObject gameSearchUI;
    public GameObject createGameUI;
    public GameObject customGameQueueUI;
    public InputField idSearchInput;

    [Header("Admin Utils")]
    public InputField numberOfTeams;

    [Header("Queue UI")]
    public TMP_Text gameTitleText; 
    public GameObject queueUI;
    public GameObject teamLeaderQueueUtils;
    public GameObject gameLeaderUtils; 
    public GameObject[] queueTeams;
    public Text[] queueTeamNameTexts;
    public string[] queueTeamNames = new string[10];
    public string[] localQueueTeamNames = new string[10];

    //Private Member Variables
    private string teamCode;
    private int memberCount;
    private int memberID;
    public int teamLocalVersion = -1;
    public int teamFBVersion = 0;
    public int gameLocalVersion = -1;
    public int gameFBVersion = 0;
    private string[] memberIDs;
    private bool needsUpdate;
    private bool needsQueueUpdate;
    private bool avaliableGameNeedsUpdate;
    private bool apiNeedsUpdate;
    private bool queueNeedsUpdate; 
    private bool startQueue = false;
    private bool quit;
    private string gameID;
    private string teamName;
    private bool selfExists;
    private bool gameLeader;
    private int selfExistWarningCount = 0;

    private bool leave_Team_Funtion_Lock;

    TeamData teamData;
    InGameData inGameData;

    //Coroutine Timers
    public float updateCoroutineTimer = 0;
    public float updateAPITimer = 0;
    public float updateAvaliableGameTimer = 0;

    //Queue Private Variables
    private bool queueInit;
    private int startSeconds;
    private int startMinutes;
    private int currSeconds;
    private int currMinutes;
    public List<string> gameIDs = new List<string>();
    public List<string> localGameIDs = new List<string>();
    public ArrayList teamAmount = new ArrayList();
    public List<GameObject> tempGameObject = new List<GameObject>();
    private Timer timerLeader;
    private Timer timerMember;

    private void Awake()
    {
        instance = this;
        StartCoroutine(buffer_init_period());
    }
    private void Start()
    {
        InvokeRepeating("API_Periodic_Sync", 3.0f, 5.0f);

        loadingUI.SetActive(true);
        quit = false;
        selfExists = true;
        needsUpdate = false;
        needsQueueUpdate = false;
        avaliableGameNeedsUpdate = false;
        leave_Team_Funtion_Lock = false;
        apiNeedsUpdate = true;
        startQueue = false;
        memberCountText.text = " ";
        teamCodeText.text = " ";
        teamNameText.text = " ";
        gameID = "placeholder";
        teamLocalVersion = -1;
        gameLocalVersion = -1;
        selfExistWarningCount = 0;

        timerLeader = TimerLeader.GetComponent<Timer>();
        timerMember = TimerMember.GetComponent<Timer>();

        teamData = API.instance.getTeamData();
        teamCode = teamData.teamCode;
        teamCodeText.text = "Team Code: " + teamCode;

        teamLeaderUtils.SetActive(false);
        gameLeaderUtils.SetActive(false);
        teamLeaderQueueUtils.SetActive(false);
        customGameUI.SetActive(false);
        normalGameUI.SetActive(false);
        queueUI_Leader.SetActive(false);
        queueUI_Member.SetActive(false);
        createGameUI.SetActive(false);
        customGameQueueUI.SetActive(false);

        gameSearchUI.SetActive(false);


        gameIDs = new List<string>();
        localGameIDs = new List<string>();
        gameIDs.Clear();
        localGameIDs.Clear();

        gameLeader = false;

        //Reset Timers
        updateCoroutineTimer = 0;
        updateAPITimer = 0;
        updateAvaliableGameTimer = 0;

    }


    private void Update()
    {
        updateCoroutineTimer += Time.deltaTime;
        updateAPITimer += Time.deltaTime;
        updateAvaliableGameTimer += Time.deltaTime;

        if (needsUpdate)
        {
            updateCoroutineTimer = 0;
            StartCoroutine(update_Coroutine());
        }

        if (avaliableGameNeedsUpdate)
        {
            updateAvaliableGameTimer = 0;
            StartCoroutine(update_AvaliableGame_Coroutine());
        }

        if (apiNeedsUpdate)
        {
            updateAPITimer = 0;
            StartCoroutine(update_API());
        }

        if (teamData.gameID != "placeholder")
        {
            startQueue = true;
            StartCoroutine(update_Queue_Data());
        }

        if (inGameData.gameStart && !quit)
        {
            quit = true;
            LevelLoader.instance.loadScene("Relay Game");
        }

        //Update Timer Check
        if (updateCoroutineTimer >= 3)
        {
            needsUpdate = true;
        }

        if (updateAvaliableGameTimer >= 3)
        {
            avaliableGameNeedsUpdate = true;
        }

        if (updateAPITimer >= 3)
        {
            apiNeedsUpdate = true; 
        }

        /*
        //Queue Timer
        string getSecondsLeader = timerLeader.getCountUpSeconds().ToString();
        if (timerLeader.getCountUpSeconds() < 10)
        {
            getSecondsLeader = "0" + timerLeader.getCountUpSeconds().ToString();
        }

        string getSecondsMember = timerMember.getCountUpSeconds().ToString();
        if (timerMember.getCountUpSeconds() < 10)
        {
            getSecondsMember = "0" + timerMember.getCountUpSeconds().ToString();
        }

        queueUI_Leader_Text.text = "Entered Queue " + timerLeader.getCountUpMinutes().ToString() +
            " : " + getSecondsLeader + "\nWaiting for others to join...";
        queueUI_Member_Text.text = "Entered Queue " + timerMember.getCountUpMinutes().ToString() +
            " : " + getSecondsMember + "\nWaiting for others to join...";
        */


    }

    private IEnumerator buffer_init_period()
    {
        yield return new WaitForSeconds(2f);
        queueTeamNames = new string[10];
        localQueueTeamNames = new string[10];
        teamData = API.instance.getTeamData();
        teamCode = teamData.teamCode;
        apiNeedsUpdate = true;
        needsUpdate = true;
        avaliableGameNeedsUpdate = true;
        queueNeedsUpdate = true; 
    }
    private IEnumerator update_API()
    {
        apiNeedsUpdate = false;
        yield return new WaitForEndOfFrame();

        long _statusCode = 401;
        string _responseMessage = "Unknown Error";
        
        if (teamFBVersion != teamLocalVersion)
        {
            API.instance.Update_Team_Data(teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;


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

        if (gameLocalVersion != gameFBVersion && startQueue)
        {

            API.instance.Update_InGame_Data(teamData.teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;
            Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");

            inGameData = API.instance.getInGameData();

            gameLocalVersion = gameFBVersion;
        }

        //Check the existance of this team
        if (!selfExists)
        {
            Debug.Log("!selfExists detected " + selfExistWarningCount + " times.");
            selfExistWarningCount++;
            API_Periodic_Sync();
        }

        if (selfExistWarningCount >= 4)
        {
            Debug.Log("Found current userID is not in the game. Returning to main lobby.");
            selfExists = true;
            PlayerManager.instance.setTeamCode("placeholder");
            PlayerManager.instance.setGameStatus("placeholder", "placeholder");
            LevelLoader.instance.loadScene("Lobby");
        }

        apiNeedsUpdate = true;
    }

    void API_Periodic_Sync()
    {
        teamLocalVersion = -1;
        gameLocalVersion = -1; 
    }

    private IEnumerator update_AvaliableGame_Coroutine()
    {
        avaliableGameNeedsUpdate = false;

        yield return new WaitForEndOfFrame();

        FirebaseManager.instance.updateAvaliableGames(teamCode);

        while (!FirebaseManager.instance.isDataRecieved())
        {
            yield return null;
        }

        gameIDs = FirebaseManager.instance.getGameIDList();
        teamAmount = FirebaseManager.instance.getTeamAmountInEachGame();

        //New Game Display

        if (tempGameObject.Count > gameIDs.Count)
        {
            for (int i = 0; i < gameIDs.Count; ++i)
            {
                if (tempGameObject[i] == null || tempGameObject[i].GetComponent<CustomButtonClick>().getGameID() != gameIDs[i])
                {
                    Destroy(tempGameObject[i]);
                    tempGameObject[i] = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform);
                    tempGameObject[i].GetComponent<CustomButtonClick>().setDisplayText(gameIDs[i].ToString(), teamAmount[i].ToString());
                }
            }

            for (int i = gameIDs.Count; i < tempGameObject.Count; ++i)
            {
                Destroy(tempGameObject[i]);
                tempGameObject.RemoveAt(i);
            }

        }
        else
        {
            for (int i = 0; i < tempGameObject.Count; ++i)
            {
                if (tempGameObject[i] == null || tempGameObject[i].GetComponent<CustomButtonClick>().getGameID() != gameIDs[i])
                {
                    Destroy(tempGameObject[i]);
                    tempGameObject[i] = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform);
                    tempGameObject[i].GetComponent<CustomButtonClick>().setDisplayText(gameIDs[i].ToString(), teamAmount[i].ToString());
                }
            }

            for (int i = tempGameObject.Count; i < gameIDs.Count; ++i)
            {
                GameObject tempClone = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform) as GameObject;
                tempGameObject.Add(tempClone);
                tempClone.GetComponent<CustomButtonClick>().setDisplayText(gameIDs[i].ToString(), teamAmount[i].ToString());
            }
        }

        localGameIDs = gameIDs;
        /*
        //Debug.Log("From TeamLobbyManager => GameID Count: " + gameIDs.Count.ToString() + ", tempGameObject Count: " + tempGameObject.Count.ToString());
        for (int i = 0; i < tempGameObject.Count; i++)
        {
            Destroy(tempGameObject[i]);
        }

        tempGameObject.Clear();

        for (int i = 0; i < gameIDs.Count; i++)
        {
            //Debug.Log("From TeamLobbyManager => Avaliable GameID: " + gameIDs[i]);
            tempGameObject.Add(
                Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform)
                    as GameObject);
            Text gameText = joinGameButton.GetComponentInChildren<Text>();
            gameText.text = "Game ID: " + gameIDs[i].ToString() +
                "     " + teamAmount[i].ToString() + " / 5";
        }
        */

        avaliableGameNeedsUpdate = true;
    }

    private IEnumerator update_Queue_Data()
    {
        queueNeedsUpdate = false;
        yield return new WaitForEndOfFrame();

        if (startQueue)
        {
            queueUI.SetActive(true);
        }

        if (inGameData.gameID != "placeholder")
        {
            FirebaseManager.instance.updateCustomQueueData(inGameData.gameID);

            while (!FirebaseManager.instance.isDataRecieved())
            {
                yield return null;
            }

            queueTeamNames = FirebaseManager.instance.getQueueTeamNames();

            gameTitleText.text = "Game Queue (GameID: " + inGameData.gameID + ")";

            for (int i = 0; i < queueTeamNames.Length; ++i)
            {
                if (queueTeamNames[i] != localQueueTeamNames[i])
                {
                    if (queueTeamNames[i] != "placeholder" && queueTeamNames[i] != "" && queueTeamNames[i] != null)
                    {
                        queueTeamNameTexts[i].text = queueTeamNames[i];
                        queueTeams[i].SetActive(true);

                    }
                    else
                    {
                        queueTeams[i].SetActive(false);
                    }
                    localQueueTeamNames[i] = queueTeamNames[i];
                }
            }
        }

        

        if (gameLeader)
        {
            gameLeaderUtils.SetActive(true);
            
        }
        else
        {
            gameLeaderUtils.SetActive(false);
        }

        if (teamData.isLeader)
        {
            teamLeaderQueueUtils.SetActive(true);
        }
        else
        {
            teamLeaderQueueUtils.SetActive(false);
        }

        queueNeedsUpdate = true;
    }

    private IEnumerator update_Coroutine()
    {
        
        needsUpdate = false;

        //Update Routine Start...
        yield return new WaitForEndOfFrame();
        teamCodeText.text = "Team Code: " + teamCode;

        FirebaseManager.instance.updateTeamVersion(teamData.teamCode);

        while (!FirebaseManager.instance.isDataRecieved())
        {
            yield return null;
        }

        teamFBVersion = FirebaseManager.instance.getTeamVersion();

        if (inGameData.gameID != "placeholder")
        {
            FirebaseManager.instance.updateGameVersion(inGameData.gameID);

            while (!FirebaseManager.instance.isDataRecieved())
            {
                yield return null;
            }

            gameFBVersion = FirebaseManager.instance.getGameVersion();
        }
        

        bool isLeader = teamData.isLeader;
        teamName = teamData.teamname;
        memberCount = teamData.memberCount;
        memberIDs = teamData.memberIDs;

        //Debug.Log("memberCount in TeamLobbyManager:  " + memberCount);
        memberCountText.text = memberCount.ToString() + "/10";
        teamNameText.text = teamName;

        if (isLeader)
        {
            teamLeaderUtils.SetActive(true);
            customGameUI.SetActive(true);

        }
        else
        {
            teamLeaderUtils.SetActive(false);
        }

        //Join Custom Game Queue Condition
        //Debug.Log("Join Custom Game Queue Logic: " + FirebaseManager.instance.getJoinedCustomGameQueue().ToString());

        /*
        //Join Normal Game Condition
        if (teamData.gameID != "placeholder" && teamData.gameType == "Relay Game" && !quit)
        {
            //Debug.Log("Join New Normal Relay Game");
            yield return new WaitForEndOfFrame();
            gameID = teamData.gameID;
            yield return new WaitForSeconds(0.5f);
            FirebaseManager.instance.initRelayGamePlay(gameID, teamCode, teamName);
            yield return new WaitForSeconds(0.5f);
            timerLeader.stopCountUpTimer();
            timerLeader.resetCountUpTimer();
            timerMember.stopCountUpTimer();
            timerMember.resetCountUpTimer();
            SceneManager.LoadScene("Relay Game");
            //PlayerManager.instance.loadScene("Relay Game", false);
        }
        */

        //Turn off the loadingUI if it is turned on and if quit is false
        if (loadingUI.activeSelf && !quit)
        {
            LevelLoader.instance.ClearCrossFade();
            loadingUI.SetActive(false);
        }

        needsUpdate = true;

    }

    private IEnumerator updateQueueUI(string teamCode)
    {
        needsQueueUpdate = false;
        queueUI_Member.SetActive(true);

        yield return new WaitForEndOfFrame();
        //Add Time...
        timerMember.startCountUpTimer();
    }

    public void leave_Button()
    {
        StartCoroutine(leave_Coroutine());
    }

    private IEnumerator unlock_Leave_Team()
    {
        yield return new WaitForSecondsRealtime(5f);

        leave_Team_Funtion_Lock = false;
    }
    private IEnumerator leave_Coroutine()
    {
        if (!leave_Team_Funtion_Lock)
        {
            leave_Team_Funtion_Lock = true;
            quit = true;
            loadingUI.SetActive(true);
            yield return new WaitForEndOfFrame();

            API.instance.Leave_Team(teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                PlayerManager.instance.setGameStatus("placeholder", "placeholder");
                LevelLoader.instance.loadScene("Lobby");
            }
            else
            {
                loadingUI.SetActive(false);
                //Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }

            StartCoroutine(unlock_Leave_Team());
        }

    }

    public void join_game_Button()
    {
        StartCoroutine(join_game_Coroutine());
    }

    private IEnumerator join_game_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        FirebaseManager.instance.queue_to_matchmaking(teamCode);
        yield return new WaitForSeconds(0.2f);
        queueUI_Leader.SetActive(true);
        timerLeader.startCountUpTimer();
    }

    public void generateTeams()
    {
        FirebaseManager.instance.generateFakeTeams(int.Parse(numberOfTeams.text));
    }
    public void cancel_queue_Button()
    {
        quit = false;
        needsUpdate = true;
        apiNeedsUpdate = true;
        avaliableGameNeedsUpdate = true;

        FirebaseManager.instance.cancel_Queue(teamCode);
        queueUI_Leader.SetActive(false);
        timerLeader.stopCountUpTimer();
        timerLeader.resetCountUpTimer();
    }

    public string getTeamCode()
    {
        return teamCode;
    }
    public string getGameID()
    {
        return gameID;
    }

    public int getMemberID()
    {
        return memberID;
    }

    public int getMemberCount()
    {
        return memberCount;
    }

    public string getTeamName()
    {
        return teamName;
    }

    public bool getGameLeader()
    {
        return gameLeader; 
    }

    //---------------------------------------------------------------------------------Custom Game---------------------------------------------------------------------------------------------
    public void id_join_custom_game()
    {
        string _gameID = idSearchInput.text;
        StartCoroutine(idJoinGameCoroutine(_gameID));
    }

    private IEnumerator idJoinGameCoroutine(string _gameID)
    {
        LevelLoader.instance.display_loading_screen();
        gameLeader = false; 
        yield return new WaitForEndOfFrame();

        API.instance.Join_Game(_gameID, teamCode, teamName, PlayerManager.instance.getData("uid"));

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        long _statusCode = API.instance.statusCode;
        string _responseMessage = API.instance.responseMessage;

        if (_statusCode == 200)
        {
            API.instance.Update_InGame_Data(teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                queueUI.SetActive(true);
                API.instance.Update_Team_Data(teamData.teamCode, PlayerManager.instance.getData("uid"));
                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }
                inGameData = API.instance.getInGameData();
                gameID = inGameData.gameID;
                startQueue = true;
                gameSearchUI.SetActive(false);
            }
            else
            {
                quit = false;
                LevelLoader.instance.close_loading_screen();
                //Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }
        }
        else
        {
            quit = false;
            LevelLoader.instance.close_loading_screen();
            //Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
        }
    }

    public void create_custom_game()
    {
        //createGameUI.SetActive(true);
        StartCoroutine(createCustomGameCoroutine());
    }
    private IEnumerator createCustomGameCoroutine()
    {
        LevelLoader.instance.display_loading_screen();
        yield return new WaitForEndOfFrame();

        API.instance.Create_Game(teamCode, teamName, PlayerManager.instance.getData("uid"));

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        long _statusCode = API.instance.statusCode;
        string _responseMessage = API.instance.responseMessage;

        if (_statusCode == 200)
        {
            API.instance.Update_InGame_Data(teamData.teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {

                gameLeader = true; 
                API.instance.Update_Team_Data(teamData.teamCode, PlayerManager.instance.getData("uid"));
                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }
                inGameData = API.instance.getInGameData();
                gameID = inGameData.gameID;
                //CustomRelayGameLobbyManager.instance.startQueue();
                startQueue = true;
                LevelLoader.instance.close_loading_screen();
            }
            else
            {
                quit = false;
                loadingUI.SetActive(false);
                LevelLoader.instance.close_loading_screen();
                //Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }
        }
        else
        {
            quit = false;
            loadingUI.SetActive(false);
            LevelLoader.instance.close_loading_screen();
            //Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
        }

    }

    public void search_game()
    {
        avaliableGameNeedsUpdate = true;
        gameSearchUI.SetActive(true);
    }

    public void close_game_search()
    {
        gameSearchUI.SetActive(false);
        createGameUI.SetActive(false);
    }

    public void LoadToCustomRelayGame(string _game_ID)
    {
        StartCoroutine(idJoinGameCoroutine(_game_ID));
    }

    public void leave_queue()
    {
        StartCoroutine(leave_queue_coroutine()) ;
    }

    private IEnumerator leave_queue_coroutine()
    {

        API.instance.Leave_Game(inGameData.gameID, teamData.teamCode, PlayerManager.instance.getData("uid"));

        while (!API.instance.dataRecieved)
        {
            yield return null; 
        }

        long status_code = API.instance.statusCode;
        string response_message = API.instance.responseMessage;

        Debug.Log("Return with status code (" + status_code + "): " + response_message);

        if (status_code == 200)
        {
            queueUI.SetActive(false);
            startQueue = false;
            quit = false;

        }
    }

    public void startGame()
    {
        FirebaseManager.instance.gameLeader_start_game(gameID);
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
        leave_Button();

    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit Called in TeamLobby");
        API.instance.Leave_Team(teamCode, PlayerManager.instance.getData("uid"));
    }



}
