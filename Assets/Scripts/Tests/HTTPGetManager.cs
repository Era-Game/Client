using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using Managers;

namespace Managers
{
    public class HTTPGetManager : MonoBehaviour
    {
        public Text messagetext;
        private string rawJson;

        TeamData teamData;
        InGameData inGameData;

        public string[] UIDs = {
        "2kaLLUOhunYU29ThUamWIMI02Kk1",
        "B6gSjxbA2rdfwTbYogNo5l9AJSp1",
        "OI5HweiALLQVabncyCAccu6O1Dc2",
        "bTlvCVUac2arETGzl4HtP1OAepy2",
        "etPhU3N0SVbjqhDY44tcecVB6fy2",
        "s06aUe3opsf70mg8pY5KNO8TTf73",
        "xtux1l55hWWPWb0yVu1wPDOBJFu1",
        "Tn0iBjiBJ3YAVrjlRJBQGF5oMfN2",
    };

        [Header("UI")]
        public Text statusCodeText;
        public Text responseMessageText;

        [Header("Team Utilities")]
        public string TEAMNAME = "Default Teamname";
        public string TEAMCODE = "000000";
        public int UID_INDEX = 0;
        public string GAME_TYPE = "Default Game Type";

        [Header("Game Utilities")]
        public string GAME_ID = "Default Game ID";
        public int STEPS = 0;
        public float DISTANCE = 0;
        public float VELOCITY = 0;
        public bool IS_READY = true;
        public string USERNAME = "Default Username";
        public float PERSONAL_BEST_TIME = 9999999;


        readonly string getURL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/test_HTTP_Request";
        readonly string Temp_URL = URL.Test_URL;

        public bool startQueue = false;
        public bool apiNeedsUpdate = false;

        private void Update()
        {
            if (startQueue)
            {
                StartCoroutine(update_API());
            }
        }

        private IEnumerator update_API()
        {
            apiNeedsUpdate = false;
            yield return new WaitForEndOfFrame();

            //Update when in queue
            if (startQueue)
            {
                API.instance.Update_InGame_Data(TEAMCODE, UIDs[UID_INDEX]);

                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }

                long _statusCode = API.instance.statusCode;
                string _responseMessage = API.instance.responseMessage;
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");

                yield return new WaitForSecondsRealtime(5f);

                API.instance.Update_Queue_Data(GAME_ID);

                while (!API.instance.dataRecieved)
                {
                    yield return null;
                }

                _statusCode = API.instance.statusCode;
                _responseMessage = API.instance.responseMessage;
                Debug.Log("Request returns with Error: " + _statusCode + " (" + _responseMessage + ").");

                yield return new WaitForSecondsRealtime(5f);

            }

            apiNeedsUpdate = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            //API.instance.Update_Team_Data("258573", "B6gSjxbA2rdfwTbYogNo5l9AJSp1");
            //API.instance.Join_Team("854751", "2kaLLUOhunYU29ThUamWIMI02Kk1", "custom relay race");
            //API.instance.Join_Team("854751", "B6gSjxbA2rdfwTbYogNo5l9AJSp1", "custom relay race");
            //API.instance.Join_Team("854751", "OI5HweiALLQVabncyCAccu6O1Dc2", "custom relay race");
            //API.instance.Join_Team("854751", "bTlvCVUac2arETGzl4HtP1OAepy2", "custom relay race");
            //API.instance.Join_Team("854751", "etPhU3N0SVbjqhDY44tcecVB6fy2", "custom relay race");
            //API.instance.Join_Team("854751", "s06aUe3opsf70mg8pY5KNO8TTf73", "custom relay race");
            //API.instance.Join_Team("002482", "xtux1l55hWWPWb0yVu1wPDOBJFu1", "custom relay race");
            //API.instance.Leave_Team("002482", "2kaLLUOhunYU29ThUamWIMI02Kk1");
            //API.instance.Leave_Team("002482", "Tn0iBjiBJ3YAVrjlRJBQGF5oMfN2");
            //API.instance.Leave_Team("002482", "OI5HweiALLQVabncyCAccu6O1Dc2");
            //API.instance.Leave_Team("002482", "bTlvCVUac2arETGzl4HtP1OAepy2");
            //API.instance.Leave_Team("535548", "B6gSjxbA2rdfwTbYogNo5l9AJSp1");

            //API.instance.Create_Team("THIS IS A TEST", "B6gSjxbA2rdfwTbYogNo5l9AJSp1", "Custom Relay Game");
            API.instance.Create_Game("055060", "THIS IS A TEST", "B6gSjxbA2rdfwTbYogNo5l9AJSp1");
            //Tn0iBjiBJ3YAVrjlRJBQGF5oMfN2, B6gSjxbA2rdfwTbYogNo5l9AJSp1, 2kaLLUOhunYU29ThUamWIMI02Kk1
        }

