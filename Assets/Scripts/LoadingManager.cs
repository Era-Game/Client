using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void loadScene(string str)
    {
        SceneManager.LoadScene(str);
    }
}
