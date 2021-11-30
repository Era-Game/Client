using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CoroutineHelper;
using static UserAPI;

public class NewLobbyManager : MonoBehaviour
{
    public static NewLobbyManager instance;


    [Header("Lobby Utilities")]
    public Text usernameText;
    public Text coinText;
    public Text rejoiningText;
    public GameObject TeamMenuUI;
    public GameObject SettingUI;
    public Transform character_spawn;

    private bool dataLoaded;
    private string gameName;
    private string gameStatus;
    private bool disableUI;

    private GameObject clone;
    private Animator animator;

    // Use this for initialization
    void Start()
    {
        LevelLoader.instance.ClearCrossFade();
        //Screen.orientation = ScreenOrientation.Portrait;
        TeamMenuUI.SetActive(false);
        SettingUI.SetActive(false);
        dataLoaded = false;
        disableUI = false;
        PlayerManager.instance.setIsOnline("True");
        usernameText.text = NewPlayerManager.instance.user.username;
        coinText.text = "x " + NewPlayerManager.instance.user.Coins;

        clone = Instantiate(SkinManager.instance.getSkinByID(int.Parse(PlayerManager.instance.getData("skinID"))), character_spawn.position, Quaternion.identity);
        clone.transform.parent = character_spawn;
        clone.transform.Rotate(0f, -140f, 0f);

        clone.GetComponent<PlaneTexture>().setName(PlayerManager.instance.getData("username"));
        clone.GetComponent<PlaneTexture>().hideNameTag();

        animator = clone.GetComponent<Animator>();
        animator.SetInteger("state", 0);

        gameName = PlayerManager.instance.getData("gameName");
        gameStatus = PlayerManager.instance.getData("gameStatus");

        PlayerManager.instance.setGameStatus("placeholder", "placeholder");

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        instance = this;
    }

    public void interaction_hitbox_1()
    {
        animator.SetTrigger("Interaction_1");
    }
    public void setConfigConstant()
    {
        FirebaseManager.instance.setConfig();
    }

    public void logout_button()
    {
        if (!disableUI)
        {
            PlayerManager.instance.initPlayManagerUtilities();
            FirebaseManager.instance.LogOutButton();
            LevelLoader.instance.loadScene("Login");
        }

    }

    public void close_button()
    {
        SettingUI.SetActive(false);
        TeamMenuUI.SetActive(false);
    }
    public void open_setting_UI()
    {
        SettingUI.SetActive(true);
    }

    public void shop_button()
    {
        if (!disableUI)
        {
            //SceneManager.LoadScene("Skin Shop");
            LevelLoader.instance.loadScene("Skin Shop");
        }

    }

    public void pet_button()
    {
        LevelLoader.instance.loadScene("Pet Scene");
    }

    public void edit_organization()
    {
        LevelLoader.instance.loadScene("Organization Menu");
    }

    public void open_spectator_menu()
    {
        LevelLoader.instance.loadScene("Spectator Menu");
    }
    public void joingame()
    {
        if (!disableUI)
        {
            //SceneManager.LoadScene("Game Lobby");

            //Temporary Code for Versions 1.1.x
            PlayerManager.instance.setGameStatus("Custom Relay Game", "In Lobby");
            //SceneManager.LoadScene("Team Menu");   
            TeamMenuUI.SetActive(true);
        }

    }

    private IEnumerator loadLobby()
    {
        yield return new WaitForEndOfFrame();

        // TODO: not handle the skin data yet.
        //fix_data();

        CoroutineWithData cd = new CoroutineWithData(this, UserAPI.instance.Rejoin_Check(NewPlayerManager.instance.user.id));
        yield return cd.coroutine;

        if (cd.result != null)
        {
            RejoinResponse response = (RejoinResponse)cd.result;

            if (response.targetScene != "Lobby")
            {
                // TODO: not handle rejoin case yet, these code wouldn't work
                // Also, Rejoin check should search on the new data schema. So pending this now.

                //API.instance.Update_Team_Data(teamCode, UID);

                //while (!API.instance.dataRecieved)
                //{
                //    yield return null;
                //}

                //if (_responseMessage == "Relay Game")
                //{
                //    API.instance.Update_InGame_Data(teamCode, UID);

                //    while (!API.instance.dataRecieved)
                //    {
                //        yield return null;
                //    }
                //}
            } else
            {
                // Todo: check with Howard what're the usage of these two fields
                //PlayerManager.instance.setTeamCode("placeholder");
                //PlayerManager.instance.setGameStatus("placeholder", "placeholder");

                //FirebaseManager.instance.readOrganizationExist(NewPlayerManager.instance.user);

                //while (!FirebaseManager.instance.isDataRecieved())
                //{
                //    yield return null;
                //}

                //if (! FirebaseManager.instance.organizationExist())
                //{
                //    LevelLoader.instance.loadScene("Organization Menu");
                //}
            }

            LevelLoader.instance.loadScene(response.targetScene);
        }
        else if (cd.result == null)
        {
            LoginManager.instance.update_warningLoginText("login error");
            UIManager.instance.displayWarning("Error", "an error occurred while rejoin check");
        }

        

        //if (_statusCode == 200)
        //{
        //    if (_responseMessage != "Lobby")
        //    {
        //        isRejoin = true;
        //        API.instance.Update_Team_Data(teamCode, UID);

        //        while (!API.instance.dataRecieved)
        //        {
        //            yield return null;
        //        }

        //        if (_responseMessage == "Relay Game")
        //        {
        //            API.instance.Update_InGame_Data(teamCode, UID);

        //            while (!API.instance.dataRecieved)
        //            {
        //                yield return null;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        PlayerManager.instance.setTeamCode("placeholder");
        //        PlayerManager.instance.setGameStatus("placeholder", "placeholder");
        //    }

        //    FirebaseManager.instance.readOrganizationExist(UID);

        //    while (!FirebaseManager.instance.isDataRecieved())
        //    {
        //        yield return null;
        //    }

        //    if (FirebaseManager.instance.organizationExist())
        //    {
        //        LevelLoader.instance.loadScene(_responseMessage);
        //    }
        //    else
        //    {
        //        LevelLoader.instance.loadScene("Organization Menu");
        //    }

        //}
    }

    private void fix_data()
    {
        //if (ownedSkins.Length < NUM_SKINS)
        //{
        //    Debug.Log("Fix Data in Player Manager");
        //    bool[] arr = new bool[ownedSkins.Length];

        //    for (int i = 0; i < ownedSkins.Length; ++i)
        //    {
        //        arr[i] = ownedSkins[i] == "True" ? true : false;
        //    }
        //    FirebaseManager.instance.fix_ownedSkins_data(arr);
        //}
    }
}
