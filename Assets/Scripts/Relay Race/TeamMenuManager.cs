using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Michsky.UI.ModernUIPack;
using TMPro;
using Model;
using Utils;

public class TeamMenuManager : MonoBehaviour
{
    [Header("Team Menu Manager")]
    public GameObject JoinTeamUI;
    public GameObject CreateTeamUI;
    public GameObject TeamMenuUI;
    public GameObject loadingUI;
    public TMP_InputField TeamCodeInputField;
    public TMP_InputField TeamNameInputField;
    public NotificationManager notification;
    public Text logText;

    //Player Data
    private string _UID;
    private bool isCustom;
    private User Player;


    private void Awake()
    {
        resetScene();
        getPlayerData();
    }

    public void getPlayerData()
    {
        Player = PlayerManager.instance.GetUser();
        _UID = Player.id.ToString();//PlayerManager.instance.getData("uid");
        isCustom = Player.gameStatus_gameName == "Custom Relay Game";//PlayerManager.instance.getData("gameName") == "Custom Relay Game";
    }
    public void resetScene()
    {
        loadingUI.SetActive(false);
        JoinTeamUI.SetActive(false);
        CreateTeamUI.SetActive(false);
        TeamMenuUI.SetActive(false);
    }
    public void open_Create_Team_UI()
    {
        CreateTeamUI.SetActive(true);
        loadingUI.SetActive(false);
        JoinTeamUI.SetActive(false);
        TeamMenuUI.SetActive(false);
    }
    public void Create_Team()
    {
        string tempTeamName = TeamNameInputField.text;
        StartCoroutine(Create_Team_Coroutine(tempTeamName));
    }

    private IEnumerator Create_Team_Coroutine(string tempTeamName)
    {
        loadingUI.SetActive(true);
        Debug.Log(_UID);
        Debug.Log(@"Player ID is: " + Player.id.ToString());
        //string defaultData = PlayerPrefsHelper.GetDefaultData();
        //User player = JsonUtility.FromJson<User>(PlayerPrefsHelper.GetDefaultData());
        API.instance.Create_Team(tempTeamName, Player.id.ToString(), PlayerManager.instance.getData("gameName"));

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        long _statusCode = API.instance.statusCode;
        string _responseMessage = API.instance.responseMessage;

        if (_statusCode == 200)
        {
            push_notification("Success", "Entering team " + tempTeamName + "'s team lobby...");
            Player.teamCode = tempTeamName;
            PlayerPrefsHelper.SetDefaultData(Player);
            yield return new WaitForSeconds(1f);
            //SceneManager.LoadScene("Team Lobby");
            LevelLoader.instance.loadScene("Team Lobby");
        }
        else
        {
            loadingUI.SetActive(false);
            push_notification("Warning", _responseMessage);
            //logText.text = _responseMessage;
            Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
        }
    }

    public void Open_Join_Team_UI()
    {
        JoinTeamUI.SetActive(true);
        loadingUI.SetActive(false);
        CreateTeamUI.SetActive(false);
        TeamMenuUI.SetActive(false);
    }

    public void close_Button()
    {
        JoinTeamUI.SetActive(false);
        CreateTeamUI.SetActive(false);
        loadingUI.SetActive(false);
        TeamMenuUI.SetActive(false);
    }

    public void Join_Team()
    {
        string teamCode = TeamCodeInputField.text;
        StartCoroutine(Join_Team_Coroutine(teamCode));
    }

    private IEnumerator Join_Team_Coroutine(string _teamCode)
    {
        loadingUI.SetActive(true);
        API.instance.Join_Team(_teamCode, Player.id.ToString(), PlayerManager.instance.getData("gameName"));

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        long _statusCode = API.instance.statusCode;
        string _responseMessage = API.instance.responseMessage;

        if (_statusCode == 200)
        {
            API.instance.Update_Team_Data(_teamCode, Player.id.ToString());
            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                push_notification("Success", "Entering team lobby...");
                Player.teamCode = _teamCode;
                PlayerPrefsHelper.SetDefaultData(Player);
                yield return new WaitForSeconds(1f);
                //SceneManager.LoadScene("Team Lobby");
                LevelLoader.instance.loadScene("Team Lobby");
            }
            else
            {
                loadingUI.SetActive(false);
                logText.text = _responseMessage;
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
            }
        }
        else
        {
            loadingUI.SetActive(false);
            push_notification("Warning", _responseMessage);
            Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");
        }
    }

    public void back_Button()
    {
        //SceneManager.LoadScene("Game Lobby");
        PlayerManager.instance.setGameStatus("placeholder", "placeholder");
        LevelLoader.instance.loadScene("Lobby");
        //SceneManager.LoadScene("Lobby");
    }

    private void push_notification(string title, string message)
    {
        notification.title = title;
        notification.description = message;
        notification.UpdateUI();
        notification.OpenNotification();
    }
}
