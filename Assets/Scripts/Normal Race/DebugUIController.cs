using System.Collections;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugUIController : MonoBehaviour
{
    public static DebugUIController instance;

    [Header("Firebase")]
    public Text warningCountThreshold;
    public Text warningThreshold;
    public Text refreshRate;
    public Text walkingThreshold;
    public InputField steps;

    public GameObject Debug_UI;

    public bool UI_Active = false;
    public float warning_count_threshold = 13f;
    public float warning_threshold = 14f;
    public float refresh_rate = 1.2f;
    public float walking_threshold = 0.2f;
    public float steps_ = 0.1f;

    private void Awake()
    {
        UI_Active = false;
        instance = this;
        warning_count_threshold = 13f;
        warning_threshold = 14f;
        refresh_rate = 1.2f;
        steps_ = 0.1f;
        walking_threshold = 0.2f;
    }

    public void wct_add()
    {
        warning_count_threshold += steps_;
    }

    public void wct_subtract()
    {
        warning_count_threshold -= steps_; 
    }

    public void wt_add()
    {
        warning_threshold += steps_;
    }

    public void wt_subtract()
    {
        warning_threshold -= steps_;
    }

    public void rr_add()
    {
        refresh_rate += steps_;
    }

    public void rr_subtract()
    {
        refresh_rate -= steps_;
    }

    public void walking_threshold_add()
    {
        walking_threshold += steps_;
    }

    public void walking_threshold_subtract()
    {
        walking_threshold -= steps_;
    }

    private void Update()
    {
        if (UI_Active)
        {
            warningCountThreshold.text = warning_count_threshold.ToString();
            warningThreshold.text = warning_threshold.ToString();
            refreshRate.text = refresh_rate.ToString();
            walkingThreshold.text = walking_threshold.ToString();
        }
        
    }

    public void updateSteps()
    {
        steps_ = float.Parse(steps.text);
    }

    public void start_Debug_Button()
    {
        UI_Active = true;
        Debug_UI.SetActive(true);
    }
    public void closeButton()
    {
        UI_Active = false;
        Debug_UI.SetActive(false);
    }
}
