using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public static IntroManager instance;
    public GameObject loadingUI;
    public GameObject crossfade;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        anim = crossfade.GetComponent<Animator>();
        LevelLoader.instance.ClearCrossFade();
    }
   

    public void loadScene(string str)
    {
        LevelLoader.instance.loadScene(str);
    }
}
