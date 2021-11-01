using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WildAbilityHandler : MonoBehaviour
{
    //UI
    [SerializeField] Text SkillDisplayText;
    [SerializeField] GameObject SkillDisplayUI;

    //Abilities
    public Ability wild_Ability;

    float cooldownTime;
    float activeTime;
    bool activate;
    string displayText;

    enum AbilityState { ready, active, cooldown }

    AbilityState state = AbilityState.ready;

    //Touch Phase
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 currentTouchPos;

    private bool stopTouch = false;

    private float swipeRange = 50;
    private float tapRange = 10;

    public GameObject hitPlayer;

    TeamData teamdata;
    InGameData inGameData;

    private void Start()
    {
        activate = false;
        state = AbilityState.ready;
        SkillDisplayText.text = "";
        SkillDisplayUI.SetActive(false);
    }
    void Update()
    {
        teamdata = API.instance.getTeamData();
        inGameData = API.instance.getInGameData();

        switch (state)
        {
            case AbilityState.ready:
                if (activate)
                {
                    activate = false;
                    wild_Ability.Activate();
                    state = AbilityState.active;
                    activeTime = wild_Ability.activeTime;
                }
                break;
            case AbilityState.active:
                if (activeTime > 0)
                {
                    SkillDisplayText.text = displayText + "\n" + System.Math.Round(activeTime, 2).ToString();
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    SkillDisplayText.text = "";
                    wild_Ability.BeginCooldown();
                    state = AbilityState.cooldown;
                    cooldownTime = wild_Ability.coolDownTime;
                }
                break;
            case AbilityState.cooldown:
                if (cooldownTime > 0)
                {
                    cooldownTime -= Time.deltaTime;
                }
                else
                {
                    state = AbilityState.ready;
                }
                break;
            default:
                break;
        }

        //Touch Phase

        if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Began))
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

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8))
                {
                    if (hit.collider != null && state == AbilityState.ready)
                    {
                        hitPlayer = hit.collider.gameObject;
                        wild_Ability = hitPlayer.GetComponent<WildAnimalController>().getWildAbility();
                        displayText = hitPlayer.GetComponent<WildAnimalController>().getDisplayText();
                        hitPlayer.GetComponent<WildAnimalController>().clicked();

                        float multiply_factor = hitPlayer.GetComponent<WildAnimalController>().get_multiplier();

                        API.instance.Ability_Multiplier(inGameData.gameID, teamdata.teamCode, multiply_factor);

                        StartCoroutine(display_skill_seconds("Velocity x " + multiply_factor.ToString(), 2));
                        //activate = true;
                    }
                }
            }
        }

        if (Input.touchCount > 1 && (Input.GetTouch(1).phase == TouchPhase.Began))
        {
            startTouchPos = Input.GetTouch(1).position;
        }

        if (Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            currentTouchPos = Input.GetTouch(1).position;
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

        if (Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Ended)
        {
            stopTouch = false;

            endTouchPos = Input.GetTouch(1).position;

            Vector2 Distance = endTouchPos - startTouchPos;

            if (Mathf.Abs(Distance.x) < tapRange && Mathf.Abs(Distance.y) < tapRange)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.touches[1].position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8))
                {
                    if (hit.collider != null && state == AbilityState.ready)
                    {
                        hitPlayer = hit.collider.gameObject;
                        wild_Ability = hitPlayer.GetComponent<WildAnimalController>().getWildAbility();
                        displayText = hitPlayer.GetComponent<WildAnimalController>().getDisplayText();
                        hitPlayer.GetComponent<WildAnimalController>().clicked();

                        float multiply_factor = hitPlayer.GetComponent<WildAnimalController>().get_multiplier();

                        API.instance.Ability_Multiplier(inGameData.gameID, teamdata.teamCode, multiply_factor);

                        StartCoroutine(display_skill_seconds("Velocity x " + multiply_factor.ToString(), 2));
                        //activate = true;
                    }
                }
            }
        }

    }

    IEnumerator display_skill_seconds(string str, float time)
    {
        SkillDisplayText.text = str;
        SkillDisplayUI.SetActive(true);
        yield return new WaitForSeconds(time);
        SkillDisplayUI.SetActive(false);
        SkillDisplayText.text = "";
    }
}
