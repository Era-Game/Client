using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAIMovement : MonoBehaviour
{
    bool moving;
    float randRotation;
    float randMovementTime;
    float randIdleTime;
    public float movementSpeed; 

    private Animator animator;
    private Rigidbody rb;

    private void Start()
    {
        moving = false;
        rb = gameObject.GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!moving)
        {
            StartCoroutine(AI_Movement());
        }
    }

    IEnumerator AI_Movement()
    {
        moving = true;
        yield return new WaitForEndOfFrame();

        randRotation = Random.Range(-45, 45);
        randMovementTime = Random.Range(5, 30);
        randIdleTime = Random.Range(5, 10);

        transform.Rotate(0, randRotation, 0);

        while (randMovementTime > 0)
        {
            randMovementTime -= Time.deltaTime;
            rb.velocity = transform.forward * movementSpeed;
            yield return null;
        }

        rb.velocity = new Vector3(0, 0, 0);
        yield return new WaitForSecondsRealtime(randIdleTime);

        moving = false;

    }
}
