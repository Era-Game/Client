using System;
using System.Collections;
using System.Text;
using Model;
using UnityEngine;
using UnityEngine.Networking;

namespace APIs
{
    public class AuthAPI : MonoBehaviour
    {
        class Request<T>
        {
            public T Data { get; set; }

            public UnityWebRequest Post(string url)
            {
                string jsonData = JsonUtility.ToJson(Data);
                Debug.Log(jsonData);
                byte[] postData = Encoding.UTF8.GetBytes(jsonData);

                UnityWebRequest request = UnityWebRequest.Post(url, new WWWForm());
                request.uploadHandler = new UploadHandlerRaw(postData);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                return request;
            }
        }

        public static AuthAPI instance;

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
        public class LoginWithTokenRequest
        {
            public string accessToken;

            public LoginWithTokenRequest(string token)
            {
                this.accessToken = token;
            }
        }

        [Serializable]
        public class LoginResponse
        {
            public string status;
            public User user;
            public string access_token;

            public LoginResponse CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<LoginResponse>(jsonString);
            }
        }

        public IEnumerator Login(string email, string password)
        {
            Debug.Log("[AuthAPI] start login");
            Request<LoginRequest> req = new Request<LoginRequest>();
            req.Data = new LoginRequest(email, password);
            UnityWebRequest request = req.Post("http://ec2-3-112-239-208.ap-northeast-1.compute.amazonaws.com/auth/login");
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

        public IEnumerator LoginWithToken(string token)
        {
            Request<LoginWithTokenRequest> req = new Request<LoginWithTokenRequest>();
            req.Data = new LoginWithTokenRequest(token);
            UnityWebRequest request = req.Post("http://ec2-3-112-239-208.ap-northeast-1.compute.amazonaws.com/auth/login");
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
}

