using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;
using Cinemachine;
using Managers;

namespace Managers
{
    public class RelayGameManager : MonoBehaviour
    {
        public static RelayGameManager instance;

        [Header("Relay Game Manager")]
        [SerializeField] Text stepsText;
        [SerializeField] Text warningText;
        [SerializeField] TMP_Text multiplier_text;
        //[SerializeField] Text runnerText;
        [SerializeField] Text rankText;
        [SerializeField] Text totalStepsText;
        [SerializeField] Text velocityText;
        [SerializeField] Text teamNameText;
        [SerializeField] Text cameraDisplay;
        [SerializeField] Slider speedometer;
        [SerializeField] ProgressBar speedometer_MUIP;

        [SerializeField] GameObject countDownUI;
        [SerializeField] GameObject viewerUI;
        [SerializeField] GameObject runnerUI;
        [SerializeField] GameObject joystickUI;
        [SerializeField] GameObject loadingUI;
        [SerializeField] GameObject[] maps;
        [SerializeField] Text countDownText;
        [SerializeField] GameObject tap_UI;
        private bool perspective; // true = third person, false = first person
        private Animator tap_anim;

        [SerializeField] GameObject[] PlayersArr;

        [Header("Leader Board")]
        [SerializeField] GameObject leaderBoardUI;
        private Animator leaderBoardAnim;
        private bool isCollapse = false;

        [Space]

        [Header("AFK Screen")]
        [SerializeField] ModalWindowManager afkScreen;

        [SerializeField] Text[] rankStepTexts;
        [SerializeField] Text[] rankTeamNameTexts;
        private string[] leaderboardTeamName = new string[5];
        private double[] leaderboardTotalSteps = new double[5];
        private string[] otherTeamNames = new string[5];
        private string[] otherTeamCodes = new string[5];
        private float[] otherTeamSteps = new float[5];
        private double[] otherTeamDistance = new double[5];
        private double[] otherTeamVelocity = new double[5];
        private int[] otherTeamSkinID = new int[5];
        private string[] otherTeamRunner = new string[5];
        private string[] otherTeamImageURL = new string[5];

        [Header("Step Count")]
        //Step Count Variables
        private float lowLimit = 0.005f; //平缓
        private bool isHigh = false; // 状态
        private float filterCurrent = 10.0f; // 滤波参数 得到拟合值
        private float filterAverage = 0.1f; //   滤波参数  得到均值
        private float accelerationCurrent = 0f; //拟合值
        private float accelerationAverage = 0f;//均值

        [Header("Camera")]
        // Cinemachine cameras
        public GameObject ThirdPersonCamera;
        public GameObject FirstPersonCamera;
        public GameObject FlyCameraStand;
        public GameObject FlyCameraStand_Reset_Position;
        public Transform AIM;
        public GameObject[] cameras;
        public GameObject[] winCamera;
        public GameObject[] winCameraFront;
        private bool currentTurnTransition = false;
        public int currentCameraIndex;

        // Win Scene Variables
        private int winTeamIndex;

        //Private Coroutine Variables / Operation Logics
        private bool activate_Utilities;
        private bool anyWins;
        private bool needsUpdate;
        private bool apiNeedsUpdate;
        private bool cameraNeedsReset;
        private bool write_data = true;
        private bool gameStart;
        private bool myTurnInit = true;
        private bool myTurnNeedsUpdate = false;
        private bool autoLock = false;
        private string[] camera_Names = { "Third Person View", "Third Person Side View", "Third Person Front View", "Finish Line Angled View", "Finish Line Ominious View", "Full Runway Ominious View", "Mid-Runway Ominious View", "Fly Camera View" };
        private int local_mapID;
        private bool quit;


        //Private Player Data
        private float totalSteps;
        private string teamCode;
        private string prevRunner;
        private string gameName;
        private string gameID;
        private string gameStatus;
        private int turns;
        private int teamLocalVersion = -1;
        private int teamFBVersion = 0;
        private int gameLocalVersion = -1;
        private int gameFBVersion = 0;

        private float multiplier_factor = 1;

        InGameData inGameData;
        TeamData teamData;

        //Firebase Config Constants
        private int _TOTALSTEPS;
        private int _STEP_THRESHOLD;

        [Header("Timer")]
        //Timer Utils
        private float timer;
        private float personalBestTime;
        public bool isStartCountDown = false;
        public bool isStartGame = false;

