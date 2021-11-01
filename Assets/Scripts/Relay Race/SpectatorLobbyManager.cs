using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpectatorLobbyManager : MonoBehaviour
{

    [Header("Team Lobby Manager")]
    public GameObject listOfGames;
    public GameObject joinGameButton;
    public TMP_Text gameTitleText;
    public TMP_Text teamNumberText;

    public SpectatorData sData;

    public bool needsUpdate;
    public bool apiNeedsUpdate;
    private bool differenceSpotted;
    public bool quit; 

    public List<GameObject> tempGameObject = new List<GameObject>();

    public List<string> processed_teams = new List<string>();
    public List<string> processed_teamCodes = new List<string>();
    public List<string> processed_status = new List<string>();
    public string[] teams;
    public string[] teamCodes;
    public string[] status;
    public string[] members;

    public void leave_Button()
    {
        LevelLoader.instance.loadScene("Spectator Menu");
    }
    // Start is called before the first frame update
    void Start()
    {
        gameTitleText.text = "Game " + SpectatorMenuManager.instance.getGameID() + "'s Lobby";
        teamNumberText.text = "0/5";
        processed_teams.Clear();

        needsUpdate = true;
        apiNeedsUpdate = true;
        differenceSpotted = false;
        quit = false;

        LevelLoader.instance.ClearCrossFade();
    }

    // Update is called once per frame
    void Update()
    {
        if (needsUpdate)
        {
            StartCoroutine(update_AvaliableGame_Coroutine());
        }
        


        if (apiNeedsUpdate)
        {
            StartCoroutine(update_API());
        }

        if (sData.gameStart && !quit)
        {
            StartCoroutine(load_spectator_scene());
        }
    }

    private IEnumerator load_spectator_scene()
    {
        quit = true;

        yield return new WaitForEndOfFrame();

        LevelLoader.instance.loadScene("Spectator Scene");
    }

    private IEnumerator update_API()
    {
        apiNeedsUpdate = false;

        yield return new WaitForEndOfFrame();

        API.instance.Update_Spectator_Data(SpectatorMenuManager.instance.getGameID());

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        long statusCode = API.instance.statusCode;
        string responseMessage = API.instance.responseMessage;

        Debug.Log("Spectator Update Returns with code (" + statusCode.ToString() + "}: " + responseMessage);

        sData = API.instance.getSpectatorData();

        apiNeedsUpdate = true;
        
    }
    private IEnumerator update_AvaliableGame_Coroutine()
    {
        needsUpdate = false;

        yield return new WaitForEndOfFrame();

        FirebaseManager.instance.updateCustomQueueData(SpectatorMenuManager.instance.getGameID());

        while (!FirebaseManager.instance.isDataRecieved())
        {
            yield return null;
        }

        teams = FirebaseManager.instance.getQueueTeamNames();
        teamCodes = FirebaseManager.instance.getQueueTeamCodes();
        status = FirebaseManager.instance.getQueueTeamStatus();

        processed_teams.Clear();
        processed_teamCodes.Clear();
        processed_status.Clear();

        for (int i = 0; i < teams.Length; ++i)
        {
            if (teams[i] != "placeholder")
            {
                processed_teams.Add(teams[i]);
                processed_teamCodes.Add(teamCodes[i]);
                processed_status.Add(status[i]);
            }
            
        }

        //New Game Display

        teamNumberText.text = processed_teams.Count.ToString() + "/5";

        if (tempGameObject.Count > processed_teams.Count)
        {
            Debug.Log("Detected data array is less than object in scene.");
            for (int i = 0; i < processed_teams.Count; ++i)
            {

                if (tempGameObject[i] == null || tempGameObject[i].GetComponent<SpectatorTeamObject>().getTeamName() != processed_teams[i])
                {

                    Debug.Log("Correcting display");

                    Destroy(tempGameObject[i]);
                    tempGameObject[i] = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform);

                    yield return new WaitForSeconds(0.1f);

                    tempGameObject[i].GetComponent<SpectatorTeamObject>().setTeamName(processed_teams[i], processed_teamCodes[i]);

                }
            }

            for (int i = processed_teams.Count; i < tempGameObject.Count; ++i)
            {
                Debug.Log("Destroying Extra OBjects");
                Destroy(tempGameObject[i]);
                tempGameObject.RemoveAt(i);
            }

        }
        else
        {
            Debug.Log("Detected object in scene is less than data array.");

            for (int i = 0; i < tempGameObject.Count; ++i)
            {

                if (tempGameObject[i] == null || tempGameObject[i].GetComponent<SpectatorTeamObject>().getTeamName() != processed_teams[i])
                {
                    Debug.Log("Correcting display");
                    Destroy(tempGameObject[i]);
                    tempGameObject[i] = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform);

                    yield return new WaitForSeconds(0.1f);

                    tempGameObject[i].GetComponent<SpectatorTeamObject>().setTeamName(processed_teams[i], processed_teamCodes[i]);
                }
            }

            for (int i = tempGameObject.Count; i < processed_teams.Count; ++i)
            {
                GameObject tempClone = Instantiate(joinGameButton, listOfGames.transform.position, Quaternion.identity, listOfGames.transform) as GameObject;
                tempGameObject.Add(tempClone);

                yield return new WaitForSeconds(0.1f);

                tempClone.GetComponent<SpectatorTeamObject>().setTeamName(processed_teams[i], processed_teamCodes[i]);
            }
        }

        needsUpdate = true;
    }
}
