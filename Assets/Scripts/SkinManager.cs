using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager instance;

    //Public Variables -> Prefab Skins
    [Header("Avaliable Skins")]
    public GameObject[] skins;
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

    public GameObject getSkinByID(int ID)
    {
        return skins[ID];
    }
}
