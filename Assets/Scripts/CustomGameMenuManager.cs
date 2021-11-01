using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomGameMenuManager : MonoBehaviour
{
    public void custom_relay_game()
    {
        PlayerManager.instance.setGameStatus("Custom Relay Game", "In Team Menu");
        SceneManager.LoadScene("Team Menu");
    }

    public void back_Button()
    {
        SceneManager.LoadScene("Game Lobby");
    }
}
