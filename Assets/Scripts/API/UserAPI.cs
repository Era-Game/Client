using UnityEngine;
using System.Collections;
using System;
using System.Text;
using UnityEngine.Networking;

public class UserAPI : MonoBehaviour
{
    public static UserAPI instance;

    [Serializable]
    public class RejoinRequest
    {
        public int userId;

        public RejoinRequest(int userId)
        {
            this.userId = userId;
        }
    }

    [Serializable]
    public class RejoinResponse
    {
        public string targetScene;

        public RejoinResponse CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<RejoinResponse>(jsonString);
        }
    }

    // TODO: adjust server DB query logic
    public IEnumerator Rejoin_Check(int userId)
    {
        RejoinRequest rejoinReq = new RejoinRequest(userId);
        string jsonData = JsonUtility.ToJson(rejoinReq);
        Debug.Log(jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = UnityWebRequest.Post("http://ec2-3-112-239-208.ap-northeast-1.compute.amazonaws.com/auth/login", new WWWForm());
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            yield return null;
        }
        else
        {
            string text = request.downloadHandler.text;
            Debug.Log("Response text:" + text);
            yield return new RejoinResponse().CreateFromJSON(text);
        }
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
}
