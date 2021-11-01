using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Michsky.UI.ModernUIPack;
using TMPro;
using Cinemachine;
public class SpectatorSceneManager : MonoBehaviour
{
    public static SpectatorSceneManager instance;

    [Header("Relay Game Manager")]

    //[SerializeField] Text runnerText;

    [SerializeField] GameObject loadingUI;
    [SerializeField] GameObject[] maps;

    private Animator tap_anim;

    [SerializeField] GameObject[] PlayersArr;

    [Space]

    public string[] otherTeamNames = new string[5];
    public string[] otherTeamCodes = new string[5];
    public float[] otherTeamSteps = new float[5];
    public double[] otherTeamDistance = new double[5];
    public double[] otherTeamVelocity = new double[5];
    public int[] otherTeamSkinID = new int[5];
    public string[] otherTeamRunner = new string[5];
    public string[] otherTeamImageURL = new string[5];

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
    private bool write_data = true;
    private bool gameStart;
    private string[] camera_Names = { "Third Person View", "Third Person Side View", "Third Person Front View", "Finish Line Angled View", "Finish Line Ominious View", "Full Runway Ominious View", "Mid-Runway Ominious View", "Fly Camera View" };
    private int local_mapID;
    private bool quit;

    SpectatorData sdata;

    //Private Player Data
    private float totalSteps;
    private string teamCode;
    private int gameLocalVersion = -1;
    private int gameFBVersion = 0;

    private float multiplier_factor = 1;

    //Firebase Config Constants
    private int _TOTALSTEPS;
    private int _STEP_THRESHOLD;

    [Header("Timer")]
    //Timer Utils
    private float timer;
    private float personalBestTime;
    public bool isStartCountDown = false;
    public bool isStartGame = false;

    private void Reset_Variables()
    {
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
        gameStart = false;
        write_data = true;

        activate_Utilities = false;
        quit = false;

        personalBestTime = 100000f;
        gameFBVersion = 0;
        local_mapID = 0;
        gameLocalVersion = -1;
        winTeamIndex = 0;
        multiplier_factor = 1;
        currentCameraIndex = 7;
    }

    void Awake()
    {
        //Screen.orientation = ScreenOrientation.Portrait;

        instance = this;
        winTeamCode = "placeholder";
        Reset_Variables();

        for (int i = 0; i < maps.Length; ++i)
        {
            maps[i].SetActive(false);
        }

        maps[0].SetActive(true);
    }
    void Start()
    {
        StartCoroutine(initRelayGameData());
    }

    private IEnumerator initRelayGameData()
    {
        isStartGame = false;

        yield return new WaitForEndOfFrame();

        isStartGame = true;
        gameStart = true;

    }

    void Update()
    {
        //Debug.Log("myDistance: " + myDistance + " Modified: " + ((int)distance / _STEP_THRESHOLD).ToString() + "storedTurns: " + storedTurns.ToString());
        if (gameStart)
        {
            //Activation On: Detect there's a winner in this game
            if (sdata.gameEnd && !quit)
            {
                Debug.Log("Game Has Ended...Calculating Data");
                quit = true;
                StartCoroutine(play_Win_Animation());
            }

            if (apiNeedsUpdate)
            {
                StartCoroutine(update_API());
            }

            if (local_mapID != sdata.relay_map)
            {
                updateMap();
            }
        }
    }

    private IEnumerator update_API()
    {
        apiNeedsUpdate = false;

        yield return new WaitForEndOfFrame();

        API.instance.Update_Spectator_Data(SpectatorMenuManager.instance.getGameID());

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        string responseMessage = API.instance.responseMessage;
        long responseStatus = API.instance.statusCode;

        Debug.Log("Respose Return with code (" + responseStatus.ToString() + "): " + responseMessage);

        sdata = API.instance.getSpectatorData();

        yield return new WaitForSeconds(0.2f);

        if (!sdata.gameEnd)
        {
            update_Leaderboard();
        }

        apiNeedsUpdate = true;
    }

