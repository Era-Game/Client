using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPhaseDetector : MonoBehaviour
{

    public static TouchPhaseDetector instance;

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 currentTouchPos;

    private bool stopTouch = false;

    private float swipeRange = 50;
    private float tapRange = 10;

    public GameObject hitPlayer;

    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        /*
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
                    stopTouch = true;
                }
                else if (Distance.x > swipeRange)
                {
                    //Swipe Right
                    stopTouch = true;
                }
                else if (Distance.y < -swipeRange)
                {
                    //Swipe Down
                    stopTouch = true;
                }
                else if (Distance.y > swipeRange)
                {
                    //Swipe Up
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
                Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 7))
                {
                    if (hit.collider != null)
                    {
                        hitPlayer = hit.collider.gameObject;
                        hitPlayer.GetComponent<TeamLobbyPlayer>().EnableKickUI(startTouchPos);
                    }
                }
                
                else
                {
                    if (hitPlayer != null)
                    {
                        hitPlayer.GetComponent<TeamLobbyPlayer>().DisableKickUI();
                    }
                }
            }
        }
        */
    }
}
