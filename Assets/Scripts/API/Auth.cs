using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CoroutineHelper;
using UnityEngine;
using UnityEngine.Networking;

public class Auth : MonoBehaviour
{
    public static Auth instance;

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;

        public LoginRequest(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    [Serializable]
    public class LoginResponse
    {
        public string status;
        public string username;
        public string access_token;

        public LoginResponse CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<LoginResponse>(jsonString);
        }
    }

    [Serializable]
    public class ErrorResponse
    {
        public string status;

        public LoginResponse CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<LoginResponse>(jsonString);
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


    public IEnumerator Login(string email, string password)
    {
        LoginRequest loginReq = new LoginRequest(email, password);
        string jsonData = JsonUtility.ToJson(loginReq);
        Debug.Log(jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = UnityWebRequest.Post("http://localhost/auth/login", new WWWForm());
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
            yield return new LoginResponse().CreateFromJSON(text);
        }
    }
}
