using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayGamePlayerController : MonoBehaviour
{
    public static RelayGamePlayerController instance;

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
    private string imageURL;

    InGameData inGameData;

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
        prevRunner = "prev";
        imageURL = "placeholder";
        movementCompleted = true;
        skinUpdate = true;
        updateTime = 0;
    }

    private void Awake()
    {
        Reset_Variables();
        instance = this;
    }
    void Start()
    {
        startPoint = transform;
        spawnPoint = transform;

        audioSource = GetComponent<AudioSource>();

        inGameData = API.instance.getInGameData();
        animator = null;

        TOTAL_DISTANCE = inGameData.relay_totalsteps;
        DISTANCE_THRESHOLD = inGameData.relay_steps_threshold;

        startPos = transform.position;
        currPos = startPos;
        UNIT = Vector3.Distance(startPos, endPoint.position) / TOTAL_DISTANCE;
        storedTurns = (int)distance / DISTANCE_THRESHOLD;

        despawn_VFX.SetActive(false);
        spawn_VFX.SetActive(false);

        StartCoroutine(transition());
        prevRunner = runner;
    }

    // Update is called once per frame
    void Update()
    {
        inGameData = API.instance.getInGameData();

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

        if (runner != prevRunner && !inGameData.gameEnd)
        {
            if (distance < TOTAL_DISTANCE)
            {
                StartCoroutine(transition());
            }
            prevRunner = runner;
        }

        if (playerIndex == 0 && RelayGameManager.instance.getPerspective())
        {
            clone.GetComponent<PlaneTexture>().showNameTag();
        }
        else
        {
            clone.GetComponent<PlaneTexture>().hideNameTag();
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
    private IEnumerator transition()
    {
        despawn_VFX.SetActive(true);
        DestroyImmediate(clone);
        yield return new WaitForSeconds(2f);
        UpdateData();
        spawnPoint = transform;
        clone = Instantiate(SkinManager.instance.getSkinByID(skinID), spawnPoint.position, Quaternion.identity);
        clone.transform.parent = transform;
        clone.transform.Rotate(0, -90, 0);

        clone.GetComponent<PlaneTexture>().setName(runner);
        clone.GetComponent<PlaneTexture>().setImage(imageURL);

        animator = clone.GetComponent<Animator>();
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

            clone.GetComponent<PlaneTexture>().setName(runner);
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
       

        float _distance = (float)RelayGameManager.instance.getOtherTeamDistance()[playerIndex];
        if (_distance > distance)
        {
            distance = _distance;
        }

        if (inGameData.gameEnd &&　distance < inGameData.relay_totalsteps * 1.06)
        {
            distance += 0.5f * Time.deltaTime;
        }
        velocity = (float)RelayGameManager.instance.getOtherTeamVelocity()[playerIndex];
        skinID = RelayGameManager.instance.getOtherTeamSkinIDs()[playerIndex];

        if ((RelayGameManager.instance.getOtherTeamRunners()[playerIndex] != null && RelayGameManager.instance.getOtherTeamRunners()[playerIndex] != "") && !inGameData.gameEnd)
        {
            runner = RelayGameManager.instance.getOtherTeamRunners()[playerIndex].ToString();
            if (clone != null)
            {
                clone.GetComponent<PlaneTexture>().setName(runner);
                
            }
        }

        if (imageURL != RelayGameManager.instance.getOtherTeamImageURL()[playerIndex] && clone != null && !inGameData.gameEnd)
        {
            imageURL = RelayGameManager.instance.getOtherTeamImageURL()[playerIndex];
            clone.GetComponent<PlaneTexture>().setImage(imageURL);
        }
        

        TOTAL_DISTANCE = inGameData.relay_totalsteps;
        DISTANCE_THRESHOLD = inGameData.relay_steps_threshold;
    }

    public void UpdateAnimation()
    {
        if (runner != "placeholder" && runner != null && runner != "")
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
