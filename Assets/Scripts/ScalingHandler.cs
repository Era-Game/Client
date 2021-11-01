using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingHandler : MonoBehaviour
{
    [Header("Scale Objects")]
    public GameObject[] gameObjects;

    private float screenWidth;
    private float screenHeight;
    // Start is called before the first frame update
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        for (int i = 0; i < gameObjects.Length; ++i)
        {
            gameObjects[i].transform.localScale = new Vector3(screenWidth / 4, screenHeight, -1);
            gameObjects[i].transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }

    private void Update()
    {
        for (int i = 0; i < gameObjects.Length; ++i)
        {
            gameObjects[i].transform.localScale = new Vector3(screenWidth / 4, screenHeight, -1);
            gameObjects[i].transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }
}
