using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using UnityEngine.SceneManagement;
using static AuthAPI;
using CoroutineHelper;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;
    private StatusOfUser statusOfUser; // enum list

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckStatusOfUser());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
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

    private IEnumerator CheckStatusOfUser()
    {
        yield return new WaitForEndOfFrame();

        string statusString = PlayerPrefsHelper.GetAuthStatus();
        Debug.Log("[AuthManager] Previous status of user: " + statusString);

        // check user's current status type
        if (Enum.TryParse(statusString, out statusOfUser))
        {
            switch (statusOfUser)
            {
                case StatusOfUser.LOGGED_IN:
                    Debug.Log("[AuthManager] user has logged in before");
                    //StartCoroutine(loginWithAccessToken());
                    break;

                case StatusOfUser.LOGGED_OUT:
                    Debug.Log("[AuthManager] user has logged out");
                    //StartCoroutine(login_wait_for_start());
                    break;

                case StatusOfUser.NOT_ACTIVATED:
                    Debug.Log("[AuthManager] user wasn't activated");
                    //SendVerificationEmail();
                    break;

                case StatusOfUser.BANNED:
                    Debug.Log("[AuthManager] user was banned");
                    break;

                default:
                    break;

            }
        } else // first time to play (StatusOfUser=None)
        {
            Debug.Log("[AuthManager] first time to play");
            LevelLoader.instance.loadScene("Login");
        }
    }

    public void HandleLoginBtn()
    {
        // fetch the input data from Login Scene
        string email = LoginManager.instance.getLoginEmail();
        string password = LoginManager.instance.getLoginPassword();
        Debug.Log("[AuthManager] Trying to login, got email: " + email + " and password: " + password);
        StartCoroutine(login(email, password));
    }

    private IEnumerator login(string email, string password)
    {
        CoroutineWithData cd = new CoroutineWithData(this, AuthAPI.instance.Login(email, password));
        yield return cd.coroutine;
        
        if (cd.result != null)
        {
            LoginResponse response = (LoginResponse) cd.result;
            UIManager.instance.displayWarning("Success", "Welcome Back " + response.user.username + "!");

            PlayerPrefsHelper.SetAuthStatus(StatusOfUser.LOGGED_IN.ToString("g"));
            PlayerPrefsHelper.SetDefaultData(response.user);

            //TODO: This line would trigger PlayerManager Update() condition
            PlayerManager.instance.initPlayManagerUtilities();
            PlayerManager.instance.setLoggedIn(true);
            yield return new WaitForSeconds(1.5f);
            LevelLoader.instance.display_loading_screen();

            Debug.Log("User log in successfully!");

            LevelLoader.instance.loadScene("lobby");
        }
        else if (cd.result == null)
        {
            LoginManager.instance.update_warningLoginText("login error");
            UIManager.instance.displayWarning("Error", "an error occurred while logging in");
        }
    }

    private IEnumerator loginWithAccessToken(string accessToken)
    {
        CoroutineWithData cd = new CoroutineWithData(this, AuthAPI.instance.LoginWithToken(accessToken));
        yield return cd.coroutine;

        if (cd.result != null)
        {
            LoginResponse response = (LoginResponse)cd.result;
            UIManager.instance.displayWarning("Success", "Welcome Back " + response.user.username + "!");

            PlayerPrefsHelper.SetAuthStatus(StatusOfUser.LOGGED_IN.ToString("g"));
            PlayerPrefsHelper.SetDefaultData(response.user);

            //TODO: This line would trigger PlayerManager Update() condition
            PlayerManager.instance.initPlayManagerUtilities();
            PlayerManager.instance.setLoggedIn(true);
            yield return new WaitForSeconds(1.5f);
            LevelLoader.instance.display_loading_screen();

            Debug.Log("User log in successfully!");

            LevelLoader.instance.loadScene("lobby");
        }
        else if (cd.result == null)
        {
            LoginManager.instance.update_warningLoginText("login error");
            UIManager.instance.displayWarning("Error", "an error occurred while logging in");
        }
    }
}
