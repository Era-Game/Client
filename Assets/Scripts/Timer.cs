using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float secondsUp;
    private int minutesUp;
    private int hoursUp;

    private float secondsDown;
    private int minutesDown;
    private int hoursDown;

    private bool isCountUp;
    private bool isCountDown;
    private bool isTimesUp;

    // Start is called before the first frame update
    void Start()
    {
        secondsUp = 0f;
        minutesUp = 0;
        hoursUp = 0;
        isCountUp = false;

        secondsDown = 0f;
        minutesDown = 0;
        hoursDown = 0;
        isCountDown = false;
        isTimesUp = false;
    }

    // Update is called once per frame
    void Update()
    {
        // CountUp timer

        if(isCountUp)
        {
            secondsUp += Time.deltaTime;
            //Debug.Log(secondsUp);
            if ((int)secondsUp >= 60)
            {
                minutesUp = minutesUp + (int)secondsUp / 60;
                secondsUp %= 60;
            }
            else if (minutesUp >= 60)
            {
                hoursUp = hoursUp + minutesUp / 60;
                minutesUp %= 60;
            }
        }

        // CountDown timer

        if (isCountDown && !isTimesUp)
        {
            secondsDown -= Time.deltaTime;
            if (secondsDown < 0 && minutesDown == 0 && hoursDown == 0)
            {
                isTimesUp = true;
            }
            else if (secondsDown < 0)
            {
                if (minutesDown <= 0)
                {
                    hoursDown -= 1;
                    minutesDown += 59;
                }
                else
                {
                    minutesDown -= 1;
                }
                secondsDown += 60;
            }
        }
    }

    //Count up functions ----------------------------------------------------------------------------

    public void startCountUpTimer()
    {
        isCountUp = true;
    }

    public void stopCountUpTimer()
    {
        isCountUp = false;
    }

    public void resetCountUpTimer()
    {
        secondsUp = 0;
        minutesUp = 0;
        hoursUp = 0;
    }

    public int getCountUpHours()
    {
        return hoursUp;
    }

    public int getCountUpMinutes()
    {
        return minutesUp;
    }

    public int getCountUpSeconds()
    {
        return (int)secondsUp;
    }

    //Count Down functions ----------------------------------------------------------------------------

    public void setCountDownTimer(int hours, int minutes, int seconds)
    {
        isTimesUp = false;
        hoursDown = hours;
        minutesDown = minutes;
        secondsDown = seconds;
    }

    public void startCountDownTimer()
    {
        isCountDown = true;
    }

    public void stopCountDownTimer()
    {
        isCountDown = false;
    }

    public void resetCountDownTimer()
    {
        isTimesUp = false;
        secondsDown = 0;
        minutesDown = 0;
        hoursDown = 0;
    }

    public int getCountDownHours()
    {
        return hoursDown;
    }

    public int getCountDownMinutes()
    {
        return minutesDown;
    }

    public int getCountDownSeconds()
    {
        return (int)secondsDown;
    }

    public bool isTimeUp()
    {
        return isTimesUp;
    }
}
