using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpectatorMenuManager : MonoBehaviour
{
    public static SpectatorMenuManager instance;

    [Header("Team Lobby Manager")]
    public GameObject listOfGames;
    public GameObject joinGameButton;

    [Header("Admin Utils")]
    public InputField numberOfTeams;

    //Private Member Variables
    private bool avaliableGameNeedsUpdate;
    private bool quit;
    private string gameID;

    //Queue Private Variables
    public List<string> gameIDs = new List<string>();
    public List<string> localGameIDs = new List<string>();
    public ArrayList teamAmount = new ArrayList();
    public List<GameObject> tempGameObject = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        quit = false;
        avaliableGameNeedsUpdate = true;
        gameID = "placeholder";

        gameIDs = new List<string>();
        localGameIDs = new List<string>();
        gameIDs.Clear();
        localGameIDs.Clear();

        LevelLoader.instance.ClearCrossFade();
    }

    private void Update()
    {
        if (avaliableGameNeedsUpdate)
        {
            StartCoroutine(update_AvaliableGame_Coroutine());
        }
    }
    private IEnumerator update_AvaliableGame_Coroutine()
    {
        avaliableGameNeedsUpdate = false;

        yield return new WaitForEndOfFrame();

        FirebaseManager.instance.updateSpectatableGames();

        yield return new WaitForSeconds(1f);

        gameIDs = FirebaseManager.instance.getGameIDList();
        teamAmount = FirebaseManager.instance.getTeamAmountInEachGame();

        //New Game Display

        if (tempGameObject.Count > gameIDs.Count)
        {
            for (int i = 0; i < gameIDs.Count; ++i)
            {
                if (tempGameObject[i] == null || tempGameObject[i].GetComponent<CustomButtonClick>().getGameID() != gameIDs[i])
                {
                    Destroy(tempGameObject[i]);
                    tempGameObject[i] = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform);
                    tempGameObject[i].GetComponent<CustomButtonClick>().setDisplayText(gameIDs[i].ToString(), teamAmount[i].ToString());
                }
            }

            for (int i = gameIDs.Count; i < tempGameObject.Count; ++i)
            {
                Destroy(tempGameObject[i]);
                tempGameObject.RemoveAt(i);
            }

        }
        else
        {
            for (int i = 0; i < tempGameObject.Count; ++i)
            {
                if (tempGameObject[i] == null || tempGameObject[i].GetComponent<CustomButtonClick>().getGameID() != gameIDs[i])
                {
                    Destroy(tempGameObject[i]);
                    tempGameObject[i] = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform);
                    tempGameObject[i].GetComponent<CustomButtonClick>().setDisplayText(gameIDs[i].ToString(), teamAmount[i].ToString());
                }
            }

            for (int i = tempGameObject.Count; i < gameIDs.Count; ++i)
            {
                GameObject tempClone = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform) as GameObject;
                tempGameObject.Add(tempClone);
                tempClone.GetComponent<CustomButtonClick>().setDisplayText(gameIDs[i].ToString(), teamAmount[i].ToString());
            }
        }

        avaliableGameNeedsUpdate = true;
    }

    public string getGameID()
    {
        return gameID;
    }
    public void leave_Button()
    {
        LevelLoader.instance.loadScene("Lobby");
    }

    public void LoadToCustomRelayGame(string _game_ID)
    {
        gameID = _game_ID;
        LevelLoader.instance.loadScene("Spectator Lobby");
    }
}