        public void create_team()
        {
            StartCoroutine(create_team_coroutine());
        }

        private IEnumerator create_team_coroutine()
        {
            API.instance.Create_Team(TEAMNAME, UIDs[UID_INDEX], GAME_TYPE);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }

        public void join_team()
        {
            StartCoroutine(join_team_coroutine());
        }

        private IEnumerator join_team_coroutine()
        {
            API.instance.Join_Team(TEAMCODE, UIDs[UID_INDEX], GAME_TYPE);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();
        }

        public void leave_team()
        {
            StartCoroutine(leave_team_coroutine());
        }

        private IEnumerator leave_team_coroutine()
        {
            API.instance.Leave_Team(TEAMCODE, UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();
        }

        public void update_team()
        {
            StartCoroutine(update_team_coroutine());
        }
        private IEnumerator update_team_coroutine()
        {
            API.instance.Update_Team_Data(TEAMCODE, UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            API.instance.logTeamData();
        }

        public void create_game()
        {
            StartCoroutine(create_game_coroutine());
        }
        private IEnumerator create_game_coroutine()
        {
            API.instance.Create_Game(TEAMCODE, TEAMNAME, UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }

        public void join_game()
        {
            StartCoroutine(join_game_coroutine());
        }
        private IEnumerator join_game_coroutine()
        {
            API.instance.Join_Game(GAME_ID, TEAMCODE, TEAMNAME, UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }

        public void leave_game()
        {
            StartCoroutine(leave_game_coroutine());
        }
        private IEnumerator leave_game_coroutine()
        {
            API.instance.Leave_Game(GAME_ID, TEAMCODE, UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }

        public void update_game()
        {
            StartCoroutine(update_game_coroutine());
        }
        private IEnumerator update_game_coroutine()
        {
            API.instance.Update_InGame_Data(TEAMCODE, UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }

        public void post_steps()
        {
            StartCoroutine(post_steps_coroutine());
        }
        private IEnumerator post_steps_coroutine()
        {
            API.instance.Post_Steps(GAME_ID, TEAMCODE, STEPS.ToString(), DISTANCE.ToString(), VELOCITY.ToString(), UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }


        public void post_isReady()
        {
            StartCoroutine(post_isReady_coroutine());
        }
        private IEnumerator post_isReady_coroutine()
        {

            yield return null;
            /*
            API.instance.Post_isReady(GAME_ID, TEAMCODE, IS_READY.ToString(), UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
            */
        }

        public void post_teamRecords()
        {
            StartCoroutine(post_teamRecords_coroutine());
        }
        private IEnumerator post_teamRecords_coroutine()
        {
            API.instance.Post_teamRecords(GAME_ID, TEAMCODE, USERNAME, PERSONAL_BEST_TIME.ToString(), UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
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
        IEnumerator SimpleGetRequest()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UID", "sodijfpaisjdfdfasdf");
            data.Add("username", "Howard Yang");
            data.Add("age", "19");
            data.Add("sex", "male");

            string json = JSONIZE(data);
            Debug.Log(json);

            UnityWebRequest www = UnityWebRequest.Get(getURL + json);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                rawJson = www.downloadHandler.text;
                messagetext.text = www.downloadHandler.text;
            }
        }

        private SomeData saveData()
        {
            return JsonUtility.FromJson<SomeData>(rawJson);
        }

        [Serializable]
        public class MyClass
        {
            public string UID;
        }

        IEnumerator SimplePostRequest1()
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("?UID=huuiohihh"));

            UnityWebRequest request = UnityWebRequest.Post(getURL, formData);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            else
            {
                messagetext.text = request.downloadHandler.text;
            }
        }

        IEnumerator SimplePostRequest2()
        {
            MyClass myObject = new MyClass();
            myObject.UID = "0000000";

            var jsonString = JsonUtility.ToJson(myObject) ?? "NOTHING";
            Debug.Log(jsonString);
            UnityWebRequest request = UnityWebRequest.Put(getURL, jsonString);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.Send();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                messagetext.text = request.downloadHandler.text;
            }
        }

        public void Rejoin()
        {
            StartCoroutine(Rejoin_Coroutine());
        }
        private IEnumerator Rejoin_Coroutine()
        {
            API.instance.Rejoin_Check(UIDs[UID_INDEX]);

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long _statusCode = API.instance.statusCode;
            string _responseMessage = API.instance.responseMessage;

            responseMessageText.text = _responseMessage;
            statusCodeText.text = _statusCode.ToString();

            //API.instance.logTeamData();
        }

    }

    [Serializable]
    public class SomeData
    {
        private string _TOTALSTEPS;
        private string _STEP_THRESHOLD;
    }
}