    public void updateMap()
    {
        for (int i = 0; i < maps.Length; ++i)
        {
            if (i == sdata.relay_map)
            {
                maps[i].SetActive(true);
            }
            else
            {
                maps[i].SetActive(false);
            }
        }

        local_mapID = sdata.relay_map;
    }

    //--------------------------------------------------------------------UI Buttons-----------------------------------------------------------------------------------
    public void leaveGame()
    {
        PlayerManager.instance.setGameStatus("placeholder", "placeholder");
        PlayerManager.instance.setTeamCode("placeholder");
        FirebaseManager.instance.clear_spectator_data();
        LevelLoader.instance.loadScene("Lobby");
    }

    //-----------------------------------------------------------------Leaderboard Code-----------------------------------------------------------------------------------

    private void update_Leaderboard()
    {
        FirebaseManager.instance.getSpectatorData(SpectatorMenuManager.instance.getGameID());

        otherTeamNames = FirebaseManager.instance.getOtherTeamNames();
        otherTeamCodes = FirebaseManager.instance.getOtherTeamCodes();
        otherTeamSteps = FirebaseManager.instance.getOtherTeamSteps();
        otherTeamDistance = FirebaseManager.instance.getOtherTeamDist();
        otherTeamVelocity = FirebaseManager.instance.getOtherTeamVelocity();
        otherTeamSkinID = FirebaseManager.instance.getOtherTeamSkinIDs();
        otherTeamRunner = FirebaseManager.instance.getOtherTeamRunners();
        otherTeamImageURL = FirebaseManager.instance.getOtherTeamImageURL();

        double max_dis = 0;
        int max_index = 0;

        for (int i = 0; i < otherTeamNames.Length; ++i)
        {
            if (otherTeamDistance[i] > max_dis)
            {
                max_dis = otherTeamDistance[i];
                max_index = i;
            }

            if (otherTeamNames[i] == "placeholder" || otherTeamNames[i] == null || otherTeamNames[i] == "")
            {
                PlayersArr[i].SetActive(false);
            }
            else
            {
                PlayersArr[i].SetActive(true);
            }
        }

        Debug.Log("Request to set resetPosIndex to: " + max_index);
        SpectatorCamera.instance.setResetPosIndex(max_index);

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

    //------------------------------------------------------Win Scene-------------------------------------------------------
    private string winTeamCode;
    private IEnumerator play_Win_Animation()
    {
        while (sdata.winnerTeamID == "placeholder")
        {
            yield return null;
        }

        for (int i = 0; i < otherTeamCodes.Length; ++i)
        {
            Debug.Log("Looking at otherTeamCodes[" + i.ToString() + "]: " + otherTeamCodes[i]);
            if (otherTeamCodes[i] == sdata.winnerTeamID)
            {
                winTeamIndex = i;
            }
        }

        Debug.Log("Got WinIndex: " + winTeamIndex.ToString() + " API win teamcode: " + sdata.winnerTeamID);

        //StartCoroutine(win_animation_player_movement_control(winTeamIndex));
        otherTeamDistance[winTeamIndex] = sdata.relay_totalsteps;

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
        LevelLoader.instance.loadScene("Spectator Win Scene");
    }

    private IEnumerator win_animation_player_movement_control(int _winTeamIndex)
    {
        otherTeamDistance[winTeamIndex] = sdata.relay_totalsteps;
        otherTeamVelocity[_winTeamIndex] = 1;

        while (otherTeamDistance[_winTeamIndex] <= (_TOTALSTEPS + _TOTALSTEPS * 0.1f))
        {
            otherTeamDistance[_winTeamIndex] += 1f * 0.3;
            yield return new WaitForSecondsRealtime(5f);
            yield return null;
        }

    }

    public string getWinTeamCode()
    {
        return winTeamCode;
    }

    public void play_tutorial(){
        StartCoroutine(tutorial());
    }

    IEnumerator tutorial()
    {
        yield return new WaitForSeconds(4.15f);
        activate_Utilities = true;
    }
}



