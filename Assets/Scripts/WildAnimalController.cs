using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildAnimalController : MonoBehaviour
{
    [Header("Wild Animal Attributes")]
    [SerializeField] string name;
    [SerializeField] string displayText;
    [SerializeField] float movement_speed;
    [SerializeField] float multiplier;
    [SerializeField] Ability wild_Ability;

    private Animator animator;
    private Rigidbody rb;
    float cooldownTime;
    float activeTime;
    bool activate;

    enum AbilityState {ready, active, cooldown}

    AbilityState state = AbilityState.ready;

    //Private Memeber Variables
    private bool moving = false;
    private bool exist = true;
    float randRotation;
    float randMovementTime;
    float randIdleTime;

    // Start is called before the first frame update
    void Start()
    {
        exist = true;
        moving = false;
        animator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving && exist)
        {
            StartCoroutine(AI_Movement());
        }

    }

    private void OnTriggerStay(Collider other)
    {
        randRotation = Random.Range(-5, 5);
        transform.Rotate(0, randRotation, 0);
    }

    IEnumerator AI_Movement()
    {
        moving = true;
        yield return new WaitForEndOfFrame();

        randRotation = Random.Range(-45, 45);
        randMovementTime = Random.Range(5, 30);
        randIdleTime = Random.Range(5, 10);

        transform.Rotate(0, randRotation, 0);
        animator.SetInteger("state", 1);

        
        while (randMovementTime > 0)
        {
            randMovementTime -= Time.deltaTime; 
            rb.velocity = transform.forward * movement_speed;
            yield return null;
        }

        animator.SetInteger("state", 0);
        rb.velocity = new Vector3(0, 0, 0);
        yield return new WaitForSecondsRealtime(randIdleTime);

        moving = false;

    }



    public Ability getWildAbility()
    {
        return wild_Ability;
    }

    public string getDisplayText()
    {
        return displayText;
    }

    public void clicked()
    {
        exist = false;
        StartCoroutine(ClickedCoroutine());
    }

    IEnumerator ClickedCoroutine()
    {
        animator.SetTrigger("clicked"); 
        yield return new WaitForSecondsRealtime(0.5f);
        gameObject.GetComponentInParent<SpawnerManager>().despawn();
    }
    public void kill()
    {
        exist = false;
        StartCoroutine(killCoroutine());
    }

    IEnumerator killCoroutine()
    {
        animator.SetInteger("state", 2);
        yield return new WaitForSecondsRealtime(1f);
        gameObject.GetComponentInParent<SpawnerManager>().despawn();
    }

    public float get_multiplier()
    {
        return multiplier;
    }

}
