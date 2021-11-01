using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetManager : MonoBehaviour
{

    public static PetManager instance;
    //Public Variables -> Prefab Skins
    [Header("Avaliable Pets")]
    public GameObject[] pets;
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
        return pets[ID];
    }

}
