using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.SceneManagement;


public class WinSceneManager : MonoBehaviour
{

    [Header("Win Scene Utilities")]
    public TMP_Text usernameText;
    public GameObject character1;
    public Sprite[] skins;

    private bool canPush = true;

    public void back_to_lobby()
    {
        if (canPush)
        {
            canPush = false;
            StartCoroutine(ResetAllNormalRaceData());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        usernameText.text = FirebaseManager.instance.winPlayerUsername;
        character1.AddComponent<SpriteRenderer>();
        character1.GetComponent<SpriteRenderer>().sprite = skins[FirebaseManager.instance.winPlayerSkinID];
    }

    IEnumerator ResetAllNormalRaceData()
    {
        string player_uid = PlayerManager.instance.getData("uid");
        FirebaseManager.instance.ResetNormalRaceData(player_uid);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Lobby");
    }
}
