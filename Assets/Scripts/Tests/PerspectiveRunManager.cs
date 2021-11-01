using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PerspectiveRunManager : MonoBehaviour
{
    [Header("Scene Objects")]
    public InputField[] speed_Inputs;
    public GameObject[] playersOnScene;
    public InputField specifiedPlayer;
    public InputField specifiedSteps;
    public GameObject settingUI;
    public GameObject Relay_Baton_Anim;

    //Private Member Variables
    private float stepScaleFactor;
    private int[] playerSpeed;
    private int[] steps;
    private int[] relativeSteps;
    private bool startSim;
    private bool needsUpdate;
    private float STARTING_POS_Y;
    private float STARTING_POS_Y_TPP;
    private float[] STARTING_POS_X;
    private float[] STARTING_POS_X_TPP;
    private Animator[] playerAnimator;
    private string pov;
    private Animator anim;

    //Constants
    const float STANDARD_UNIT = 0.63625f;
    const float STANDARD_UNIT_TPP = 1f;
    const float DEGREE = 0.28318f;
    const float MAX_Y_POS = -79.7f;
    const float INIT_SCALE = 73f;
    const float LANE_SETOFF_1 = 1.10f;
    const float LANE_SETOFF_2 = 1.0f;
    const float LANE_SETOFF_3 = 0.9f;
    const float Y_POS_TPP_OFFSET = -180;

    void Start()
    {
        settingUI.SetActive(false);
        startSim = false;
        needsUpdate = true;
        pov = "third person";

        STARTING_POS_X = new float[7];
        STARTING_POS_X_TPP = new float[7];
        playerAnimator = new Animator[7];
        anim = Relay_Baton_Anim.GetComponent<Animator>();

        for (int i = 0; i < playersOnScene.Length; ++i)
        {
            STARTING_POS_Y = playersOnScene[i].transform.position.y;
            STARTING_POS_Y_TPP = STARTING_POS_Y + Y_POS_TPP_OFFSET;

            float _Delta_X;

            switch (i)
            {
                case 1:
                    _Delta_X = 1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE) * LANE_SETOFF_1;
                    break;
                case 2:
                    _Delta_X = -1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE) * LANE_SETOFF_1;
                    break;
                case 3:
                    _Delta_X = 1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 2) * LANE_SETOFF_2;
                    break;
                case 4:
                    _Delta_X = -1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 2) * LANE_SETOFF_2;
                    break;
                case 5:
                    _Delta_X = 1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 3) * LANE_SETOFF_3;
                    break;
                case 6:
                    _Delta_X = -1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 3) * LANE_SETOFF_3;
                    break;
                default:
                    _Delta_X = 0;
                    break;
            }

            

            STARTING_POS_X[i] = playersOnScene[i].transform.position.x;
            STARTING_POS_X_TPP[i] = STARTING_POS_X[i] + _Delta_X;
            playersOnScene[i].transform.position = new Vector3(playersOnScene[i].transform.position.x, STARTING_POS_Y, playersOnScene[i].transform.position.z);
            playersOnScene[i].transform.localScale = new Vector3(INIT_SCALE, INIT_SCALE, INIT_SCALE);
            playerAnimator[i] = playersOnScene[i].GetComponent<Animator>();
        }

        playerSpeed = new int[7];
        steps = new int[7];
        relativeSteps = new int[7];

        for (int i = 0; i < 7; ++i)
        {
            playerSpeed[i] = 0;
            steps[i] = 0;
            relativeSteps[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startSim && needsUpdate)
        {
            StartCoroutine(update_Coroutine());
        }
    }

    private IEnumerator update_Coroutine()
    {
        needsUpdate = false;
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < steps.Length; ++i)
        {
            steps[i] += playerSpeed[i];
        }
       

        for (int i = 1; i < relativeSteps.Length; i++)
        {
            relativeSteps[i] = steps[i] - steps[0];
        }

        int currTurn = steps[0] / 200;
        int mod_Steps = 0;

        if (pov == "third person")
        {
                
            for (int i = 0; i < playersOnScene.Length; i++)
            {
                playerAnimator[i].SetBool("front", true);
                mod_Steps = 200 - (steps[i] - currTurn * 200);
                Debug.Log("currTurn: " + currTurn + ", mod_Steps: " + mod_Steps);
                movePlayerTPP(i, mod_Steps);
            }

        }
        else
        {

            /*if (playerSpeed[0] >= 5)
            {
                playerAnimator[0].SetBool("front", false);
                playerAnimator[0].SetInteger("speed", 1);

            }
            else*/ if (playerSpeed[0] > 0)
            {
                playerAnimator[0].SetBool("front", false);
                playerAnimator[0].SetInteger("speed", 1);
                playerAnimator[0].speed = playerSpeed[0] * 0.333f;
            }
            else if (playerSpeed[1] == 0)
            {
                playerAnimator[0].SetBool("front", false);
                playerAnimator[0].SetInteger("speed", 0);
                playerAnimator[0].speed = 1;
            }

            for (int i = 1; i < playersOnScene.Length; i++)
            {
                playerAnimator[i].SetBool("front", false);
                movePlayerFPP(i);
            }
        }

        yield return new WaitForSecondsRealtime(0.2f);

        needsUpdate = true;
    }

    private void movePlayerFPP(int _playerID)
    {
        float Y_POS = STARTING_POS_Y;
        float X_POS = STARTING_POS_X[_playerID];
        float Delta_Y = 0;
        float Delta_X = 0;

        if (playerSpeed[_playerID] >= 5)
        {
            playerAnimator[_playerID].SetBool("front", false);
            playerAnimator[_playerID].SetInteger("speed", 2);

        }
        else if (playerSpeed[_playerID] > 0)
        {
            playerAnimator[_playerID].SetBool("front", false);
            playerAnimator[_playerID].SetInteger("speed", 1);
        }
        else if (playerSpeed[1] == 0)
        {
            playerAnimator[_playerID].SetBool("front", false);
            playerAnimator[_playerID].SetInteger("speed", 0);
        }
        

        if (relativeSteps[_playerID] < 50)
        {
            stepScaleFactor = 4;
            Delta_Y = stepScaleFactor * relativeSteps[_playerID] * STANDARD_UNIT;
            Y_POS += Delta_Y;
        }
        else if (relativeSteps[_playerID] < 100)
        {
            stepScaleFactor = 2;
            Delta_Y = (50 * 4 * STANDARD_UNIT + (relativeSteps[_playerID] - 50) * stepScaleFactor * STANDARD_UNIT);
            Y_POS += Delta_Y;
        }
        else
        {
            stepScaleFactor = 1;
            Delta_Y = (50 * 4 * STANDARD_UNIT + 50 * 2 * STANDARD_UNIT + (relativeSteps[_playerID] - 100) * stepScaleFactor * STANDARD_UNIT);
            Y_POS += Delta_Y;
        }

        switch (_playerID) {
            case 1:
                Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_SETOFF_1;
                break;
            case 2:
                Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_SETOFF_1;
                break;
            case 3:
                Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_SETOFF_2;
                break;
            case 4:
                Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_SETOFF_2;
                break;
            case 5:
                Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_SETOFF_3;
                break;
            case 6:
                Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_SETOFF_3;
                break;
            default:
                Delta_X = 0;
                break;
        }

        X_POS += Delta_X;
        float NEW_SCALE = (((465.71f - Y_POS) * INIT_SCALE) / (465.71f - STARTING_POS_Y));
        Debug.Log("Y_POS = " + Y_POS + ", New Scale = " + ((465.71f - Y_POS)).ToString()+ " / " + (465.71f - STARTING_POS_Y).ToString() + " * " + INIT_SCALE.ToString() + " = " + (((465.71f - Y_POS) * INIT_SCALE) / (465.71f - STARTING_POS_Y)).ToString());
        playersOnScene[_playerID].transform.position = new Vector3(X_POS, Y_POS, playersOnScene[_playerID].transform.position.z);
        playersOnScene[_playerID].transform.localScale = new Vector3(NEW_SCALE, NEW_SCALE, NEW_SCALE);
        //move(playersOnScene[_playerID], new Vector3(X_POS, Y_POS, playersOnScene[_playerID].transform.position.z), NEW_SCALE);

        if (Y_POS >= 350 || Y_POS <= -96)
        {
            playerAnimator[_playerID].SetBool("front", false);
            playersOnScene[_playerID].SetActive(false);
        }
        else
        {
            playerAnimator[_playerID].SetBool("front", false);
            playersOnScene[_playerID].SetActive(true);
        }
    }

    private void movePlayerTPP(int _playerID, int _mod_Steps)
    {
        float Y_POS = STARTING_POS_Y_TPP;
        float X_POS = STARTING_POS_X_TPP[_playerID];
        float Delta_Y = 0;
        float Delta_X = 0;

        if (playerSpeed[_playerID] >= 5)
        {
            playerAnimator[_playerID].SetBool("front", true);
            playerAnimator[_playerID].SetInteger("speed", 2);

        }
        else if (playerSpeed[_playerID] > 0)
        {
            playerAnimator[_playerID].SetBool("front", true);
            playerAnimator[_playerID].SetInteger("speed", 1);
        }
        else if (playerSpeed[1] == 0)
        {
            playerAnimator[_playerID].SetBool("front", true);
            playerAnimator[_playerID].SetInteger("speed", 0);
        }

        if (_mod_Steps < 50)
        {
            stepScaleFactor = 4;
            Delta_Y = stepScaleFactor * _mod_Steps * STANDARD_UNIT_TPP;
            Y_POS += Delta_Y;
        }
        else if (_mod_Steps < 100)
        {
            stepScaleFactor = 2;
            Delta_Y = (50 * 4 * STANDARD_UNIT_TPP + (_mod_Steps - 50) * stepScaleFactor * STANDARD_UNIT_TPP);
            Y_POS += Delta_Y;
        }
        else
        {
            stepScaleFactor = 1;
            Delta_Y = (50 * 4 * STANDARD_UNIT_TPP + 50 * 2 * STANDARD_UNIT_TPP + (_mod_Steps - 100) * stepScaleFactor * STANDARD_UNIT_TPP);
            Y_POS += Delta_Y;
        }

        switch (_playerID)
        {
            case 1:
                Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_SETOFF_1;
                break;
            case 2:
                Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_SETOFF_1;
                break;
            case 3:
                Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_SETOFF_2;
                break;
            case 4:
                Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_SETOFF_2;
                break;
            case 5:
                Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_SETOFF_3;
                break;
            case 6:
                Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_SETOFF_3;
                break;
            default:
                Delta_X = 0;
                break;
        }

        X_POS += Delta_X;
        float NEW_SCALE = (((465.71f - Y_POS) * INIT_SCALE) / (465.71f - STARTING_POS_Y));
        Debug.Log("Y_POS = " + Y_POS + ", New Scale = " + ((465.71f - Y_POS)).ToString() + " / " + (465.71f - STARTING_POS_Y).ToString() + " * " + INIT_SCALE.ToString() + " = " + (((465.71f - Y_POS) * INIT_SCALE) / (465.71f - STARTING_POS_Y)).ToString());
        playersOnScene[_playerID].transform.position = new Vector3(X_POS, Y_POS, playersOnScene[_playerID].transform.position.z);
        playersOnScene[_playerID].transform.localScale = new Vector3(NEW_SCALE, NEW_SCALE, NEW_SCALE);

        if (Y_POS >= 350 || Y_POS <= -96)
        {
            playerAnimator[_playerID].SetBool("front", true);
            playersOnScene[_playerID].SetActive(false);
        }
        else
        {
            playerAnimator[_playerID].SetBool("front", true);
            playersOnScene[_playerID].SetActive(true);
        }
    }
    public void setting_Button()
    {
        settingUI.SetActive(true);
    }

    public void close_Button()
    {
        settingUI.SetActive(false);
    }

    public void start_Button()
    {
        startSim = true;
    }

    public void pause_Button()
    {
        anim.SetTrigger("play");
        startSim = false;
    }

    public void reset_Button()
    {
        settingUI.SetActive(false);
        startSim = false;
        needsUpdate = true;
        for (int i = 0; i < playersOnScene.Length; ++i)
        {
            playersOnScene[i].transform.position = new Vector3(STARTING_POS_X[i], STARTING_POS_Y, playersOnScene[i].transform.position.z);
            playersOnScene[i].transform.localScale = new Vector3(INIT_SCALE, INIT_SCALE, INIT_SCALE);
        }

        playerSpeed = new int[7];
        steps = new int[7];

        for (int i = 0; i < 7; ++i)
        {
            playerSpeed[i] = 0;
            steps[i] = 0;
        }
    }

    public void player1_SetSpeed()
    {
        playerSpeed[0] = int.Parse(speed_Inputs[0].text);
    }

    public void player2_SetSpeed()
    {
        playerSpeed[1] = int.Parse(speed_Inputs[1].text);
    }
    public void player3_SetSpeed()
    {
        playerSpeed[2] = int.Parse(speed_Inputs[2].text);
    }
    public void player4_SetSpeed()
    {
        playerSpeed[3] = int.Parse(speed_Inputs[3].text);
    }
    public void player5_SetSpeed()
    {
        playerSpeed[4] = int.Parse(speed_Inputs[4].text);
    }
    public void player6_SetSpeed()
    {
        playerSpeed[5] = int.Parse(speed_Inputs[5].text);
    }
    public void player7_SetSpeed()
    {
        playerSpeed[6] = int.Parse(speed_Inputs[6].text);
    }

    public void player_set_Steps()
    {
        int _playerID = int.Parse( specifiedPlayer.text);
        int _steps = int.Parse(specifiedSteps.text);

        steps[_playerID] = _steps;
    }

    public void chang_POV_Button()
    {
        if (pov == "third person")
        {
            Debug.Log("Changing POV to First Person");
            pov = "first person";
            playersOnScene[0].SetActive(true);
            playerAnimator[0].SetBool("front", false);
            playersOnScene[0].transform.position = new Vector3(STARTING_POS_X[0], STARTING_POS_Y, playersOnScene[0].transform.position.z);
            playersOnScene[0].transform.localScale = new Vector3(INIT_SCALE, INIT_SCALE, INIT_SCALE);
        }
        else
        {
            Debug.Log("Changing POV to Third Person");
            pov = "third person";
        }
    }

    private IEnumerator move(GameObject gameObject, Vector3 targetPos, float NEW_SCALE)
    {
        while (gameObject.transform.position != targetPos)
        {
            Vector3.Lerp(gameObject.transform.position, targetPos, 10f);
        }
        gameObject.transform.localScale = new Vector3(NEW_SCALE, NEW_SCALE, NEW_SCALE);
        yield return new WaitForEndOfFrame();
    }
}
