using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Managers;

public class PlaneTexture : MonoBehaviour
{
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject planeObject;
    [SerializeField] TMP_Text username;

    public string currentURL  = "placeholder";

    private readonly string Default_Photo_URL = "https://www.linkpicture.com/q/Untitled_Artwork_3.png";

    private void Update()
    {
        parentObject.transform.rotation = Quaternion.Euler(parentObject.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y - 180, parentObject.transform.localEulerAngles.z);
    }

    public void setImage(string url)
    {
        currentURL = url;
        StartCoroutine(GetText(url));
    }

    public void setName(string name)
    {
        username.text = name;
    }

    public void hideNameTag()
    {
        parentObject.SetActive(false);
    }

    public void showNameTag()
    {
        parentObject.SetActive(true);
    }
    
    IEnumerator GetText(string url)
    {

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                planeObject.GetComponent<MeshRenderer>().material.mainTexture = DownloadHandlerTexture.GetContent(uwr);
                Debug.Log("Set Image. Got picture from: " + currentURL);
            }
        }

        //planeObject.transform.rotation =
        //    Quaternion.Euler(planeObject.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, planeObject.transform.localEulerAngles.z);
    }

    IEnumerator GetText()
    {
        string url = PlayerManager.instance.getData("Profile_Image_URL");

        yield return new WaitForSeconds(0.4f);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                planeObject.GetComponent<MeshRenderer>().material.mainTexture = DownloadHandlerTexture.GetContent(uwr);
            }
        }

        //planeObject.transform.rotation =
        //    Quaternion.Euler(planeObject.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, planeObject.transform.localEulerAngles.z);
    }
}
