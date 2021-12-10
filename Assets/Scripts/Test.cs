using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.SceneManagement;
using Managers;

public class Test : MonoBehaviour
{
    [Header("Random Shit")]
    public TMP_Text testText;
    public GameObject mainCharacter;
    public Sprite[] skins;

    private int skinNum;


    public void TestButton()
    {
        //StartCoroutine(LoadUserDataToLobby());
        if (skinNum >= skins.Length)
        {
            skinNum = 0;
        }

        mainCharacter.AddComponent<SpriteRenderer>();
        mainCharacter.GetComponent<SpriteRenderer>().sprite = skins[skinNum++];

    }

    void Start()
    {
        skinNum = 0;
    }

    private IEnumerator LoadUserDataToLobby()
    {

        //Get the currently logged in user data
        var DBTask = FirebaseManager.instance.DBreference.Child("users").Child(FirebaseManager.instance.User.UserId).GetValueAsync();

        Debug.Log("Trying to get DB Task");

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        Debug.Log("Got DB Task");

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            testText.text = ": 0";
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            testText.text = "x " + snapshot.Child("coins").Value.ToString();
        }
    }
}
