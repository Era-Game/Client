using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public static LoginManager instance;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    private void Awake()
    {
        //Screen.orientation = ScreenOrientation.Portrait;
        instance = this;
        LevelLoader.instance.ClearCrossFade();
    }
    public string getLoginEmail()
    {
        Debug.Log("Got Login Email: " + emailLoginField.text);
        return emailLoginField.text;
    }

    public string getLoginPassword() 
    {
        Debug.Log("Got Login Email: " + passwordLoginField.text);
        return passwordLoginField.text;
    }
    public void clearInputFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }
    public string getRegisterUsername()
    {
        return usernameRegisterField.text;
    }

    public string getRegisterEmail()
    {
        return emailRegisterField.text;
    }

    public string getRegisterPassword()
    {
        return passwordRegisterField.text;
    }

    public string getRegisterComfirmPassword()
    {
        return passwordRegisterVerifyField.text;
    }

    public void update_emailLoginField(string txt)
    {
        emailLoginField.text = txt;
    }

    public void update_passwordLoginField(string txt)
    {
        passwordLoginField.text = txt;
    }

    public void update_warningLoginText(string txt)
    {
        warningLoginText.text = txt;
    }

    public void update_usernameRegisterField(string txt)
    {
        usernameRegisterField.text = txt;
    }

    public void update_emailRegisterField(string txt)
    {
        emailRegisterField.text = txt;
    }

    public void update_passwordRegisterField(string txt)
    {
        passwordRegisterField.text = txt;
    }

    public void update_passwordRegisterVerifyField(string txt)
    {
        passwordRegisterVerifyField.text = txt;
    }
    public void login()
    {
        FirebaseManager.instance.LoginButton();
    }

    public void register()
    {
        FirebaseManager.instance.RegisterButton();
    }

    public void logout()
    {
        FirebaseManager.instance.LogOutButton();
    }
}
