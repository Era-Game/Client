using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLobbyManager : MonoBehaviour
{
    [Header("Game Lobby Manager")]
    public Button play_Button;
    public Button next_Button;
    public Button prev_Button;

    private int NUM_GAMES = 3;
    private int gameMode;
    private string[] gameModes = new string[3];

    private void Awake()
    {
        gameMode = 0;
        gameModes[0] = "Race";
        gameModes[1] = "Relay Race";
        gameModes[2] = "Custom";
        play_Button.GetComponentInChildren<Text>().text = gameModes[gameMode];
    }

    public void play()
    {
        switch (gameMode) {
            //GameMode: Race
            case 0:
                PlayerManager.instance.setGameStatus("Race", "In Queue");
                string _uid = PlayerManager.instance.getData("uid");
                FirebaseManager.instance.setInNormalQueue(_uid);
                SceneManager.LoadScene("Queue");
                break;
            //GameMode: Relay Race
            case 1:
                PlayerManager.instance.setGameStatus("Relay Game", "In Team Menu");
                SceneManager.LoadScene("Team Menu");
                break;
            //GameMode: Custom Game
            case 2:
                SceneManager.LoadScene("Custom Game Menu");
                break;
            default:
                break;
        }

    }

    public void next()
    {
        gameMode = (gameMode + 1) % NUM_GAMES;
        play_Button.GetComponentInChildren<Text>().text = gameModes[gameMode];
    }

    public void prev()
    {
        if (gameMode == 0)
        {
            gameMode = NUM_GAMES - 1;
        }
        else
        {
            gameMode -= 1;
        }
        play_Button.GetComponentInChildren<Text>().text = gameModes[gameMode];
    }

    public void back_Button()
    {
        SceneManager.LoadScene("Lobby");
    }
}
