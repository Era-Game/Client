using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject verifyEmailUI;
    public NotificationManager warningNotification;
    public ModalWindowManager verifyWindow;
    public TMP_Text notification_description;
    public TMP_Text notification_title;

    public Text verifyEmailText;

    private void Awake()
    {
        LoginScreen();
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI

    public void ClearScreen() //Turn off all screens
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        verifyEmailUI.SetActive(false);
    }

    public void LoginScreen() //Back button
    {
        ClearScreen();
        loginUI.SetActive(true);
    }

    public void RegisterScreen() // Regester button
    {
        ClearScreen();
        registerUI.SetActive(true);
    }
    public void LoadTestScene()
    {
        ClearScreen();
        SceneManager.LoadScene("Test Scene");
    }

    public void LoadGameplayScene()
    {
        ClearScreen();
        SceneManager.LoadScene("Game Play");
    }

    public void displayWarning(string title, string message)
    {
        //LoginManager.instance.clearInputFeilds();
        warningNotification.title = title;
        warningNotification.description = message;
        warningNotification.UpdateUI();
        warningNotification.OpenNotification();
    }
    public void AwaitVerification(bool _emailSent, string _email, string _message)
    {
        ClearScreen();
        //verifyEmailUI.SetActive(true);

        if (_emailSent)
        {
            verifyEmailText.text = $"Sent Email!\nPlease Verify {_email}";
            verifyWindow.descriptionText = $"Sent Email!\nPlease Verify {_email}";
        }
        else
        {
            verifyEmailText.text = $"Email Not Sent: {_message}\nPlease Verify {_email}";
            verifyWindow.descriptionText = $"Email Not Sent: {_message}\nPlease Verify {_email}";
        }
        verifyWindow.UpdateUI();
        verifyWindow.OpenWindow();
    }

    public void resend_verification()
    {
        StartCoroutine(FirebaseManager.instance.SendVerificationEmail());
    }
    public void close_verify_window()
    {
        verifyWindow.CloseWindow();
        LoginScreen();
    }
}
