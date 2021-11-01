using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement3DTest : MonoBehaviour
{
    public Text text;

    public int stepSize = 100;

    float distance = 0;
    int totalSteps = 0;
    int prevTotalSteps = 0;
    float velocity = 0;
    float stepFreq = 0;

    bool movementCompleted;

    Vector3 startPos;
    Vector3 targetPosition;
    Vector3 currPos;

    Vector3 smoothDampVelocity = Vector3.zero;
    float smoothDampTime = 0.1f;

    public float speed = 10f;
    public float t = 0.5f;
    public float distanceFactor = 100f;
    

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("calculate_distance", 0.0f, 0.5f);
        InvokeRepeating("calculate_step_frequency", 0.0f, 2f);
        movementCompleted = true;
        startPos = transform.position;
        currPos = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Distance: " + distance + ", Velocity: " + velocity;



        if (movementCompleted)
        {
            movementCompleted = false;
            targetPosition = new Vector3(startPos.x + distance * distanceFactor, currPos.y, startPos.z);
            StartCoroutine(moveObject());
        }

    }

    private void calculate_step_frequency()
    {
        int currSteps = totalSteps - prevTotalSteps;
        prevTotalSteps = totalSteps;
        stepFreq = currSteps / 2;
        //Debug.Log("stepFreq: " + stepFreq);
    }

    private void calculate_distance()
    {
        float acceleration;

        if (stepFreq >= 1 && stepFreq < 2)
        {
            stepFreq += 0.5f;
        }

        if (stepFreq >= 1)
        {
            acceleration = 1.5f * Mathf.Log10(stepFreq);
        }
        else
        {
            acceleration = -0.1f;
        }

        if ((velocity * 0.7f) + acceleration <= 0)
        {
            velocity = 0;
        }
        else
        {
            velocity = velocity * 0.7f + acceleration;
        }


        distance += velocity * 0.5f;

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
    public void addTotalSteps()
    {
        totalSteps += stepSize;
    }
}
