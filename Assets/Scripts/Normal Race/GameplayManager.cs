using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Managers
{

    public class GameplayManager : MonoBehaviour
    {

        [Header("Gameplay Utilities")]
        public TMP_Text stepsText;
        public Text statusText;
        public Text walkingFreqText;
        public Text warningText;
        public GameObject character1;
        public GameObject otherCharacter1;
        public GameObject otherCharacter2;
        public Sprite[] skins;

        private int clicks = 0;

        private bool anyWins;
        private string winPlayerUsername;
        private string[] inGameUsersUID;
        private string gameID;
        private string playerUID;

        private bool finishUpdate;
        private bool finishUpdateSkin;

        private int skinID1;
        private int skinID2;

        //Step Count Variables
        public float lowLimit = 0.005f; //平缓
        public float highLimit = 0.1f; // 走路时的波峰波谷
        public float vertHighLimit = 0.25f;//跳跃时的波峰波谷
        private bool isHigh = false; // 状态
        private float filterCurrent = 10.0f; // 滤波参数 得到拟合值
        private float filterAverage = 0.1f; //   滤波参数  得到均值
        private float accelerationCurrent = 0f; //拟合值
        private float accelerationAverage = 0f;//均值
        private int steps = 0; // 步数
        private int oldSteps;
        private float deltaTime = 0f;//计时器
        private int jumpCount = 0;//跳跃数
        private int oldjumpCount = 0;

        //Check Cheating Variables
        private bool isCheating = false;
        private bool isHigh_CC = false; // 状态
        private float filterCurrent_CC = 10.0f; // 滤波参数 得到拟合值
        private float filterAverage_CC = 0.1f; //   滤波参数  得到均值
        private float accelerationCurrent_CC = 0f; //拟合值
        private float accelerationAverage_CC = 0f;//均值
        private int steps_CC = 0; // 步数
        private float refreshRate = 0f;
        private float gameTime = 0;
        private int warningCount = 0;
        private int over15hz = 0;

        private bool startTimer = false;//开始计时
        private bool isWalking = false;
        private bool isJumping = false;

        void Awake()
        {
            //Screen.orientation = ScreenOrientation.Portrait;
            accelerationAverage = Input.acceleration.magnitude;
            oldSteps = steps;
            oldjumpCount = jumpCount;
            refreshRate = 0;
            steps_CC = 0;
            warningText.text = "";
        }

        void FixedUpdate()
        {
            //通过Lerp对Input.acceleration.magnitude(加速度标量和)滤波
            //这里使用线性插值的公式正好为EMA一次指数滤波 y[i]=y[i-1]+(x[i]-y[i])*k=(1-k)*y[i]+kx[i]
            accelerationCurrent = Mathf.Lerp(accelerationCurrent, Input.acceleration.magnitude, Time.deltaTime * filterCurrent);
            accelerationAverage = Mathf.Lerp(accelerationAverage, Input.acceleration.magnitude, Time.deltaTime * filterAverage);
            float delta = accelerationCurrent - accelerationAverage; // 获取差值，即坡度

            if (!isHigh)
            {
                if (delta > DebugUIController.instance.walking_threshold)//往高
                {
                    isHigh = true;
                    if (isCheating)
                    {
                        isCheating = false;
                        StartCoroutine(warningDisplay());
                    }
                    else if (delta < DebugUIController.instance.warning_threshold)
                    {
                        steps++;

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

        IEnumerator warningDisplay()
        {
            warningText.text = "";
            warningCount++;
            if (warningCount >= 3)
            {
                warningCount = 0;
                warningText.text = "Penalty: -50 steps";
                if (steps - 50 > 0)
                {
                    steps -= 50;
                }
                else
                {
                    steps = 0;
                }
            }
            else
            {
                warningText.text = "You are cheating, please walk.\nWarning " + warningCount + "/3 before Penalty: -50 steps";
            }

            yield return new WaitForSeconds(1.5f);
            warningText.text = "";
        }

        private void checkWalkingAndJumping()
        {
            if ((steps != oldSteps) || (oldjumpCount != jumpCount))
            {
                startTimer = true;
                deltaTime = 0f;
            }

            if (startTimer)
            {
                deltaTime += Time.deltaTime;

                if (deltaTime != 0)
                {
                    if (oldjumpCount != jumpCount)//检测是否是跳跃
                        isJumping = true;
                    else
                        isWalking = true;

                }
                if (deltaTime > 2)
                {
                    deltaTime = 0F;
                    startTimer = false;
                }
            }
            else if (!startTimer)
            {
                isWalking = false;
                isJumping = false;
            }
            oldSteps = steps;
            oldjumpCount = jumpCount;
        }

        public void tapToRun()
        {

            steps += 1;
            //Debug.Log("Tap To Run button pressed, Current clicks = " + clicks);
            stepsText.text = "Steps x " + steps;
            character1.transform.position = new Vector3(character1.transform.position.x, character1.transform.position.y + 1, character1.transform.position.z);
            //FirebaseManager.instance.setData(false, true, false, 0, clicks);
            FirebaseManager.instance.setNormalSteps(gameID, playerUID, steps);

            if (steps >= 250)
            {
                FirebaseManager.instance.winPlayerUsername = PlayerManager.instance.getData("username");
                //Debug.Log("Winning Player Name Updated: " + winPlayerUsername);
                //FirebaseManager.instance.setData(false, true, true, 300, 0);
                PlayerManager.instance.addCoins("300");
                FirebaseManager.instance.resetSkinArr();
                anyWins = true;
            }
        }

        public void exit_Button()
        {
            //FirebaseManager.instance.resetSkinArr();
            //PlayerManager.instance.setGameStatus("Race", "placeholder");
            //SceneManager.LoadScene("Lobby");
        }

        private int updateOtherPlayer(int i)
        {
            int local_steps = 0;
            local_steps = FirebaseManager.instance.getStepsByUID(inGameUsersUID[i]);
            return local_steps;
        }

        // Start is called before the first frame update
        void Start()
        {
            finishUpdate = true;
            finishUpdateSkin = true;
            anyWins = false;
            winPlayerUsername = "";

            FirebaseManager.instance.resetSkinArr();
            inGameUsersUID = FirebaseManager.instance.getOtherPlayerArray();
            playerUID = PlayerManager.instance.getData("uid");
            gameID = FirebaseManager.instance.getGameIDForNormalRace();

            int skinID = int.Parse(PlayerManager.instance.getData("skinID"));
            character1.AddComponent<SpriteRenderer>();
            character1.GetComponent<SpriteRenderer>().sprite = skins[skinID];

            PlayerManager.instance.setGameStatus("Race", "In Game");
        }

        void Update()
        {
            stepsText.text = "Steps x " + steps;
            character1.transform.position = new Vector3(character1.transform.position.x, 87.7f + steps, character1.transform.position.z);
            FirebaseManager.instance.setNormalSteps(gameID, playerUID, steps);

            checkWalkingAndJumping(); // 检测是否行走

            if (gameTime >= 10.0f)
            {
                check_Cheating();
                walkingFreqText.text = "Hz: " + (steps_CC / refreshRate).ToString() + " CC Steps: " + steps_CC.ToString();
                if (steps_CC / refreshRate >= DebugUIController.instance.warning_count_threshold)
                {
                    over15hz++;
                }
            }
            else
            {
                walkingFreqText.text = "Calibrating...";
            }


            refreshRate += Time.deltaTime;
            gameTime += Time.deltaTime;

            if (over15hz >= DebugUIController.instance.warning_threshold)
            {
                isCheating = true;
                over15hz = 0;
            }

            if (refreshRate > DebugUIController.instance.refresh_rate)
            {
                refreshRate = 0;
                steps_CC = 0;
                over15hz = 0;
            }

            if (isWalking)
            {
                statusText.text = ("Status: Running");

            }
            else if (!isWalking)
            {
                statusText.text = ("Status: Idle");
            }

            if (isJumping)
            {
                statusText.text = ("Status: Jumping");
            }

            if (finishUpdate)
            {
                finishUpdate = false;
                StartCoroutine(getOtherPlayerData());
            }

            if (finishUpdateSkin)
            {
                finishUpdateSkin = false;
                //StartCoroutine(updateOtherPlayerSkin());
            }

            if (anyWins)
            {
                PlayerManager.instance.setGameStatus("Race", "placeholder");
                SceneManager.LoadScene("Win Scene");
            }

            if (steps >= 250)
            {
                FirebaseManager.instance.winPlayerUsername = PlayerManager.instance.getData("username");
                Debug.Log("Winning Player Name Updated: " + winPlayerUsername);
                PlayerManager.instance.addCoins("300");
                FirebaseManager.instance.resetSkinArr();
                anyWins = true;
            }
        }

        private IEnumerator getOtherPlayerData()
        {

            yield return new WaitForSeconds(0.05f);

            FirebaseManager.instance.getNormalSteps(gameID);

            yield return new WaitForSeconds(1f);

            int steps_1 = updateOtherPlayer(0);
            //Debug.Log("Step_1 set to: " + steps_1);
            otherCharacter1.transform.position = new Vector3(otherCharacter1.transform.position.x, 87.7f + steps_1, otherCharacter1.transform.position.z);
            if (steps_1 >= 250)
            {
                //Debug.Log(inGameUsersUID[0] + "(otherCharacter1) wins!");
                FirebaseManager.instance.setWinnerUsername(inGameUsersUID[0]);
                FirebaseManager.instance.winPlayerSkinID = skinID1;
                FirebaseManager.instance.resetSkinArr();
                yield return new WaitForSeconds(1.0f);
                anyWins = true;
            }
            yield return new WaitForSeconds(0.05f);


            int steps_2 = updateOtherPlayer(1);
            //Debug.Log("Step_2 set to: " + steps_2);
            otherCharacter2.transform.position = new Vector3(otherCharacter2.transform.position.x, 87.7f + steps_2, otherCharacter2.transform.position.z);
            if (steps_2 >= 250)
            {
                //Debug.Log(inGameUsersUID[1] + "(otherCharacter2) wins!");
                FirebaseManager.instance.setWinnerUsername(inGameUsersUID[1]);
                FirebaseManager.instance.winPlayerSkinID = skinID2;
                FirebaseManager.instance.resetSkinArr();
                yield return new WaitForSeconds(1.0f);
                anyWins = true;
            }

            finishUpdate = true;
        }

        //private IEnumerator updateOtherPlayerSkin()
        //{

        //    yield return new WaitForSeconds(0.05f);
        //    string skinID1Temp = FirebaseManager.instance.getOtherPlayerSkinID(inGameUsersUID[0], 0);
        //    if (skinID1Temp != null)
        //    {
        //        skinID1 = int.Parse(skinID1Temp);
        //        otherCharacter1.AddComponent<SpriteRenderer>();
        //        otherCharacter1.GetComponent<SpriteRenderer>().sprite = skins[skinID1];
        //    }


        //    yield return new WaitForSeconds(0.05f);
        //    string skinID2Temp = FirebaseManager.instance.getOtherPlayerSkinID(inGameUsersUID[1], 1);

        //    if (skinID2Temp != null)
        //    {
        //        skinID2 = int.Parse(skinID2Temp);
        //        otherCharacter2.AddComponent<SpriteRenderer>();
        //        otherCharacter2.GetComponent<SpriteRenderer>().sprite = skins[skinID2];
        //    }

        //    finishUpdateSkin = true;
        //}

        void check_Cheating()
        {
            accelerationCurrent_CC = Mathf.Lerp(accelerationCurrent_CC, Input.acceleration.magnitude, Time.deltaTime * filterCurrent_CC);
            accelerationAverage_CC = Mathf.Lerp(accelerationAverage_CC, Input.acceleration.magnitude, Time.deltaTime * filterAverage_CC);
            float delta = accelerationCurrent_CC - accelerationAverage_CC; // 获取差值，即坡度

            if (delta > DebugUIController.instance.walking_threshold)//往高
            {
                steps_CC++;
            }

        }

    }
}