        [Header("Constants")]
        public double cameraResetOffset;
        private void Reset_Variables()
        {

            leaderboardTeamName = new string[5];
            leaderboardTotalSteps = new double[5];
            otherTeamNames = new string[5];
            otherTeamCodes = new string[5];
            otherTeamSteps = new float[5];
            otherTeamDistance = new double[5];
            otherTeamVelocity = new double[5];
            otherTeamSkinID = new int[5];
            otherTeamRunner = new string[5];
            otherTeamImageURL = new string[5];

            currentTurnTransition = false;
            needsUpdate = true;
            apiNeedsUpdate = true;
            myTurnNeedsUpdate = true;
            myTurnInit = true;
            gameStart = false;
            autoLock = true;
            write_data = true;
            open_Cheat = false;
            perspective = true;
            warningActive = false;
            activate_Utilities = false;
            quit = false;
            afk_handled = false;
            cameraNeedsReset = true;

            A_Key = 0;
            B_Key = 0;

            turns = 1;
            distance = 0;
            myDistance = 0;
            personalBestTime = 100000f;
            teamFBVersion = 0;
            gameFBVersion = 0;
            local_mapID = 0;
            teamLocalVersion = -1;
            gameLocalVersion = -1;
            winTeamIndex = 0;
            multiplier_factor = 1;
            warningCount = 0;
            kickCountDown = 5;

            tap_anim = tap_UI.GetComponent<Animator>();

            reset_fly_cam();
        }

        void Awake()
        {
            //Screen.orientation = ScreenOrientation.Portrait;

            instance = this;
            winTeamCode = "placeholder";
            Reset_Variables();

            gameName = PlayerManager.instance.getData("gameName");
            gameStatus = PlayerManager.instance.getData("gameStatus");


            //Initalize all the value for use
            accelerationAverage = Input.acceleration.magnitude;
            countDownUI.SetActive(false);

            teamData = API.instance.getTeamData();
            inGameData = API.instance.getInGameData();

            if (teamData.isLeader)
            {
                currentTurnTransition = true;
            }
            else
            {
                currentTurnTransition = false;
            }

            distance = inGameData.distance;

            for (int i = 0; i < leaderboardTeamName.Length; ++i)
            {
                leaderboardTeamName[i] = "N/A";
                leaderboardTotalSteps[i] = 0;
            }

            for (int i = 0; i < maps.Length; ++i)
            {
                maps[i].SetActive(false);
            }

            maps[0].SetActive(true);

            loadingUI.SetActive(true);
            leaderBoardAnim = leaderBoardUI.GetComponent<Animator>();
            leaderBoardAnim.enabled = false;
        }
        void Start()
        {
            //relayBatonAnim = relayBatonTransition.GetComponent<Animator>();

            InvokeRepeating("calculate_distance", 0.0f, 0.5f);
            InvokeRepeating("calculate_step_frequency", 0.0f, 2f);
            StartCoroutine(initRelayGameData());
        }

        /*private IEnumerator play_Transition()
        {
            playersOnScreenUI.SetActive(false);
            leaderBoardUI.SetActive(false);
            relayBatonTransition.SetActive(true);
            relayBatonAnim.SetTrigger("play");
            yield return new WaitForSecondsRealtime(1.55f);
            playersOnScreenUI.SetActive(true);
            leaderBoardUI.SetActive(true);
            relayBatonTransition.SetActive(false);
        }*/
        private IEnumerator initRelayGameData()
        {
            isStartGame = false;
            stepsText.text = "Steps x 0";
            warningText.text = " ";
            //runnerText.text = "#1 Runner";
            rankText.text = "5th";
            totalStepsText.text = "Team Total Distance: 0m";

            //Get Team Data from TeamLobbyManager and initialize data in this class
            teamCode = teamData.teamCode;

            //Get Team Data from Firebase and initialize data in this class
            yield return new WaitForEndOfFrame();
            StartCoroutine(update_API());
            yield return new WaitForSeconds(3f);

            totalSteps = inGameData.totalSteps;
            prevTotalSteps = totalSteps;
            prevRunner = inGameData.runner;
            _TOTALSTEPS = inGameData.relay_totalsteps;
            _STEP_THRESHOLD = inGameData.relay_steps_threshold;


            if (gameStatus != "In Game")
            {
                countDownUI.SetActive(true);
                countDownText.text = "";
                LevelLoader.instance.ClearCrossFade();
                //PlayerManager.instance.resetLoadScene();
                loadingUI.SetActive(false);
                yield return new WaitForSecondsRealtime(1f);

                countDownText.text = "Start In...";
                yield return new WaitForSecondsRealtime(1f);
                isStartCountDown = true;
                countDownText.text = "3";
                yield return new WaitForSecondsRealtime(1f);
                countDownText.text = "2";
                gameStart = true;
                yield return new WaitForSecondsRealtime(1f);
                countDownText.text = "1";
                yield return new WaitForSecondsRealtime(1f);
                countDownText.text = "START!";
                yield return new WaitForSecondsRealtime(1.2f);
                isStartCountDown = false;
                countDownUI.SetActive(false);
            }

            play_tutorial();

            autoLock = false;
            isStartGame = true;
            gameStart = true;
            PlayerManager.instance.setGameStatus("Custom Relay Game", "In Game");


        }

