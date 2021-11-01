using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpectatorPlayerController1 : MonoBehaviour
{
    public static SpectatorPlayerController1 instance;

    [Header("Scene Objects")]
    [SerializeField] int playerIndex;
    [SerializeField] GameObject despawn_VFX;
    [SerializeField] GameObject spawn_VFX;
    [SerializeField] GameObject loadingUI;
    [SerializeField] TMP_Text username;


    [Space]

    [Header("Reference Points")]
    [SerializeField] Transform endPoint;
    Transform startPoint;


    [Space]

    [Header("Audio Source")]
    AudioSource audioSource;


    //Player Data
    private Transform spawnPoint;
    private GameObject clone;
    private Animator animator;
    private int memberCount;
    private int skinID = 0;
    private int currSkinID = 0;
    private float distance;
    private float velocity;
    private string runner;
    private string prevRunner;
    private string teamname;
    private string imageURL;

    SpectatorData sData;

    //Controller Private Variables
    private bool movementCompleted;
    private bool skinUpdate;
    private int storedTurns = 0;
    Vector3 startPos;
    Vector3 targetPosition;
    Vector3 currPos;
    private float updateTime = 0;

    //Constants
    private float UNIT;
    private int TOTAL_DISTANCE = 600;
    private int DISTANCE_THRESHOLD = 100;

    private void Reset_Variables()
    {
        skinID = 0;
        distance = 0;
        velocity = 0;
        runner = "placeholder";
        prevRunner = "placeholder";
        imageURL = "placeholder";
        movementCompleted = true;
        skinUpdate = true;
        updateTime = 0;

        Destroy(clone);
    }

    private void Awake()
    {
        Reset_Variables();
        instance = this;
    }
    void Start()
    {
        StartCoroutine(start_coroutine());
    }

    private IEnumerator start_coroutine()
    {
        startPoint = transform;
        spawnPoint = transform;
        startPos = transform.position;
        currPos = startPos;

        audioSource = GetComponent<AudioSource>();

        API.instance.Update_Spectator_Data(SpectatorMenuManager.instance.getGameID());

        while (!API.instance.dataRecieved)
        {
            yield return null;
        }

        string responseMessage = API.instance.responseMessage;
        long responseStatus = API.instance.statusCode;

        Debug.Log("Respose Return with code (" + responseStatus.ToString() + "): " + responseMessage);

        sData = API.instance.getSpectatorData();

        animator = null;

        TOTAL_DISTANCE = sData.relay_totalsteps;
        DISTANCE_THRESHOLD = sData.relay_steps_threshold;

        UNIT = Vector3.Distance(startPos, endPoint.position) / TOTAL_DISTANCE;
        storedTurns = (int)distance / DISTANCE_THRESHOLD;

        despawn_VFX.SetActive(false);
        spawn_VFX.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        sData = API.instance.getSpectatorData();

        updateTime += Time.deltaTime;
        if (animator == null && clone != null)
        {
            animator = clone.GetComponent<Animator>();
        }

        if (updateTime >= 8)
        {
            LevelLoader.instance.ClearCrossFade();
            loadingUI.SetActive(false);
        }

        UpdateAnimation();
        UpdateData();

        if (movementCompleted)
        {
            movementCompleted = false;
            targetPosition = new Vector3(startPos.x - (distance * UNIT), currPos.y, startPos.z);
            StartCoroutine(moveObject());
        }

        if (runner != prevRunner && !sData.gameEnd)
        {
            if (distance < TOTAL_DISTANCE)
            {
                Debug.Log("Running Transition, Runner: " + runner + ", PrevRunner: " + prevRunner);
                transition();
            }
            prevRunner = runner;
        }

    }
    private void update_skin()
    {
        DestroyImmediate(clone);
        setCharacter();

    }
    private IEnumerator preheat_Countdown(float time)
    {
        yield return new WaitForSecondsRealtime(time);
    }
    private void transition()
    {
        StartCoroutine(play_portal_animation());
        DestroyImmediate(clone);
        UpdateData();
        spawnPoint = transform;
        clone = Instantiate(SkinManager.instance.getSkinByID(skinID), spawnPoint.position, Quaternion.identity);
        clone.transform.parent = transform;
        clone.transform.Rotate(0, -90, 0);

        clone.GetComponent<PlaneTexture>().setName(teamname);
        clone.GetComponent<PlaneTexture>().setImage(imageURL);

        animator = clone.GetComponent<Animator>();
    }

    IEnumerator play_portal_animation()
    {
        despawn_VFX.SetActive(true);
        yield return new WaitForSeconds(3f);
        despawn_VFX.SetActive(false);
    }

    private void setCharacter()
    {
        if (clone == null)
        {
            clone = Instantiate(SkinManager.instance.getSkinByID(skinID), spawnPoint.position, Quaternion.identity);
            clone.transform.parent = transform;
            clone.transform.Rotate(0, -90, 0);

            clone.GetComponent<PlaneTexture>().setName(teamname);
            clone.GetComponent<PlaneTexture>().setImage(imageURL);

            animator = clone.GetComponent<Animator>();
        }

    }

    public void UpdateData()
    {

        if (clone != null && (clone.transform.position.x != clone.transform.parent.position.x || clone.transform.position.z != clone.transform.parent.position.z))
        {
            clone.transform.position = new Vector3(clone.transform.parent.position.x, clone.transform.position.y, clone.transform.parent.position.z);
        }
       

        float _distance = (float)SpectatorSceneManager.instance.getOtherTeamDistance()[playerIndex];
        if (_distance > distance  && distance < sData.relay_totalsteps * 1.06)
        {
            distance = _distance;
        }

        if (sData.gameEnd)
        {
            distance += 0.5f * Time.deltaTime;
        }

        velocity = (float)SpectatorSceneManager.instance.getOtherTeamVelocity()[playerIndex];
        skinID = SpectatorSceneManager.instance.getOtherTeamSkinIDs()[playerIndex];

        if ((SpectatorSceneManager.instance.getOtherTeamRunners()[playerIndex] != null && SpectatorSceneManager.instance.getOtherTeamRunners()[playerIndex] != ""))
        {
            runner = SpectatorSceneManager.instance.getOtherTeamRunners()[playerIndex].ToString();
        }

        if ((SpectatorSceneManager.instance.getOtherTeamRunners()[playerIndex] != null && SpectatorSceneManager.instance.getOtherTeamRunners()[playerIndex] != "") && !sData.gameEnd)
        {
            teamname = SpectatorSceneManager.instance.getOtherTeamNames()[playerIndex].ToString();
            if (clone != null)
            {
                clone.GetComponent<PlaneTexture>().setName(teamname);

            }
        }

        if (imageURL != SpectatorSceneManager.instance.getOtherTeamImageURL()[playerIndex] && clone != null && !sData.gameEnd)
        {
            imageURL = SpectatorSceneManager.instance.getOtherTeamImageURL()[playerIndex];
            clone.GetComponent<PlaneTexture>().setImage(imageURL);
        }
        

        TOTAL_DISTANCE = sData.relay_totalsteps;
        DISTANCE_THRESHOLD = sData.relay_steps_threshold;
    }

    public void UpdateAnimation()
    {
        if (runner != "placeholder" && runner != null && runner != "" && animator != null)
        {
            if (velocity > 3)
            {
                animator.SetInteger("state", 1);
                animator.speed = (float)velocity * 0.333f;

                if (audioSource.pitch == 1.5f || audioSource.pitch == 1.25f)
                {
                    audioSource.Play();
                    audioSource.pitch = (float)velocity * 0.666f;
                }
            }
            else if (velocity > 2)
            {
                animator.SetInteger("state", 1);
                animator.speed = 1;

                if (audioSource.pitch != 1.5f)
                {
                    audioSource.Play();
                    audioSource.pitch = 1.5f;
                }
            }
            else if (velocity > 0) //  || transform.position != targetPosition
            {
                //Debug.Log(velocity);
                animator.SetInteger("state", 1);
                animator.speed = 1;

                if (audioSource.pitch != 1.25f)
                {
                    audioSource.Play();
                    audioSource.pitch = 1.25f;
                }
            }
            else
            {
                animator.SetInteger("state", 0);
                animator.speed = 1;

                audioSource.Stop();
            }
        }
        
    }
    public IEnumerator moveObject()
    {
        movementCompleted = false;
        float totalMovementTime = 2f; //the amount of time you want the movement to take
        float currentMovementTime = 0f;//The amount of time that has passed
        while (Vector3.Distance(transform.localPosition, targetPosition) > 0)
        {
            currentMovementTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(currPos, targetPosition, currentMovementTime / totalMovementTime);
            yield return null;
        }
        currPos = transform.localPosition;
        movementCompleted = true;
    }
}
