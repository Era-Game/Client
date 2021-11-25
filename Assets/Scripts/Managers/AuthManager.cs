using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerPrefsList;
using System;
using UnityEngine.SceneManagement;
using static Auth;
using CoroutineHelper;

[Flags] enum StatusOfUser
{
    LOGGED_IN,
    LOGGED_OUT,
    NOT_ACTIVATED,
    BANNED
}

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

    public string GetUserAuthStatus()
    {
        return PlayerPrefs.GetString(PlayerPrefsList.User.AuthStatus.ToString("g"));
    }

    private IEnumerator CheckStatusOfUser()
    {
        yield return new WaitForEndOfFrame();

        string statusString = GetUserAuthStatus();

        // check user's current status type
        if (Enum.TryParse(statusString, out statusOfUser))
        {
            switch (statusOfUser)
            {
                case StatusOfUser.LOGGED_IN:
                    Debug.Log("user has logged in before");
                    //StartCoroutine(loginProcedure());
                    break;

                case StatusOfUser.LOGGED_OUT:
                    Debug.Log("user has logged out");
                    //StartCoroutine(login_wait_for_start());
                    break;

                case StatusOfUser.NOT_ACTIVATED:
                    Debug.Log("user wasn't activated");
                    //SendVerificationEmail();
                    break;

                case StatusOfUser.BANNED:
                    Debug.Log("user was banned");
                    break;

                default:
                    break;

            }
        } else // first time to play (StatusOfUser=None)
        {
            Debug.Log("first time to play");
            StartCoroutine(GotoLoginScene());
        }
    }

    private IEnumerator GotoLoginScene()
    {
        yield return null;

        LevelLoader.instance.display_loading_screen();
        SceneManager.LoadScene("Login");
    }

    public void HandleLoginBtn()
    {
        // fetch the input data from Login Scene
        string email = LoginManager.instance.getLoginEmail();
        string password = LoginManager.instance.getLoginPassword();
        Debug.Log("Trying to login, got email: " + email + " and password: " + password);
        StartCoroutine(login(email, password));
    }

    private IEnumerator login(string email, string password)
    {
        CoroutineWithData cd = new CoroutineWithData(this, Auth.instance.Login(email, password));
        yield return cd.coroutine;
        
        if (cd.result != null)
        {
            Debug.Log("User log in successfully!");
            LoginResponse response = (LoginResponse) cd.result;
            UIManager.instance.displayWarning("Successs", "Welcome Back " + response.username + "!");
            PlayerManager.instance.initPlayManagerUtilities();
            PlayerManager.instance.setLoggedIn(true);
            yield return new WaitForSeconds(1.5f);
            LevelLoader.instance.display_loading_screen();
        }
        else if (cd.result == null)
        {
            LoginManager.instance.update_warningLoginText("login error");
            UIManager.instance.displayWarning("Error", "an error occurred while logging in");
        }

        //{
        //    if (User.IsEmailVerified)
        //    {
        //        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        //        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        //        if (DBTask.Exception != null)
        //        {
        //            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        //        }
        //        else if (DBTask.Result.Value == null)
        //        {
        //            //No data exists yet
        //            Debug.Log("Account does not have existing data, initialzing data...");
        //            initializeAccount(User.DisplayName);
        //        }

        //        UIManager.instance.displayWarning("Successs", "Welcome Back " + User.DisplayName + "!");
        //        //Debug.Log("Loading Lobby...");
        //        PlayerManager.instance.initPlayManagerUtilities();
        //        PlayerManager.instance.setLoggedIn(true);
        //        ClearInputFeilds();
        //        yield return new WaitForSeconds(1.5f);
        //        LevelLoader.instance.display_loading_screen();
        //    }
        //    else
        //    {
        //        StartCoroutine(SendVerificationEmail());
        //    }
        //}
    }
}
