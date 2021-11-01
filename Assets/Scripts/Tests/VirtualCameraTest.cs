using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraTest : MonoBehaviour
{
    public GameObject Reset_Pos; // Default auto-pilot position
    public CinemachineVirtualCamera vCam; // The actual camera
    public Transform aim; //The player's transform

    public bool state;

    private void Start()
    {
        state = true;
        gameObject.transform.parent = Reset_Pos.transform;
        gameObject.transform.position = gameObject.transform.parent.position;
        gameObject.transform.LookAt(aim);
        vCam.LookAt = aim;
    }

    private void Update()
    {
        if (state)
        {
            gameObject.transform.position = gameObject.transform.parent.position;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("On trigger enter activated");
        StartCoroutine(auto_pilot());
    }

    public void resetCamera()
    {
        /*flycamera.transform.LookAt(aim);
        flycamera.transform.position = Reset_Pos.transform.position;
        flycamera.transform.LookAt(aim);*/
        if (state)
        {
            StartCoroutine(control_mode());

        }
        else
        {
            StartCoroutine(auto_pilot());
        }
    }

    private IEnumerator auto_pilot()
    {
        state = true;

        yield return new WaitForEndOfFrame();

        gameObject.transform.parent = Reset_Pos.transform;
        yield return new WaitForSeconds(0.1f);
        gameObject.transform.position = gameObject.transform.parent.position;

        gameObject.transform.LookAt(aim);
        vCam.LookAt = aim;
    }

    private IEnumerator control_mode()
    {
        state = false;

        yield return new WaitForEndOfFrame();

        gameObject.transform.parent = null;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 50, gameObject.transform.position.z);
        vCam.LookAt = gameObject.transform;
        gameObject.transform.eulerAngles = new Vector3(14, -42, -13f);
    }
}
