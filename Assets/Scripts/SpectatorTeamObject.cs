using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;

using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;


public class SpectatorTeamObject : MonoBehaviour
{
    public static SpectatorTeamObject instance;

    [Header("Scene Object")]
    public TMP_Text[] memberNames;
    public TMP_Text teamNameText;
    [SerializeField] Sprite readySprite;
    [SerializeField] Sprite unreadySprite;

    public string gameID;
    public string teamName;
    public string teamCode;
    private int memberCount;
    public string[] members = new string[10];

    private bool needsUpdate;
    public bool apiNeedsUpdate;

    SpectatorTeamData sTeamDatap;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        members = new string[10];
        teamName = "";
        teamCode = "placeholder";
        memberCount = 0;

    }

    private void Update()
    {

        if (apiNeedsUpdate)
        {
            StartCoroutine(Update_Spectator_Team_Data(SpectatorMenuManager.instance.getGameID(), teamCode));
        }
    }

    public void setTeamName(string _teamName, string _teamCode)
    {
        teamCode = _teamCode;
        teamName = _teamName;
        needsUpdate = true;
        apiNeedsUpdate = true;
    }
    public void setMemberName(int index, string name)
    {
        Debug.Log("Changing members[" + index + "] to " + name);
        

        memberNames[index].text = name;

        if (name == "---")
        {
            members[index] = "placeholder";
        }
        else
        {
            members[index] = name;
        }
        
        
    }

    public string getTeamName()
    {
        return teamName;
    }
    public string[] getMemberNames()
    {
        return members;
    }


    private IEnumerator Update_Spectator_Team_Data(string _gameID, string _teamCode)
    {
        apiNeedsUpdate = false;

        yield return new WaitForEndOfFrame();

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("GAME_ID", _gameID);
        data.Add("TEAMCODE", _teamCode);

        string json = JSONIZE(data);

        Debug.Log("JSON SENT: " + json);

        UnityWebRequest www = UnityWebRequest.Get(URL.Update_Spectator_Team_Data + json);
        yield return www.SendWebRequest();
        //yield return new WaitForSecondsRealtime(0.5f);

        long  statusCode = www.responseCode;
        string responseMessage = www.downloadHandler.text;
        

        if (statusCode == 200)
        {
            sTeamDatap = JsonConvert.DeserializeObject<SpectatorTeamData>(www.downloadHandler.text);

            members = sTeamDatap.nameList;

            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i] != "placeholder")
                {
                    memberNames[i].text = members[i];
                }
                else
                {
                    memberNames[i].text = "---";
                }
                
            }

            memberCount = 0;

            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i] != "placeholder")
                {
                    ++memberCount;
                }
            }

            teamNameText.text = teamName + " (" + memberCount.ToString() + "/10)";
        }

        Debug.Log(responseMessage);



        apiNeedsUpdate = true;
    }

    private string JSONIZE(Dictionary<string, string> input)
    {
        string output = "?";

        foreach (KeyValuePair<string, string> item in input)
        {
            output += string.Format("{0}={1}&", item.Key, item.Value);
        }

        output = output.Remove(output.Length - 1);
        return output;
    }
}