        private IEnumerator update_API()
        {
            apiNeedsUpdate = false;
            yield return new WaitForEndOfFrame();

            if (teamFBVersion != teamLocalVersion)
            {
                API.instance.Update_Team_Data(teamCode, PlayerManager.instance.getData("uid"));

                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }

                long _statusCode = API.instance.statusCode;
                string _responseMessage = API.instance.responseMessage;


                if (_statusCode == 200)
                {
                    teamData = API.instance.getTeamData();
                    API.instance.logTeamData();
                }
                else
                {
                    Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
                }

                teamLocalVersion = teamFBVersion;
            }

            if (gameFBVersion != gameLocalVersion)
            {
                API.instance.Update_InGame_Data(teamCode, PlayerManager.instance.getData("uid"));

                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }

                long _statusCode = API.instance.statusCode;
                string _responseMessage = API.instance.responseMessage;


                if (_statusCode == 200)
                {
                    inGameData = API.instance.getInGameData();

                    if (inGameData.gameID == "placeholder" && gameStart)
                    {
                        LevelLoader.instance.loadScene("Team Lobby");
                    }
                }
                else
                {
                    Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
                    LevelLoader.instance.loadScene("Team Lobby");
                }

                gameLocalVersion = gameFBVersion;
            }
            apiNeedsUpdate = true;
        }

        private IEnumerator update_Coroutine()
        {
            needsUpdate = false;
            yield return new WaitForEndOfFrame();

            //Update Data

            FirebaseManager.instance.updateTeamVersion(teamData.teamCode);
            FirebaseManager.instance.updateGameVersion(inGameData.gameID);

            yield return new WaitForSeconds(0.5f);

            teamFBVersion = FirebaseManager.instance.getTeamVersion();
            gameFBVersion = FirebaseManager.instance.getGameVersion();

            //Get several data from the firebase  

            prevRunner = inGameData.runner;
            _TOTALSTEPS = inGameData.relay_totalsteps;
            _STEP_THRESHOLD = inGameData.relay_steps_threshold;
            teamNameText.text = teamData.teamname;

            if (!inGameData.myTurn)
            {
                distance = inGameData.distance;
                velocity = inGameData.velocity;
                double _totalSteps = inGameData.totalSteps;

                if (_totalSteps > totalSteps)
                {
                    totalSteps = inGameData.totalSteps;
                }
            }

            //Display Read Data to the UI
            if (!inGameData.gameEnd)
            {
                update_Leaderboard();
            }

            needsUpdate = true;
        }


        void Update()
        {
            if (FlyCameraStand.transform.position.x > FlyCameraStand_Reset_Position.transform.position.x - cameraResetOffset)
            {
                Debug.Log("FlyCameraStand Position : (" + FlyCameraStand.transform.position.x + ", " + FlyCameraStand.transform.position.y + ", " + FlyCameraStand.transform.position.z + ")");
                Debug.Log("FlyCameraStandReset Position : (" + FlyCameraStand_Reset_Position.transform.position.x + ", " + FlyCameraStand_Reset_Position.transform.position.y + ", " + FlyCameraStand_Reset_Position.transform.position.z + ")");
                cameraNeedsReset = false;
                StartCoroutine(reset_fly_cam());
            }

            multiplier_text.text = "Team Bonus: Steps x " + System.Math.Round(inGameData.bonus, 2).ToString();
            //Debug.Log("myDistance: " + myDistance + " Modified: " + ((int)distance / _STEP_THRESHOLD).ToString() + "storedTurns: " + storedTurns.ToString());
            if (gameStart)
            {
                //Activation On: Detect there's a winner in this game
                if (inGameData.gameEnd && !quit)
                {
                    Debug.Log("Game Has Ended...Calculating Data");
                    quit = true;
                    autoLock = true;
                    FirebaseManager.instance.write_teamRecords(inGameData.gameID, teamData.teamCode, PlayerManager.instance.getData("uid"), personalBestTime);
                    StartCoroutine(play_Win_Animation());
                }

                //Activation On: Finish current update and ready to start the next
                if (needsUpdate)
                {
                    StartCoroutine(update_Coroutine());
                }

                if (apiNeedsUpdate)
                {
                    StartCoroutine(update_API());
                }

                //Activation On: It is the turn where this player is the runner
                if (inGameData.myTurn)
                {
                    if (currentTurnTransition)
                    {
                        currentTurnTransition = false;
                        currentCameraIndex = 0;
                        cameras[0].GetComponent<CinemachineFreeLook>().Priority = 1;
                        cameras[7].GetComponent<CinemachineVirtualCamera>().Priority = 0;
                    }

                    if (!inGameData.gameEnd)
                    {
                        StartCoroutine(myTurnRoutine());
                        viewerUI.SetActive(false);
                        runnerUI.SetActive(true);
                        joystickUI.SetActive(false);
                    }

                }
                else
                {
                    runnerUI.SetActive(false);
                    viewerUI.SetActive(true);
                    joystickUI.SetActive(true);
                    if (!currentTurnTransition)
                    {
                        currentTurnTransition = true;
                        currentCameraIndex = 7;
                        cameras[0].GetComponent<CinemachineFreeLook>().Priority = 0;
                        cameras[7].GetComponent<CinemachineVirtualCamera>().Priority = 1;
                    }
                    cameraDisplay.text = camera_Names[currentCameraIndex];
                    autoLock = false;
                    myTurnInit = true;
                }

                if (teamData.memberCount == 1)
                {

                    autoLock = false;
                }
                else
                {
                    int count = 0;
                    for (int i = 0; i < teamData.isOnlineList.Length; ++i)
                    {
                        if (teamData.isOnlineList[i] == "True" || teamData.isOnlineList[i] == "true")
                        {
                            ++count;
                        }
                    }

                    if (count <= 1)
                    {
                        autoLock = false;
                    }

                }

                //Activation On: Switch Turns
                if (_STEP_THRESHOLD > 0)
                {
                    int myStoredTurns = (int)distance / _STEP_THRESHOLD;

                    if (myStoredTurns > storedTurns && !inGameData.myTurn)
                    {
                        resetStep();
                    }
                }


                if (local_mapID != inGameData.relay_map)
                {
                    updateMap();
                }
            }
        }

        public void updateMap()
        {
            for (int i = 0; i < maps.Length; ++i)
            {
                if (i == inGameData.relay_map)
                {
                    maps[i].SetActive(true);
                }
                else
                {
                    maps[i].SetActive(false);
                }
            }

            local_mapID = inGameData.relay_map;
        }

        private IEnumerator myTurnRoutine()
        {
            myTurnNeedsUpdate = false;
            yield return new WaitForEndOfFrame();

            if (myTurnInit)
            {
                Debug.Log("Start Timer");
                velocity = 0;
                timer = 0f;
                warningCount = 0;
                warningActive = false;
                myTurnInit = false;
                reset_fly_cam();
                sendNotificaton();
            }

            timer += Time.deltaTime;

            if (_STEP_THRESHOLD != 0)
            {
                int myStoredTurns = (int)distance / _STEP_THRESHOLD;

                if (myStoredTurns > storedTurns)
                {
                    Debug.Log("Stop Timer");
                    myTurnInit = true;
                    //Debug.Log("Passing To Other Player");
                    if (timer < personalBestTime && timer >= 8)
                    {
                        personalBestTime = timer;
                    }
                    reset_fly_cam();
                    resetStep();
                }
            }


            myTurnNeedsUpdate = true;
        }

        private void resetStep()
        {
            Debug.Log("Reset Steps");
            storedTurns = (int)distance / _STEP_THRESHOLD;
        }


        private void sendNotificaton()
        {
            // Handheld.Vibrate();
        }


        //-------------------------------------------------------------Pedometer Code-----------------------------------------------------------------------------------
        /*
        void FixedUpdate()
        {

            accelerationCurrent = Mathf.Lerp(accelerationCurrent, Input.acceleration.magnitude, Time.deltaTime * filterCurrent);
            accelerationAverage = Mathf.Lerp(accelerationAverage, Input.acceleration.magnitude, Time.deltaTime * filterAverage);
            float delta = accelerationCurrent - accelerationAverage;

            if (!isHigh)
            {
                if (delta > 0.06f)//往高
                {
                    isHigh = true;
                    int myStoredTurns = (int)distance / _STEP_THRESHOLD;
                    if (!autoLock && inGameData.myTurn && myStoredTurns <= storedTurns)
                    {
                        Debug.Log("Relay Game Manager: " + multiplier_factor);
                        totalSteps += 3 * multiplier_factor;
                    }
                }
            }
            else
            {
                if (delta < lowLimit)//往低
                {

                    isHigh = false;
                }
            }

        }
        */
        //--------------------------------------------------------------------UI Buttons-----------------------------------------------------------------------------------
        bool left_foot = false;
        bool right_foot = false;
        public void leaveGame()
        {
            StartCoroutine(leave_coroutine());
        }

        private IEnumerator leave_coroutine()
        {
            API.instance.Leave_Team(teamCode, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
            }
            else
            {
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }

            PlayerManager.instance.setGameStatus("placeholder", "placeholder");
            PlayerManager.instance.setTeamCode("placeholder");
            LevelLoader.instance.loadScene("Lobby");
        }

        /*
        public void tap_to_Run()
        {
            int myStoredTurns = (int)distance / _STEP_THRESHOLD;
            if (!autoLock && inGameData.myTurn && myStoredTurns <= storedTurns)
            {
                Debug.Log("Relay Game Manager: " + multiplier_factor);
                totalSteps += 3 * multiplier_factor;
            }
        }
        */

        public void tap_left_foot()
        {
            if (_STEP_THRESHOLD > 0)
            {
                int myStoredTurns = (int)distance / _STEP_THRESHOLD;
                if (!autoLock && inGameData.myTurn && myStoredTurns <= storedTurns)
                {
                    left_foot = true;
                    if (left_foot && right_foot)
                    {
                        totalSteps += inGameData.bonus;
                        left_foot = false;
                        right_foot = false;
                    }
                }
            }


        }

        public void tap_right_foot()
        {
            int myStoredTurns = (int)distance / _STEP_THRESHOLD;
            if (!autoLock && inGameData.myTurn && myStoredTurns <= storedTurns)
            {
                right_foot = true;
                if (left_foot && right_foot)
                {
                    totalSteps += inGameData.bonus;
                    left_foot = false;
                    right_foot = false;
                }
            }

        }

        public void switch_perspective()
        {
            if (perspective)
            {
                perspective = false;
                ThirdPersonCamera.GetComponent<CinemachineFreeLook>().Priority = 0;
                FirstPersonCamera.GetComponent<CinemachineFreeLook>().Priority = 1;
            }
            else
            {
                perspective = true;
                FirstPersonCamera.GetComponent<CinemachineFreeLook>().Priority = 0;
                ThirdPersonCamera.GetComponent<CinemachineFreeLook>().Priority = 1;
            }
        }

        private IEnumerator reset_fly_cam()
        {
            yield return new WaitForEndOfFrame();

            FlyCamera.instance.setAutoPilot();

            yield return new WaitForSeconds(1.5f);

            cameraNeedsReset = true;
        }

        //-----------------------------------------------------------------Leaderboard Code-----------------------------------------------------------------------------------

        private void update_Leaderboard()
        {
            FirebaseManager.instance.getLeaderBoardData(inGameData.gameID, teamData.teamCode);

            int ranking = 0;

            for (int i = 0; i < leaderboardTeamName.Length; ++i)
            {
                if (teamData.teamname == leaderboardTeamName[i])
                {
                    ranking = i + 1;
                    break;
                }
            }

            if (ranking == 1)
            {
                rankText.text = "1st";
            }
            else if (ranking == 2)
            {
                rankText.text = "2nd";
            }
            else if (ranking == 3)
            {
                rankText.text = "3rd";
            }
            else
            {
                rankText.text = ranking.ToString() + "th";
            }

            leaderboardTeamName = FirebaseManager.instance.getLeaderboardTeamName();
            leaderboardTotalSteps = FirebaseManager.instance.getLeaderboardTotalSteps();
            otherTeamNames = FirebaseManager.instance.getOtherTeamNames();
            otherTeamCodes = FirebaseManager.instance.getOtherTeamCodes();
            otherTeamSteps = FirebaseManager.instance.getOtherTeamSteps();
            otherTeamDistance = FirebaseManager.instance.getOtherTeamDist();
            otherTeamVelocity = FirebaseManager.instance.getOtherTeamVelocity();
            otherTeamSkinID = FirebaseManager.instance.getOtherTeamSkinIDs();
            otherTeamRunner = FirebaseManager.instance.getOtherTeamRunners();
            otherTeamImageURL = FirebaseManager.instance.getOtherTeamImageURL();

            otherTeamNames[0] = teamData.teamname;
            otherTeamCodes[0] = teamData.teamCode;
            otherTeamSteps[0] = totalSteps;
            otherTeamDistance[0] = distance;
            otherTeamVelocity[0] = velocity;
            otherTeamRunner[0] = inGameData.runner;

            for (int i = 1; i < otherTeamNames.Length; ++i)
            {
                if (otherTeamNames[i] == "placeholder" || otherTeamNames[i] == null || otherTeamNames[i] == "")
                {
                    PlayersArr[i].SetActive(false);
                }
                else
                {
                    PlayersArr[i].SetActive(true);
                }
            }

            for (int i = 0; i < leaderboardTeamName.Length; ++i)
            {
                if (leaderboardTeamName[i] != "placeholder")
                {
                    if (leaderboardTeamName[i] == teamData.teamname)
                    {
                        totalStepsText.text = "Distance: " + System.Math.Round(leaderboardTotalSteps[i], 1).ToString() + " (m)";
                        velocityText.text = System.Math.Round(velocity, 1).ToString() + " m/s";
                        speedometer_MUIP.speed = 2;
                        speedometer_MUIP.currentPercent = ((float)velocity / 10f) * 100;
                        //speedometer_MUIP.textPercent.text = System.Math.Round(velocity, 1).ToString() + " m/s";
                        speedometer_MUIP.UpdateUI();
                        //speedometer.value = (float)velocity/5f;
                        //speedometer.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = new Color(((float)velocity / 5f), (1 - ((float)velocity / 5f)), 0, 0.5f); ;
                    }

                    rankTeamNameTexts[i].text = leaderboardTeamName[i];
                    rankStepTexts[i].text = System.Math.Round(leaderboardTotalSteps[i], 1).ToString();
                }
                else
                {
                    rankTeamNameTexts[i].text = "";
                    rankStepTexts[i].text = "";
                }
            }
        }

        // onClick collapse the leaderBoardUI
        public void CollapseLeaderBoardUI()
        {
            leaderBoardAnim.enabled = true;
            if (isCollapse)
            {
                leaderBoardAnim.SetBool("isCollapse", true);
            }
            else
            {
                leaderBoardAnim.SetBool("isCollapse", false);
            }
            isCollapse = !isCollapse;
        }

        public string[] getOtherTeamNames()
        {
            return otherTeamNames;
        }

        public float[] getOtherTeamSteps()
        {
            return otherTeamSteps;
        }

        public double[] getOtherTeamDistance()
        {
            return otherTeamDistance;
        }

        public double[] getOtherTeamVelocity()
        {

            return otherTeamVelocity;
        }

        public string[] getOtherTeamImageURL()
        {
            return otherTeamImageURL;
        }
        public bool getIsMyTurn()
        {
            return inGameData.myTurn;
        }

        public int[] getOtherTeamSkinIDs()
        {
            return otherTeamSkinID;
        }

        public string[] getOtherTeamRunners()
        {
            return otherTeamRunner;
        }
        public bool getGameStart()
        {
            return activate_Utilities;
        }

        public float getMultiplierFactor()
        {
            return multiplier_factor;
        }

        public void setMultiplierFactor(float _factor)
        {
            multiplier_factor = _factor;
        }

        public bool getLeftTapButtonStatus()
        {
            return left_foot;
        }

        public bool getRightTapButtonStatus()
        {
            return right_foot;
        }

        public bool getPerspective()
        {
            return perspective;
        }
        //-----------------------------------------------------New Relay Race Calculation--------------------------------------------------------
        private float prevTotalSteps = 0;
        private double velocity = 0;
        private double distance = 0;
        private double myDistance = 0;
        private float stepFreq = 0;
        private int storedTurns = 0;

        private float warningCount = 0;
        private float kickCountDown = 5;
        private bool warningActive = false;

        private void calculate_step_frequency()
        {
            float currSteps = totalSteps - prevTotalSteps;
            prevTotalSteps = totalSteps;
            stepFreq = currSteps / 2;

            /*
            if (currSteps == 0 && !warningActive && inGameData.myTurn)
            {
                warningCount += 1;
            }
            else
            {
                warningCount = 0;
            }

            if (warningCount >= 10 && inGameData.myTurn)
            {
                warningCount = 0;
                warningActive = true;
                StartCoroutine(afk_Countdown());
            }
            //Debug.Log("stepFreq: " + stepFreq);
            */
        }

        private void calculate_distance()
        {
            if (_STEP_THRESHOLD != 0)
            {
                int myStoredTurns = (int)distance / _STEP_THRESHOLD;
                if (!autoLock && inGameData.myTurn && write_data && distance < _TOTALSTEPS && myStoredTurns <= storedTurns)
                {
                    StartCoroutine(Calculate_Distance_Coroutine());
                }
            }

        }

        IEnumerator Calculate_Distance_Coroutine()
        {
            write_data = false;
            yield return new WaitForEndOfFrame();

            inGameData = API.instance.getInGameData();
            API.instance.logTeamData();
            float acceleration;

            double _distance = inGameData.distance;
            if (_distance > distance)
            {
                distance = _distance;
            }

            velocity = inGameData.velocity;

            if (stepFreq >= 1 && stepFreq < 2)
            {
                stepFreq += 0.5f;
            }

            if (stepFreq >= 1)
            {
                acceleration = 1.5f * Mathf.Log10(stepFreq);
            }
            else
            {
                acceleration = -0.1f;
            }

            if (open_Cheat)
            {
                velocity = 8f;
            }
            else if ((velocity * 0.7f) + acceleration >= 10.8f)
            {
                velocity = 10.8f;
            }
            else if ((velocity * 0.7f) + acceleration <= 0)
            {
                velocity = 0;
            }
            else
            {
                velocity = System.Math.Round((velocity * 0.7f) + acceleration, 4);
            }

            if ((distance % _STEP_THRESHOLD) + velocity > _STEP_THRESHOLD)
            {
                if (teamData.memberCount != 1)
                {
                    autoLock = true;
                }

                distance = System.Math.Round(((distance + System.Math.Round(velocity * 0.5, 1)) / 100d), 0) * 100;
            }
            else
            {
                distance += System.Math.Round(velocity * 0.5, 1);
            }

            API.instance.Post_Steps(inGameData.gameID, teamCode, totalSteps.ToString(), distance.ToString(), velocity.ToString(), PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");

            write_data = true;
        }

        public void cancel_afk_procedure()
        {
            warningCount = 0;
            warningActive = false;
            afkScreen.CloseWindow();
        }
        private IEnumerator afk_Countdown()
        {
            kickCountDown = 5;
            afkScreen.titleText = "AFK Kick Warning";
            afkScreen.descriptionText = "You turn will be passed on in 5.00 seconds.";
            afkScreen.UpdateUI();
            afkScreen.OpenWindow();

            while (kickCountDown >= 0)
            {
                kickCountDown -= Time.deltaTime;
                //change_runner
                afkScreen.descriptionText = "You turn will be passed on in...\n" + System.Math.Round(kickCountDown, 2).ToString() + " (s)";
                afkScreen.UpdateUI();
                yield return null;
            }

            //API CAll kick
            if (warningActive)
            {
                myTurnInit = true;
                afkScreen.descriptionText = "Your turn have been passed on.";
                afkScreen.UpdateUI();
                //API.instance.Post_AFK_Request(inGameData.gameID, teamData.teamCode, PlayerManager.instance.getData("uid"));
                Debug.Log("Sending request to kick");
            }
        }

        bool afk_handled = false;
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && !afk_handled)
            {
                afk_handled = true;
                API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), false, inGameData.myTurn);
                Debug.Log("Application Focus Called (Leave)");
            }
            else if (focus)
            {
                afk_handled = false;
                //PlayerManager.instance.setIsOnline("True");
                API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), true, inGameData.myTurn);
                //StartCoroutine(Handle_AFK("True", true));
                Debug.Log("Application Focus Called (Join)");
            }
        }
        private void OnApplicationPause(bool pause)
        {

            if (pause && !afk_handled)
            {
                afk_handled = true;
                //PlayerManager.instance.setIsOnline("False");
                API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), false, inGameData.myTurn);
                //StartCoroutine(Handle_AFK("False", false));
                Debug.Log("Application Pause Called (Leave)");
            }
            else if (!pause)
            {
                afk_handled = false;
                //PlayerManager.instance.setIsOnline("True");
                API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), true, inGameData.myTurn);
                //StartCoroutine(Handle_AFK("True", true));
                Debug.Log("Application Pause Called (Join)");
            }

        }


        private void OnApplicationQuit()
        {
            Debug.Log("Online status Seen in appQuit: " + PlayerManager.instance.getData("online status"));
            if (!afk_handled)
            {
                afk_handled = true;
                //PlayerManager.instance.setIsOnline("False");
                API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), false, inGameData.myTurn);
                Debug.Log("OnApplicationQuit Called");
            }
        }

        private IEnumerator Handle_AFK(string setStatus, bool isOnline)
        {
            PlayerManager.instance.setIsOnline(setStatus);
            yield return new WaitForSecondsRealtime(1f);

            Debug.Log("Sending AFK Request...");
            API.instance.Post_AFK_Request(teamData.teamCode, teamData.gameID, PlayerManager.instance.getData("uid"), isOnline, inGameData.myTurn);
        }

        //-----------------------------------------------------Cheat Codes--------------------------------------------------------
        int A_Key;
        int B_Key;

        bool open_Cheat;

        public void add_A_Key()
        {
            A_Key += 1;
        }

        public void add_B_Key()
        {
            B_Key += 1;
        }
        public void enter_Cheat_Code()
        {
            if (A_Key == 6 && B_Key == 9)
            {
                open_Cheat = true;
            }
            else
            {
                open_Cheat = false;
            }

            A_Key = 0;
            B_Key = 0;
        }

        //------------------------------------------------------Camera Change-------------------------------------------------------
        public void PrevCamera()
        {
            // Prev priority to 0
            if (currentCameraIndex == 0)
            {
                cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
                currentCameraIndex = 6;
            }
            else
            {
                cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
                currentCameraIndex -= 1;
            }

            // Current priority to 1
            cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 1;
        }

        public void NextCamera()
        {
            // Prev priority to 0
            if (currentCameraIndex == 6)
            {
                cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
                currentCameraIndex = 0;
            }
            else
            {
                cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
                currentCameraIndex += 1;
            }

            // Current priority to 1
            cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 1;

        }

        //------------------------------------------------------Win Scene-------------------------------------------------------
        private string winTeamCode;
        private IEnumerator play_Win_Animation()
        {
            viewerUI.SetActive(false);
            runnerUI.SetActive(false);

            while (inGameData.winnerTeamID == "placeholder")
            {
                yield return null;
            }

            for (int i = 0; i < otherTeamCodes.Length; ++i)
            {
                Debug.Log("Looking at otherTeamCodes[" + i.ToString() + "]: " + otherTeamCodes[i]);
                if (otherTeamCodes[i] == inGameData.winnerTeamID)
                {
                    winTeamIndex = i;
                }
            }

            Debug.Log("Got WinIndex: " + winTeamIndex.ToString() + " API win teamcode: " + inGameData.winnerTeamID);

            otherTeamDistance[winTeamIndex] = inGameData.relay_totalsteps;

            //StartCoroutine(win_animation_player_movement_control(winTeamIndex));

            winTeamCode = otherTeamCodes[winTeamIndex];
            if (currentCameraIndex == 7)
            {
                cameras[currentCameraIndex].GetComponent<CinemachineVirtualCamera>().Priority = 0;
            }
            else
            {
                cameras[currentCameraIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
            }
            winCamera[winTeamIndex].GetComponent<CinemachineFreeLook>().Priority = 1;
            yield return new WaitForSecondsRealtime(2.0f);
            winCamera[winTeamIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
            winCameraFront[winTeamIndex].GetComponent<CinemachineFreeLook>().Priority = 1;
            yield return new WaitForSecondsRealtime(5.0f);

            for (int i = 0; i < PlayersArr.Length; ++i)
            {
                PlayersArr[i].SetActive(false);
            }
            Reset_Variables();
            LevelLoader.instance.loadScene("Relay Win Scene");
        }

        private IEnumerator win_animation_player_movement_control(int _winTeamIndex)
        {
            otherTeamVelocity[_winTeamIndex] = 1;

            while (otherTeamDistance[_winTeamIndex] <= (_TOTALSTEPS + _TOTALSTEPS * 0.1f))
            {
                otherTeamDistance[_winTeamIndex] += 1 * 0.1f;
                yield return new WaitForSecondsRealtime(5f);
                yield return null;
            }

        }

        public string getWinTeamCode()
        {
            return winTeamCode;
        }

        public void play_tutorial()
        {
            StartCoroutine(tutorial());
        }

        IEnumerator tutorial()
        {
            tap_anim = tap_UI.GetComponent<Animator>();
            tap_anim.SetTrigger("play_tutorial");
            yield return new WaitForSeconds(4.15f);
            activate_Utilities = true;
        }
    }
}




