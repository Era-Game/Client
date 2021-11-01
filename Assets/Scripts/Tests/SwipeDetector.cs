using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeDetector : MonoBehaviour
{
    public Text HUD_Text;

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 currentTouchPos;

    private bool stopTouch = false;

    public float swipeRange;
    public float tapRange;

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPos = Input.GetTouch(0).position;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            currentTouchPos = Input.GetTouch(0).position;
            Vector2 Distance = currentTouchPos - startTouchPos;

            if (!stopTouch)
            {
                if (Distance.x < -swipeRange)
                {
                    //Swipe Left 
                    HUD_Text.text = "Left";
                    stopTouch = true;
                }
                else if(Distance.x > swipeRange)
                {
                    //Swipe Right
                    HUD_Text.text = "Right";
                    stopTouch = true;
                }
                else if (Distance.y < -swipeRange)
                {
                    //Swipe Down
                    HUD_Text.text = "Down";
                    stopTouch = true;
                }
                else if (Distance.y > swipeRange)
                {
                    //Swipe Up
                    HUD_Text.text = "UP";
                    stopTouch = true;
                }
            }

        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            stopTouch = false;

            endTouchPos = Input.GetTouch(0).position;

            Vector2 Distance = endTouchPos - startTouchPos;

            if (Mathf.Abs(Distance.x) < tapRange && Mathf.Abs(Distance.y) < tapRange)
            {
                HUD_Text.text = "tap";
            }
        }

    }
}
