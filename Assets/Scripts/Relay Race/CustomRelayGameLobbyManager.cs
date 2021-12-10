using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;
namespace Managers
{
    public class CustomRelayGameLobbyManager : MonoBehaviour
    {
        public static CustomRelayGameLobbyManager instance;

        [Header("CRGM")]
        [SerializeField] Sprite readySprite;
        [SerializeField] Sprite unreadySprite;
        [SerializeField] GameObject QueueUI;
        [SerializeField] ButtonManager ReadyButton;
        [SerializeField] TMP_Text gameTitleText;
        public GameObject teamLeaderUtils;
        public GameObject[] teams;
        public GameObject loadingUI;
        public Text[] teamNames;
        //public Text[] teamStatus;
        [SerializeField] Text readyButtonText;

        //Private Memeber Variables (Data)
        private int teamNumber;
        private string teamCode;
        private string gameID;
        private bool readyStatus;
        private int localVersion = -1;
        private int FBVersion = 0;
        public string[] queueTeamNames;
        public string[] localQueueTeamNames;
        public string[] queueTeamStatus;
        public string[] localTeamStatus;

        InGameData inGameData;
        TeamData teamData;
        public QueueData queueData;

        //Private Memeber Variables (Utilities)
        private bool needsUpdate;
        private bool apiNeedsUpdate;
        private bool startQueing;
        private Image[] images;
        private bool quit;

        private void Awake()
        {
            instance = this;
        }
        void Start()
        {
            loadingUI.SetActive(true);
            //Init Utitlities

            needsUpdate = true;
            apiNeedsUpdate = true;
            startQueing = false;
            quit = false;

            //Init Data
            readyStatus = false;
            queueTeamNames = new string[5];
            queueTeamStatus = new string[5];
            localQueueTeamNames = new string[5];
            localTeamStatus = new string[5];

            images = new Image[teams.Length];

            for (int i = 0; i < teams.Length; ++i)
            {
                images[i] = teams[i].GetComponent<Image>();
                images[i].sprite = unreadySprite;
            }


            for (int i = 0; i < teams.Length; ++i)
            {
                teams[i].SetActive(false);
            }


            FBVersion = 0;
            localVersion = -1;

            gameID = TeamLobbyManager.instance.getGameID();
            teamCode = TeamLobbyManager.instance.getTeamCode();
            teamData = API.instance.getTeamData();

            teamLeaderUtils.SetActive(false);


            ReadyButton.buttonText = "Ready";
            ReadyButton.UpdateUI();
            //Debug.Log("gameID in CRGL: " + gameID);
            for (int i = 0; i < localQueueTeamNames.Length; ++i)
            {
                localQueueTeamNames[i] = "placeholder";
                localTeamStatus[i] = "False";
            }
        }

        public void startQueue()
        {
            startQueing = true;
            needsUpdate = true;
            apiNeedsUpdate = true;
            API.instance.resetQueueData();
            QueueUI.SetActive(true);
        }

        void Update()
        {
            if (startQueing)
            {
                teamData = API.instance.getTeamData();


                if (needsUpdate)
                {
                    StartCoroutine(update_Coroutine());
                }

                if (apiNeedsUpdate)
                {
                    StartCoroutine(update_API());
                }

            }

        }


        private IEnumerator update_API()
        {
            apiNeedsUpdate = false;
            yield return new WaitForEndOfFrame();

            inGameData = API.instance.getInGameData();

            gameID = teamData.gameID;
            teamCode = teamData.teamCode;

            //queueData = API.instance.getQueueData();

            if (teamData.isLeader)
            {
                teamLeaderUtils.SetActive(true);
            }

            yield return new WaitForSeconds(0.5f);
            apiNeedsUpdate = true;
        }

        private IEnumerator update_Coroutine()
        {
            needsUpdate = false;

            yield return new WaitForEndOfFrame();

            //Debug.Log("Running bro come on");

            //Cancel Queue Condition
            if (teamData.gameID == "placeholder" && !teamData.isLeader)
            {
                QueueUI.SetActive(false);
                startQueing = false;
            }

            //Start Game Condition
            if (inGameData.gameStart && !quit)
            {
                StartCoroutine(start_game_Routine());
                //PlayerManager.instance.loadScene("Relay Game", false);
            }

            FirebaseManager.instance.updateCustomQueueData(inGameData.gameID);
            queueTeamNames = FirebaseManager.instance.getQueueTeamNames();
            queueTeamStatus = FirebaseManager.instance.getQueueTeamStatus();

            gameTitleText.text = "Game Queue (GameID: " + inGameData.gameID + ")";
            //queueTeamNames = queueData.teamNameList;
            //queueTeamStatus = queueData.teamReadyStatus;


            //Update UI

            for (int i = 0; i < queueTeamNames.Length; ++i)
            {
                if (queueTeamNames[i] != localQueueTeamNames[i] || queueTeamStatus[i] != localTeamStatus[i])
                {
                    if (queueTeamNames[i] != "placeholder" && queueTeamNames[i] != "" && queueTeamNames[i] != null)
                    {
                        teamNames[i].text = queueTeamNames[i];
                        if (queueTeamStatus[i] == "True")
                        {
                            //teamStatus[teamNumber].text = "Ready";
                            images[i].sprite = readySprite;
                        }
                        else
                        {
                            //teamStatus[teamNumber].text = "Not Ready";
                            images[i].sprite = unreadySprite;
                        }

                        //Debug.Log("Setting teams[" + i + "] to active");
                        teams[i].SetActive(true);

                    }
                    else
                    {
                        Debug.Log("Setting teams[" + i + "] to not active");
                        teams[i].SetActive(false);
                    }
                    localQueueTeamNames[i] = queueTeamNames[i];
                    localTeamStatus[i] = queueTeamStatus[i];
                }
            }

            needsUpdate = true;
        }

        private IEnumerator start_game_Routine()
        {
            quit = true;
            yield return new WaitForEndOfFrame();
            Debug.Log("Game Have started, loading relay game");
            LevelLoader.instance.loadScene("Relay Game");
        }

        public void cancel_queue()
        {
            StartCoroutine(cancel_queue_Coroutine());
        }

        private IEnumerator cancel_queue_Coroutine()
        {
            loadingUI.SetActive(true);
            needsUpdate = false;
            yield return new WaitForEndOfFrame();
            API.instance.Leave_Game(inGameData.gameID, teamData.teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                QueueUI.SetActive(false);
                startQueing = false;
            }
            else
            {
                loadingUI.SetActive(false);
                needsUpdate = true;
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }
        }
    }
}

