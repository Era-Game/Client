using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    private Animator anim;

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

        anim = gameObject.GetComponent<Animator>();
    }

    public void display_loading_screen()
    {
        anim.SetInteger("state", 2);
    }

    public void close_loading_screen()
    {
        anim.SetInteger("state", 0);
    }

    public void loadScene(string str)
    {
        StartCoroutine(loadSceneCoroutine(str));
    }

    public void ClearCrossFade()
    {
        anim.SetInteger("state", 0);
    }

    IEnumerator loadSceneCoroutine(string str)
    {
        anim.SetInteger("state", 0);
        yield return new WaitForSeconds(0.5f);
        anim.SetInteger("state", 1);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(str);
    }
}
