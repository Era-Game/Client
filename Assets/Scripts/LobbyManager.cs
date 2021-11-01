using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

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

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        LevelLoader.instance.ClearCrossFade();
        //Screen.orientation = ScreenOrientation.Portrait;
        TeamMenuUI.SetActive(false);
        SettingUI.SetActive(false);
        dataLoaded = false;
        disableUI = false;
        PlayerManager.instance.setIsOnline("True");
        usernameText.text = PlayerManager.instance.getData("username");
        coinText.text = "x " + PlayerManager.instance.getData("coins");
        
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

    public void interaction_hitbox_1()
    {
        animator.SetTrigger("Interaction_1");
    }
    public void setConfigConstant()
    {
        FirebaseManager.instance.setConfig();
    }

}
