using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SpectatorWinSceneManager : MonoBehaviour
{
    [Header("Scene Objects")]
    public GameObject SOW;
    public GameObject[] rankings;
    public TMP_Text[] teamNameTexts;
    public TMP_Text[] teamDistanceTexts;
    public GameObject screenReferenceObject; 

    SpectatorData sData;

    public string[] teamNames;
    public double[] teamDistance;

    public void leave_button()
    {
        LevelLoader.instance.loadScene("Lobby");
    }

    // Start is called before the first frame update
    void Start()
    {
        
        sData = API.instance.getSpectatorData();

        teamNames = new string[5];
        teamDistance = new double[5];

        StartCoroutine(Update_UI_Coroutine());
    }

    IEnumerator Update_UI_Coroutine()
    {

        FirebaseManager.instance.updateSpectatorLeaderboard(SpectatorMenuManager.instance.getGameID(), sData.winnerTeamID, sData.relay_totalsteps) ;

        yield return new WaitForSeconds(0.5f);

        teamNames = FirebaseManager.instance.getLeaderboardTeamName();
        teamDistance = FirebaseManager.instance.getLeaderboardTotalSteps();

        for (int i = 0; i < teamNames.Length; ++i)
        {
            if (teamNames[i] == "placeholder" || teamNames[i] == null || teamNames[i] == "")
            {
                teamNameTexts[i].text = "---";
                teamDistanceTexts[i].text = "--- (m)";
                rankings[i].SetActive(true);
            }
            else
            {
                teamNameTexts[i].text = teamNames[i];
                teamDistanceTexts[i].text = System.Math.Round(teamDistance[i], 2).ToString() + " (m)";
                rankings[i].SetActive(true);
            }
        }

        LevelLoader.instance.ClearCrossFade();
    }

    // Update is called once per frame
    void Update()
    {



        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            SOW.SetActive(false);
        }
        else
        {
            SOW.SetActive(true);
        }
    }
}
