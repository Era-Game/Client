using UnityEngine;
using System.Collections;
using Cinemachine;
using Managers;

public class FlyCamera : MonoBehaviour
{
    public static FlyCamera instance;

    [Header("Joystick Settings")]
    public float cameraSensitivityX = 3f;
    public float cameraSensitivityY = 0.8f;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 4;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    public Joystick joystick;
    public GameObject flyCamReset;

    public GameObject mainCamera;
    public GameObject joystick_object;
    public GameObject joystick_box;
    
    [Space]

    [Header("Double Tap Settings")]
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;

    public float tapRange = 10;
    private float prevTime = 0;
    private float currTime = 0;
    public float doubleTapInterval = 1;

    [Space]

    [Header("Fly Camera Components")]
    public GameObject Reset_Pos; // Default auto-pilot position
    public CinemachineVirtualCamera vCam; // The actual camera
    public Transform aim; //The player's transform
    private View_Mode mode;
    InGameData igData;

    enum View_Mode {auto_pilot, manual_control}

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        prevTime = 0;
        currTime = 0;
        StartCoroutine(auto_pilot());
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("On trigger enter activated");
        StartCoroutine(auto_pilot());
    }
    */
    /*
    private void OnTriggerStay(Collider other)
    {
        Vector3 overlap = new Vector3(0f, 0f, 0f);
        if (transform.localPosition != overlap)
        {
            Debug.Log("Camera in dead zone...Resetting");
            StartCoroutine(auto_pilot());
        }
        
    }*/

    void Update()
    {
        igData = API.instance.getInGameData();

        if (transform.position.x <= -2839.6)
        {
            transform.position = new Vector3(-2839f, transform.position.y, transform.position.z);
        }

        if (transform.position.x > Reset_Pos.transform.position.x + 0.5f)
        {
            StartCoroutine(auto_pilot());
        }

        if (!igData.myTurn && transform.position.x >= -2839.6)
        {
            //if (mode == View_Mode.auto_pilot && gameObject.transform.parent.position != null)
            //{
            //    gameObject.transform.position = gameObject.transform.parent.position;
            //}

            currTime += Time.deltaTime;

            if (RelayGameManager.instance.currentCameraIndex == 7)
            {
                Vector2 joystickAreaMin = new Vector2(joystick.transform.position.x - 150f, joystick.transform.position.y - 150f);
                Vector2 joystickAreaMax = new Vector2(joystick.transform.position.x + 150f, joystick.transform.position.y + 150f);
                Vector2 joystickBoxMin = new Vector2(joystick_box.transform.position.x - 509f, joystick_box.transform.position.y - 730f);
                Vector2 joystickBoxMax = new Vector2(joystick_box.transform.position.x + 509f, joystick_box.transform.position.y + 730f);

                Vector2 flycamResetAreaMin = new Vector2(flyCamReset.transform.position.x - 50f, flyCamReset.transform.position.y - 50f);
                Vector2 flycamResetAreaMax = new Vector2(flyCamReset.transform.position.x + 50f, flyCamReset.transform.position.y + 50f);
                
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began &&
                        (Input.GetTouch(0).position.x > joystickBoxMin.x && Input.GetTouch(0).position.x < joystickBoxMax.x) &&
                            (Input.GetTouch(0).position.y > joystickBoxMin.y && Input.GetTouch(0).position.y < joystickBoxMax.y))
                    {
                        joystick_object.transform.position = Input.GetTouch(0).position;
                        StartCoroutine(control_mode());
                    }
                    
                }

                transform.position += Vector3.right * joystick.Vertical * normalMoveSpeed * Time.deltaTime;
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startTouchPos = Input.GetTouch(0).position;
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {

                endTouchPos = Input.GetTouch(0).position;

                Vector2 Distance = endTouchPos - startTouchPos;

                if (Mathf.Abs(Distance.x) < tapRange && Mathf.Abs(Distance.y) < tapRange)
                {
                    if (currTime - prevTime < doubleTapInterval && transform.position.x < Reset_Pos.transform.position.x)
                    {
                        StartCoroutine(auto_pilot());
                    }

                    prevTime = currTime;
                }
            }

        }
    }

    private IEnumerator control_mode()
    {
        mode = View_Mode.manual_control;

        yield return new WaitForEndOfFrame();

        gameObject.transform.parent = null;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        vCam.LookAt = gameObject.transform;
    }

    public void setAutoPilot()
    {
        StartCoroutine(auto_pilot());
    }
    private IEnumerator auto_pilot()
    {
        mode = View_Mode.auto_pilot;

        yield return new WaitForEndOfFrame();

        gameObject.transform.parent = Reset_Pos.transform;
        yield return new WaitForSeconds(0.1f);
        gameObject.transform.position = gameObject.transform.parent.position;

        gameObject.transform.LookAt(aim);
        vCam.LookAt = aim;
    }
}
