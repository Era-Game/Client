using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Michsky.UI.ModernUIPack;
using TMPro;

namespace Managers
{

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


        private void Awake()
        {
            resetScene();
            getPlayerData();
        }

        public void getPlayerData()
        {
            _UID = PlayerManager.instance.getData("uid");
            isCustom = PlayerManager.instance.getData("gameName") == "Custom Relay Game";
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
            StartCoroutine(Create_Team_Coroutine());
        }

        private IEnumerator Create_Team_Coroutine()
        {
            loadingUI.SetActive(true);
            string tempTeamName = TeamNameInputField.text;
            Debug.Log(_UID);
            API.instance.Create_Team(tempTeamName, _UID, PlayerManager.instance.getData("gameName"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                push_notification("Success", "Entering team " + tempTeamName + "'s team lobby...");
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
            API.instance.Join_Team(_teamCode, _UID, PlayerManager.instance.getData("gameName"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            if (_statusCode == 200)
            {
                API.instance.Update_Team_Data(_teamCode, _UID);
                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }

                _statusCode = API.instance.statusCode;
                _responseMessage = API.instance.responseMessage;

                if (_statusCode == 200)
                {
                    push_notification("Success", "Entering team lobby...");
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
}
