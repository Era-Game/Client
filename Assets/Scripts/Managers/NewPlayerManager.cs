using UnityEngine;
using System.Collections;
using Model;

public class NewPlayerManager : MonoBehaviour
{
    public static NewPlayerManager instance;
    public bool isRejoin = false;

    public User user;

    //Public Variables -> Public Constants
    public int NUM_SKINS = 9;
    public int NUM_PETS = 4;
    public int MAX_SKILL_NUM = 3;

    // Use this for initialization
    void Start()
    {
        Debug.Log("[NewPlayerManager] Start");
    }

    private void Awake()
    {
        isRejoin = false;
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

    // Update is called once per frame
    void Update()
    {

    }

    private void setUser()
    {

    }
}
