using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class RelayRunwayManager : MonoBehaviour
    {
        [Header("Scene Objects")]
        public GameObject[] playersOnScene;
        public GameObject relayRunway;
        public Transform startPos_Y;
        public Transform endPos_Y;
        public Transform summitPos_Y;

        //Private Member Variables
        private float stepScaleFactor;
        private string[] teamNames;
        private double[] prevSteps;
        private double[] distances;
        private double[] speed;
        private double[] relativeDistances;
        private bool needsUpdate;
        private bool animNeedsUpdate;
        private bool apiNeedsUpdate;
        private bool isMyTurn;
        private float STARTING_POS_Y;
        private float STARTING_POS_Y_TPP;
        private float[] STARTING_POS_X;
        private float[] STARTING_POS_X_TPP;
        private Animator[] playerAnimator;
        private Animator relayRunwayAnim;
        private string pov;
        private string gameID;
        private string teamCode;

        TeamData teamData;
        InGameData inGameData;

        int localVersion = -1;
        int FBVersion = 0;

        //Constants
        const float DEGREE = 0.28318f;
        const float MAX_Y_POS = -79.7f;
        const float INIT_SCALE = 13f;
        const float LANE_OFFSET_1 = 1.10f;
        const float LANE_OFFSET_2 = 1.0f;
        const float LANE_OFFSET_3 = 0.9f;
        const float Y_POS_TPP_OFFSET = -180;
        public int _TOTAL_STEPS;
        public int _STEPS_THRESHOLD;

        public float STANDARD_UNIT = 0.63625f;
        public float STANDARD_UNIT_TPP = 1f;
        public float DYNAMIC_SUNIT;
        public float DYNAMIC_SUNIT_TPP;
        public float FULL_LENGTH;

        void Start()
        {
            teamData = API.instance.getTeamData();
            inGameData = API.instance.getInGameData();
            localVersion = -1;
            FBVersion = 0;

            pov = "third person";

            relayRunwayAnim = relayRunway.GetComponent<Animator>();

            STARTING_POS_X = new float[5];
            STARTING_POS_X_TPP = new float[5];
            playerAnimator = new Animator[5];

            gameID = teamData.gameID;
            teamCode = teamData.teamCode;
            _TOTAL_STEPS = inGameData.relay_totalsteps;
            _STEPS_THRESHOLD = inGameData.relay_steps_threshold;

            STANDARD_UNIT = (startPos_Y.position.y - endPos_Y.position.y) / (_STEPS_THRESHOLD * 2);
            //Debug.Log("Lenth of Runway: " + (startPos_Y.position.y - endPos_Y.position.y).ToString() + ", STEP_THRESHOLD: " + _STEPS_THRESHOLD + ", STANDARD_UNIT: " + STANDARD_UNIT);
            DYNAMIC_SUNIT = (STANDARD_UNIT * 200) / _STEPS_THRESHOLD;
            DYNAMIC_SUNIT_TPP = (STANDARD_UNIT_TPP * 200) / _STEPS_THRESHOLD;

            FULL_LENGTH = (summitPos_Y.position.y - endPos_Y.position.y) * 1.55f;

            for (int i = 0; i < playersOnScene.Length; ++i)
            {
                STARTING_POS_Y = playersOnScene[i].transform.position.y;
                STARTING_POS_Y_TPP = STARTING_POS_Y + Y_POS_TPP_OFFSET;

                float _Delta_X;

                switch (i)
                {
                    case 1:
                        _Delta_X = 1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                        break;
                    case 2:
                        _Delta_X = -1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                        break;
                    case 3:
                        _Delta_X = 1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                        break;
                    case 4:
                        _Delta_X = -1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                        break;
                    case 5:
                        _Delta_X = 1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                        break;
                    case 6:
                        _Delta_X = -1 * Y_POS_TPP_OFFSET * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
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

                needsUpdate = true;
                animNeedsUpdate = true;
                apiNeedsUpdate = true;
            }

            teamNames = new string[5];
            prevSteps = new double[5];
            distances = new double[5];
            speed = new double[5];
            relativeDistances = new double[5];

            for (int i = 0; i < 5; ++i)
            {
                distances[i] = 0;
                relativeDistances[i] = 0;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (needsUpdate)
            {
                StartCoroutine(update_Coroutine());
            }

            if (animNeedsUpdate)
            {
                StartCoroutine(animation_update_Coroutine());
            }

        }


        private IEnumerator animation_update_Coroutine()
        {
            animNeedsUpdate = false;
            yield return new WaitForEndOfFrame();

            speed = RelayGameManager.instance.getOtherTeamVelocity();

            prevSteps = distances;

            if (pov == "third person")
            {
                for (int _playerID = 0; _playerID < playersOnScene.Length; _playerID++)
                {
                    if (speed[_playerID] > 4 || speed[_playerID] < 4)
                    {
                        playerAnimator[_playerID].SetBool("front", true);
                        playerAnimator[_playerID].SetInteger("speed", 1);
                        playerAnimator[_playerID].speed = (float)speed[_playerID] * 0.8f;
                    }
                    else
                    {
                        playerAnimator[_playerID].SetBool("front", true);
                        playerAnimator[_playerID].SetInteger("speed", 0);
                        playerAnimator[_playerID].speed = (float)speed[_playerID] * 1f;
                    }
                }
            }
            else
            {

                for (int _playerID = 0; _playerID < playersOnScene.Length; _playerID++)
                {
                    if (speed[_playerID] > 4 || speed[_playerID] < 4)
                    {
                        playerAnimator[_playerID].SetBool("front", false);
                        playerAnimator[_playerID].SetInteger("speed", 1);
                        playerAnimator[_playerID].speed = (float)speed[_playerID] * 0.8f;
                    }
                    else
                    {
                        playerAnimator[_playerID].SetBool("front", false);
                        playerAnimator[_playerID].SetInteger("speed", 0);
                        playerAnimator[_playerID].speed = (float)speed[_playerID] * 1f;
                    }
                }
            }


            if (distances[0] < _STEPS_THRESHOLD && pov == "third person")
            {
                relayRunwayAnim.speed = 0;
            }
            else
            {
                relayRunwayAnim.speed = (float)speed[0] * 0.6f;
            }

            yield return new WaitForSecondsRealtime(0.5f);
            animNeedsUpdate = true;
        }

        private IEnumerator update_Coroutine()
        {
            needsUpdate = false;
            yield return new WaitForEndOfFrame();

            teamData = API.instance.getTeamData();
            inGameData = API.instance.getInGameData();

            teamNames = RelayGameManager.instance.getOtherTeamNames();
            distances = RelayGameManager.instance.getOtherTeamDistance();

            check_POV();

            int currTurn = (int)distances[0] / _STEPS_THRESHOLD;
            float mod_Steps = 200;

            for (int i = 1; i < relativeDistances.Length; i++)
            {
                relativeDistances[i] = distances[i] - distances[0];
            }

            if (pov == "third person")
            {
                double mainCharacterSteps = _STEPS_THRESHOLD - (distances[0] - currTurn * _STEPS_THRESHOLD);

                //Debug.Log("Main Character Steps Modified: " + mainCharacterSteps +  ", First Person Logic: " + (mainCharacterSteps <= _STEPS_THRESHOLD * 0.25f).ToString());
                //mainCharacterSteps <= _STEPS_THRESHOLD * 0.25f
                if (distances[0] >= _STEPS_THRESHOLD)
                {
                    for (int i = 1; i < playersOnScene.Length; i++)
                    {
                        playerAnimator[i].SetBool("front", true);
                        movePlayerFPP_Front(i);
                    }
                }
                else
                {
                    for (int i = 0; i < playersOnScene.Length; i++)
                    {
                        playerAnimator[i].SetBool("front", true);
                        mod_Steps = _STEPS_THRESHOLD - ((float)distances[i] - currTurn * _STEPS_THRESHOLD);
                        //Debug.Log("currTurn: " + currTurn + ", mod_Steps: " + mod_Steps);
                        movePlayerTPP_Front(i, mod_Steps);
                    }
                }
            }
            else
            {
                for (int i = 1; i < playersOnScene.Length; i++)
                {
                    playerAnimator[i].SetBool("front", false);
                    movePlayerFPP(i);
                }
            }
            needsUpdate = true;
        }

        private void movePlayerFPP(int _playerID)
        {
            float Y_POS = STARTING_POS_Y;
            float X_POS = STARTING_POS_X[_playerID];
            float Delta_Y = 0;
            float Delta_X = 0;

            if (relativeDistances[_playerID] < _STEPS_THRESHOLD * 0.25f)
            {
                stepScaleFactor = 4;
                Delta_Y = stepScaleFactor * (float)relativeDistances[_playerID] * STANDARD_UNIT;
                Y_POS += Delta_Y;
            }
            else if (relativeDistances[_playerID] < _STEPS_THRESHOLD * 0.5f)
            {
                stepScaleFactor = 2;
                Delta_Y = ((_STEPS_THRESHOLD * 0.25f) * 4 * STANDARD_UNIT + ((float)relativeDistances[_playerID] - (_STEPS_THRESHOLD * 0.25f)) * stepScaleFactor * STANDARD_UNIT);
                Y_POS += Delta_Y;
            }
            else
            {
                stepScaleFactor = 1;
                Delta_Y = ((_STEPS_THRESHOLD * 0.25f) * 4 * STANDARD_UNIT + (_STEPS_THRESHOLD * 0.25f) * 2 * STANDARD_UNIT + ((float)relativeDistances[_playerID] - (_STEPS_THRESHOLD * 0.5f)) * stepScaleFactor * STANDARD_UNIT);
                Y_POS += Delta_Y;
            }

            switch (_playerID)
            {
                case 1:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                    break;
                case 2:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                    break;
                case 3:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                    break;
                case 4:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                    break;
                case 5:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                    break;
                case 6:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                    break;
                default:
                    Delta_X = 0;
                    break;
            }

            X_POS += Delta_X;
            float NEW_SCALE = (((FULL_LENGTH - Y_POS) * INIT_SCALE) / (FULL_LENGTH - STARTING_POS_Y));
            //Debug.Log("FULL_LENGTH: " + FULL_LENGTH + ", Y_POS = " + Y_POS + ", New Scale = " + ((FULL_LENGTH - Y_POS)).ToString() + " / " + (FULL_LENGTH - STARTING_POS_Y).ToString() + " * " + INIT_SCALE.ToString() + " = " + (((FULL_LENGTH - Y_POS) * INIT_SCALE) / (FULL_LENGTH - STARTING_POS_Y)).ToString());
            playersOnScene[_playerID].transform.position = new Vector3(X_POS, Y_POS, playersOnScene[_playerID].transform.position.z);
            playersOnScene[_playerID].transform.localScale = new Vector3(NEW_SCALE, NEW_SCALE, NEW_SCALE);

            if (Y_POS >= 340 || Y_POS <= -96 || teamNames[_playerID] == "placeholder" || teamNames[_playerID] == null)
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

        private void movePlayerFPP_Front(int _playerID)
        {
            float Y_POS = STARTING_POS_Y;
            float X_POS = STARTING_POS_X[_playerID];
            float Delta_Y = 0;
            float Delta_X = 0;

            float[] _modSteps = new float[relativeDistances.Length];

            for (int i = 0; i < _modSteps.Length; ++i)
            {
                _modSteps[i] = _STEPS_THRESHOLD - (float)relativeDistances[i];
            }

            if (_modSteps[_playerID] < (_STEPS_THRESHOLD * 0.25f))
            {
                stepScaleFactor = 4;
                Delta_Y = stepScaleFactor * _modSteps[_playerID] * STANDARD_UNIT;
                Y_POS += Delta_Y;
            }
            else if (_modSteps[_playerID] < (_STEPS_THRESHOLD * 0.5f))
            {
                stepScaleFactor = 2;
                Delta_Y = ((_STEPS_THRESHOLD * 0.25f) * 4 * STANDARD_UNIT + (_modSteps[_playerID] - (_STEPS_THRESHOLD * 0.25f)) * stepScaleFactor * STANDARD_UNIT);
                Y_POS += Delta_Y;
            }
            else
            {
                stepScaleFactor = 1;
                Delta_Y = ((_STEPS_THRESHOLD * 0.25f) * 4 * STANDARD_UNIT + (_STEPS_THRESHOLD * 0.25f) * 2 * STANDARD_UNIT + (_modSteps[_playerID] - (_STEPS_THRESHOLD * 0.5f)) * stepScaleFactor * STANDARD_UNIT);
                Y_POS += Delta_Y;
            }

            switch (_playerID)
            {
                case 1:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                    break;
                case 2:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                    break;
                case 3:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                    break;
                case 4:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                    break;
                case 5:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                    break;
                case 6:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                    break;
                default:
                    Delta_X = 0;
                    break;
            }

            X_POS += Delta_X;
            float NEW_SCALE = (((FULL_LENGTH - Y_POS) * INIT_SCALE) / (FULL_LENGTH - STARTING_POS_Y));
            //Debug.Log("Y_POS = " + Y_POS + ", New Scale = " + ((465.71f - Y_POS)).ToString() + " / " + (465.71f - STARTING_POS_Y).ToString() + " * " + INIT_SCALE.ToString() + " = " + (((465.71f - Y_POS) * INIT_SCALE) / (465.71f - STARTING_POS_Y)).ToString());
            playersOnScene[_playerID].transform.position = new Vector3(X_POS, Y_POS, playersOnScene[_playerID].transform.position.z);
            playersOnScene[_playerID].transform.localScale = new Vector3(NEW_SCALE, NEW_SCALE, NEW_SCALE);

            if (Y_POS >= 340 || Y_POS <= -96 || teamNames[_playerID] == "placeholder" || teamNames[_playerID] == null)
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

        private void movePlayerTPP_Front(int _playerID, float _mod_Steps)
        {
            float Y_POS = STARTING_POS_Y;
            float X_POS = STARTING_POS_X[_playerID];
            float Delta_Y = 0;
            float Delta_X = 0;



            if (_mod_Steps < (_STEPS_THRESHOLD * 0.25f))
            {
                stepScaleFactor = 4;
                Delta_Y = stepScaleFactor * _mod_Steps * STANDARD_UNIT;
                Y_POS += Delta_Y;
            }
            else if (_mod_Steps < (_STEPS_THRESHOLD * 0.5f))
            {
                stepScaleFactor = 2;
                Delta_Y = ((_STEPS_THRESHOLD * 0.25f) * 4 * STANDARD_UNIT + (_mod_Steps - (_STEPS_THRESHOLD * 0.25f)) * stepScaleFactor * STANDARD_UNIT);
                Y_POS += Delta_Y;
            }
            else
            {
                stepScaleFactor = 1;
                Delta_Y = ((_STEPS_THRESHOLD * 0.25f) * 4 * STANDARD_UNIT + (_STEPS_THRESHOLD * 0.25f) * 2 * STANDARD_UNIT + (_mod_Steps - (_STEPS_THRESHOLD * 0.5f)) * stepScaleFactor * STANDARD_UNIT);
                Y_POS += Delta_Y;
            }

            switch (_playerID)
            {
                case 1:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                    break;
                case 2:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE) * LANE_OFFSET_1;
                    break;
                case 3:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                    break;
                case 4:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 2) * LANE_OFFSET_2;
                    break;
                case 5:
                    Delta_X = 1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                    break;
                case 6:
                    Delta_X = -1 * Delta_Y * Mathf.Tan(DEGREE * 3) * LANE_OFFSET_3;
                    break;
                default:
                    Delta_X = 0;
                    break;
            }

            X_POS += Delta_X;
            float NEW_SCALE = (((FULL_LENGTH - Y_POS) * INIT_SCALE) / (FULL_LENGTH - STARTING_POS_Y));
            //Debug.Log("Y_POS = " + Y_POS + ", New Scale = " + ((465.71f - Y_POS)).ToString() + " / " + (465.71f - STARTING_POS_Y_TPP).ToString() + " * " + INIT_SCALE.ToString() + " = " + (((465.71f - Y_POS) * INIT_SCALE) / (465.71f - STARTING_POS_Y)).ToString());
            playersOnScene[_playerID].transform.position = new Vector3(X_POS, Y_POS, playersOnScene[_playerID].transform.position.z);
            playersOnScene[_playerID].transform.localScale = new Vector3(NEW_SCALE, NEW_SCALE, NEW_SCALE);

            if (Y_POS >= 340 || Y_POS <= -96 || teamNames[_playerID] == "placeholder" || teamNames[_playerID] == null)
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

        public void check_POV()
        {
            isMyTurn = RelayGameManager.instance.getIsMyTurn();
            if (isMyTurn)
            {
                // Debug.Log("Changing POV to First Person");
                pov = "first person";
                playersOnScene[0].SetActive(true);
                playerAnimator[0].SetBool("front", false);
                playersOnScene[0].transform.position = new Vector3(STARTING_POS_X[0], STARTING_POS_Y, playersOnScene[0].transform.position.z);
                playersOnScene[0].transform.localScale = new Vector3(INIT_SCALE, INIT_SCALE, INIT_SCALE);
            }
            else
            {
                // Debug.Log("Changing POV to Third Person");
                pov = "third person";
            }
        }
    }
}

