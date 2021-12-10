using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

public class CustomButtonClick : MonoBehaviour
{
    Button button;
    public Text displayText;
    public TMP_Text gameIDText;
    public TMP_Text teamNumberText;
    public string buttonType;
    private string gameID;
    private string teamNumber;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonHandler);
    }

    // Update is called once per frame
    void Update()
    {
        //displayText.text = "GameID: " + gameID + "     " + teamNumber +"/5";
        gameIDText.text = "GameID: " + gameID;
        teamNumberText.text = teamNumber + "/5";
        if (gameID == null || gameID == "" || gameID == "placeholder")
        {
            Destroy(gameObject);
        }
    }

    public void setDisplayText(string _gameID, string _teamNumber)
    {
        gameID = _gameID;
        teamNumber = _teamNumber;
    }

    public string getGameID()
    {
        return gameID;
    }
    void ButtonHandler()
    {
        if (buttonType == "Relay Game")
        {
            TeamLobbyManager.instance.LoadToCustomRelayGame(gameID);
        }
        else if(buttonType == "Spectator")
        {
            SpectatorMenuManager.instance.LoadToCustomRelayGame(gameID);
        }
        
    }
}
