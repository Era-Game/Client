using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Managers
{
    public class API : MonoBehaviour
    {
        public static API instance;

        public TeamData teamData;
        public InGameData inGameData;
        public QueueData queueData;
        public SpectatorData spectatorData;

        public string responseMessage;
        public bool dataRecieved;
        public long statusCode;

        //POST Variables
        //  TEAMNAME
        //  UID
        //  GAME_TYPE
        //  GAME_ID
        //  STEPS
        //  IS_READY_STATUS
        //  USERNAME
        //  PERSONAL_BEST_TIME
        //  DISTANCE
        //  VELOCITY

        public void Rejoin_Check(string _UID)
        {
            Debug.Log("Posting Rejoin Check Request");
            StartCoroutine(Rejoin_Check_Request(_UID));
        }

        IEnumerator Rejoin_Check_Request(string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UID", _UID);


            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Rejoin_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Rejoin Check Returns with: (" + statusCode + ") " + responseMessage);
        }
        public void Create_Team(string _teamName, string _UID, string _gameType)
        {
            Debug.Log("Posting Create Team Request");
            StartCoroutine(Create_Team_Request(_teamName, _UID, _gameType));
        }

        IEnumerator Create_Team_Request(string _teamName, string _UID, string _gameType)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("TEAMNAME", _teamName);
            data.Add("UID", _UID);
            data.Add("GAME_TYPE", _gameType);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Create_Team_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
                teamData = JsonConvert.DeserializeObject<TeamData>(www.downloadHandler.text);
            }

            Debug.Log("Create Team Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Join_Team(string _teamCode, string _UID, string _gameType)
        {
            Debug.Log("Posting Join Team Request");
            StartCoroutine(Join_Team_Request(_teamCode, _UID, _gameType));
        }

        IEnumerator Join_Team_Request(string _teamCode, string _UID, string _gameType)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("TEAMCODE", _teamCode);
            data.Add("UID", _UID);
            data.Add("GAME_TYPE", _gameType);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Join_Team_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {

                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;

            }

            Debug.Log("Join Team Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Leave_Team(string _teamCode, string _UID)
        {
            Debug.Log("Posting Leave Team Request");
            StartCoroutine(Leave_Team_Request(_teamCode, _UID));
        }

        IEnumerator Leave_Team_Request(string _teamCode, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("TEAMCODE", _teamCode);
            data.Add("UID", _UID);

            string json = JSONIZE(data);


            UnityWebRequest www = UnityWebRequest.Get(URL.Leave_Team_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;

            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
                //teamData = JsonConvert.DeserializeObject<TeamData>(www.downloadHandler.text);
            }

            Debug.Log("Leave Team Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Update_Team_Data(string _teamCode, string _UID)
        {
            Debug.Log("Posting Update Team Request");
            StartCoroutine(Update_Team_Data_Request(_teamCode, _UID));
        }

        IEnumerator Update_Team_Data_Request(string _teamCode, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("TEAMCODE", _teamCode);
            data.Add("UID", _UID);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Team_Data_Update_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
                //Debug.Log(responseMessage);
                teamData = JsonConvert.DeserializeObject<TeamData>(www.downloadHandler.text);
            }

            Debug.Log("Update Team Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public TeamData getTeamData()
        {
            return teamData;
        }

        public void setTeamData_TeamCode(string _teamCode)
        {
            teamData.teamCode = _teamCode;
        }
        public void logTeamData()
        {
            string _teamCode = teamData.teamCode;
            string _gameID = teamData.gameID;
            string _gameType = teamData.gameType;
            int _memberCount = teamData.memberCount;
            string[] _memberIDs = teamData.memberIDs;
            string[] _nameList = teamData.nameList;
            string _teamname = teamData.teamname;
            bool _isLeader = teamData.isLeader;

            Debug.Log("Team Data:" +
                "\n1. teamCode: " + _teamCode +
                "\n2. gameID: " + _gameID +
                "\n3. gameType: " + _gameType +
                "\n4. memberCount: " + _memberCount +
                "\n5. teamname: " + _teamname +
                "\n6. isLeader: " + _isLeader.ToString() +
                "\n7. memberIDs: ["
                + _memberIDs[0] + ", "
                + _memberIDs[1] + ", "
                + _memberIDs[2] + ", "
                + _memberIDs[3] + ", "
                + _memberIDs[4] + ", "
                + _memberIDs[5] + ", "
                + _memberIDs[6] + ", "
                + _memberIDs[7] + ", "
                + _memberIDs[8] + ", "
                + _memberIDs[9] + "] + " +
                "\n8. nameList: ["
                + _nameList[0] + ", "
                + _nameList[1] + ", "
                + _nameList[2] + ", "
                + _nameList[3] + ", "
                + _nameList[4] + ", "
                + _nameList[5] + ", "
                + _nameList[6] + ", "
                + _nameList[7] + ", "
                + _nameList[8] + ", "
                + _nameList[9] + "]\n\n\n");
        }

        public void Create_Game(string _teamCode, string _teamname, string _UID)
        {
            Debug.Log("Posting Create Game Request");
            StartCoroutine(Create_Game_Coroutine(_teamCode, _teamname, _UID));
        }

        private IEnumerator Create_Game_Coroutine(string _teamCode, string _teamname, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("TEAMCODE", _teamCode);
            data.Add("TEAMNAME", _teamname);
            data.Add("UID", _UID);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Create_Game_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Create Game Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Leave_Game(string _gameID, string _teamCode, string _UID)
        {
            Debug.Log("Posting Leave Game Request");
            StartCoroutine(Leave_Game_Coroutine(_gameID, _teamCode, _UID));
        }

        private IEnumerator Leave_Game_Coroutine(string _gameID, string _teamCode, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("UID", _UID);

            string json = JSONIZE(data);


            UnityWebRequest www = UnityWebRequest.Get(URL.Leave_Game_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Leave Game Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Join_Game(string _gameID, string _teamCode, string _teamname, string _UID)
        {
            Debug.Log("Posting Join Game Request");
            StartCoroutine(Join_Game_Coroutine(_gameID, _teamCode, _teamname, _UID));
        }

        private IEnumerator Join_Game_Coroutine(string _gameID, string _teamCode, string _teamname, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("TEAMNAME", _teamname);
            data.Add("UID", _UID);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Join_Game_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Join Game Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Update_InGame_Data(string _teamCode, string _UID)
        {
            Debug.Log("Posting Update inGameData Request");
            StartCoroutine(Update_InGame_Data_Coroutine(_teamCode, _UID));
        }

        private IEnumerator Update_InGame_Data_Coroutine(string _teamCode, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("TEAMCODE", _teamCode);
            data.Add("UID", _UID);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Update_InGame_Data_URL + json);
            yield return www.SendWebRequest();
            //yield return new WaitForSeconds(0.5f);
            dataRecieved = true;


            statusCode = www.responseCode;
            responseMessage = www.downloadHandler.text;
            Debug.Log("Update Game Request Returns with: (" + statusCode + ") " + responseMessage);

            if (statusCode == 200)
            {
                inGameData = JsonConvert.DeserializeObject<InGameData>(www.downloadHandler.text);
            }

        }


        public void Post_Steps(string _gameID, string _teamCode, string _steps, string _distance, string _velocity, string _UID)
        {
            Debug.Log("Posting Post Steps Request");
            StartCoroutine(Post_Steps_Coroutine(_gameID, _teamCode, _steps, _distance, _velocity, _UID));
        }

        private IEnumerator Post_Steps_Coroutine(string _gameID, string _teamCode, string _steps, string _distance, string _velocity, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("STEPS", _steps);
            data.Add("DISTANCE", _distance);
            data.Add("VELOCITY", _velocity);
            data.Add("UID", _UID);

            string json = JSONIZE(data);
            //Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Post_Steps_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Post Steps Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        /*
        public void Post_isReady(string _gameID, string _teamCode, string _isReady, string _UID)
        {
            StartCoroutine(Post_isReady_Coroutine(_gameID, _teamCode, _isReady, _UID));
        }

        private IEnumerator Post_isReady_Coroutine(string _gameID, string _teamCode, string _isReady, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("IS_READY_STATUS", _isReady);
            data.Add("UID", _UID);

            string json = JSONIZE(data);
            Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Post_isReady_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
                Debug.Log(responseMessage);
                //inGameData = JsonConvert.DeserializeObject<InGameData>(www.downloadHandler.text);
            }
        }
        */

        public void Post_teamRecords(string _gameID, string _teamCode, string _username, string _peronalBestTime, string _UID)
        {
            Debug.Log("Posting Post Team Records Request");
            StartCoroutine(Post_teamRecords_Coroutine(_gameID, _teamCode, _username, _peronalBestTime, _UID));
        }

        private IEnumerator Post_teamRecords_Coroutine(string _gameID, string _teamCode, string _username, string _peronalBestTime, string _UID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("USERNAME", _username);
            data.Add("PERSONAL_BEST_TIME", _peronalBestTime);
            data.Add("UID", _UID);

            string json = JSONIZE(data);
            //Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Post_teamRecords_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Post Team Records Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Ability_Multiplier(string _gameID, string _teamCode, float _factor)
        {
            Debug.Log("Posting Ability Multiplier Request");
            StartCoroutine(Ability_Multiplier_Coroutine(_gameID, _teamCode, _factor.ToString()));
        }

        private IEnumerator Ability_Multiplier_Coroutine(string _gameID, string _teamCode, string _factor)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("MULTIPLIER", _factor);

            string json = JSONIZE(data);
            Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Ability_Multiplier + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Ability Multiplier Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Post_Org_Request(string _departmentID, bool _isHead, string _uid)
        {
            Debug.Log("Posting Org Reqest");
            StartCoroutine(Post_Org_Request_Coroutine(_departmentID, _isHead.ToString(), _uid));
        }

        private IEnumerator Post_Org_Request_Coroutine(string _departmentID, string _isHead, string _uid)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("DEPARTMENT_ID", _departmentID);
            data.Add("IS_HEAD", _isHead);
            data.Add("UID", _uid);

            string json = JSONIZE(data);
            Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Set_Organization_URL + json);
            yield return www.SendWebRequest();

            dataRecieved = true;
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (www.responseCode == 401)
                {
                    statusCode = www.responseCode;
                    responseMessage = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
            else
            {
                statusCode = www.responseCode;
                responseMessage = www.downloadHandler.text;
            }

            Debug.Log("Org Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public InGameData getInGameData()
        {
            return inGameData;
        }

        public InGameData resetInGameData()
        {
            inGameData.gameID = "placeholder";
            inGameData.totalSteps = 0;
            inGameData.distance = 0;
            inGameData.velocity = 0;
            inGameData.bonus = 1;
            inGameData.gameStart = false;
            inGameData.gameEnd = false;
            inGameData.winnerTeamID = "placeholder";
            inGameData.myTurn = false;
            inGameData.runner = "placeholder";
            inGameData.relay_totalsteps = 0;
            inGameData.relay_steps_threshold = 0;
            inGameData.relay_map = 0;

            return inGameData;
        }

        public InGameData resetQueueData()
        {
            string[] teamNameTemp = new string[5];
            bool[] teamReadyStatusTemp = new bool[5];

            for (int i = 0; i < teamNameTemp.Length; ++i)
            {
                teamNameTemp[i] = "placeholder";
                teamReadyStatusTemp[i] = false;
            }
            queueData.teamNameList = teamNameTemp;
            queueData.teamReadyStatus = teamReadyStatusTemp;

            return inGameData;
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void Update_Queue_Data(string _gameID)
        {
            Debug.Log("Posting Update Queue Request");
            StartCoroutine(Update_Queue_Data_Request(_gameID));
        }

        IEnumerator Update_Queue_Data_Request(string _gameID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);

            string json = JSONIZE(data);

            UnityWebRequest www = UnityWebRequest.Get(URL.Update_Queue_URL + json);
            yield return www.SendWebRequest();
            //yield return new WaitForSecondsRealtime(0.5f);

            dataRecieved = true;

            statusCode = www.responseCode;
            responseMessage = www.downloadHandler.text;
            Debug.Log("Update Queue Request Returns with: (" + statusCode + ") " + responseMessage);

            if (statusCode == 200)
            {
                queueData = JsonConvert.DeserializeObject<QueueData>(www.downloadHandler.text);
            }


        }

        public void Post_AFK_Request(string _teamCode, string _gameID, string _UID, bool _isOnline, bool _isRunner)
        {
            Debug.Log("Posting AFK Request");
            StartCoroutine(Post_AFK_Request_Coroutine(_teamCode, _gameID, _UID, _isOnline.ToString(), _isRunner.ToString()));
        }

        private IEnumerator Post_AFK_Request_Coroutine(string _teamCode, string _gameID, string _UID, string _isOnline, string _isRunner)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);
            data.Add("UID", _UID);
            data.Add("IS_ONLINE", _isOnline);
            data.Add("IS_RUNNER", _isRunner);

            string json = JSONIZE(data);

            Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.AFK_Handler_URL + json);
            yield return www.SendWebRequest();
            //yield return new WaitForSecondsRealtime(0.5f);

            dataRecieved = true;

            statusCode = www.responseCode;
            responseMessage = www.downloadHandler.text;

            Debug.Log("AFK Request Returns with: (" + statusCode + ") " + responseMessage);
        }

        public void Update_Spectator_Data(string _gameID)
        {
            Debug.Log("Posting Update Spectator Request");
            StartCoroutine(Update_Spectator_Data_Coroutine(_gameID));
        }

        private IEnumerator Update_Spectator_Data_Coroutine(string _gameID)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);

            string json = JSONIZE(data);

            Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Update_Spectator_Data + json);
            yield return www.SendWebRequest();
            //yield return new WaitForSecondsRealtime(0.5f);

            dataRecieved = true;

            statusCode = www.responseCode;
            responseMessage = www.downloadHandler.text;
            Debug.Log("Update Spectator Request Returns with: (" + statusCode + ") " + responseMessage);

            if (statusCode == 200)
            {
                spectatorData = JsonConvert.DeserializeObject<SpectatorData>(www.downloadHandler.text);
            }

        }

        /*
        public void Update_Spectator_Team_Data(string _gameID, string _teamCode, ref SpectatorTeamData sTeamData)
        {
            Debug.Log("Posting Update Spectator Team Data Request");
            StartCoroutine(Update_Spectator_Team_Data_Coroutine(_gameID, _teamCode));
        }

        private IEnumerator Update_Spectator_Team_Data_Coroutine(string _gameID, string _teamCode)
        {
            dataRecieved = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("GAME_ID", _gameID);
            data.Add("TEAMCODE", _teamCode);


            string json = JSONIZE(data);

            Debug.Log("JSON SENT: " + json);

            UnityWebRequest www = UnityWebRequest.Get(URL.Update_Spectator_Team_Data + json);
            yield return www.SendWebRequest();
            //yield return new WaitForSecondsRealtime(0.5f);

            dataRecieved = true;

            statusCode = www.responseCode;
            responseMessage = www.downloadHandler.text;

            Debug.Log("Update Spectator Team Request Returns with: (" + statusCode + ") " + responseMessage);

            spectatorTeamData = JsonConvert.DeserializeObject<SpectatorTeamData>(www.downloadHandler.text);
        }
        */

        public SpectatorData getSpectatorData()
        {
            return spectatorData;
        }
        public QueueData getQueueData()
        {
            return queueData;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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
}

