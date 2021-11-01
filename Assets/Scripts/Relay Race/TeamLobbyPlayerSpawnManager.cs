using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamLobbyPlayerSpawnManager : MonoBehaviour
{
    [SerializeField] GameObject[] playerSpawns;
    [SerializeField] GameObject[] clones = new GameObject[10];
    public TMP_Text[] username_Text;

    public TeamData teamData;
    private int localVersion = -1;
    private string[] localNameList = new string[10];
    private Animator[] anims;

    private bool startUpdate;
    private bool needsUpdate;

    private void Start()
    {
        startUpdate = false;
        needsUpdate = true;
        clones = new GameObject[10];
        anims = new Animator[10];
        localNameList = new string[10];
        StartCoroutine(startTimer());
    }

    private IEnumerator startTimer()
    {
        yield return new WaitForSecondsRealtime(2f);
        startUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        teamData = API.instance.getTeamData();

        if (needsUpdate && startUpdate)
        {
            StartCoroutine(update_player_spawn_coroutine());
        }
    }

    private IEnumerator update_player_spawn_coroutine()
    {
        needsUpdate = false;
        yield return new WaitForEndOfFrame();

        
        for (int i = 0; i < teamData.nameList.Length; i++)
        {
            if (localNameList[i] != teamData.nameList[i])
            {
                Destroy(clones[i]);

                if (teamData.nameList[i] != "placeholder")
                {
                    playerSpawns[i].SetActive(true);
                    clones[i] = Instantiate(SkinManager.instance.getSkinByID(int.Parse(teamData.skinIDList[i])), playerSpawns[i].transform.position, Quaternion.identity);
                    clones[i].transform.parent = playerSpawns[i].transform;
                    clones[i].transform.Rotate(0, (i * 36.0f) - 90, 0);

                    clones[i].GetComponent<PlaneTexture>().setImage(teamData.profileImageUrlList[i]);
                    clones[i].GetComponent<PlaneTexture>().setName(teamData.nameList[i]);

                    anims[i] = clones[i].GetComponent<Animator>();
                    anims[i].SetInteger("state", 0);
                }
                else
                {
                    playerSpawns[i].SetActive(false);
                }
            }
        }

        localNameList = teamData.nameList;
        
        yield return new WaitForSeconds(0.5f);

        needsUpdate = true;
    }
}
