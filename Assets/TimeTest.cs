using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTest : MonoBehaviour
{
    public float year, month, date, hour, minutes, seconds;
    public double startTime;
    public void Start()
    {
        show_time();
        startTime = System.DateTime.Now.Ticks;
    }
    public void show_time()
    {
        Debug.Log("Time.deltaTime: " + Time.deltaTime);
        Debug.Log("Time.time: " + Time.time);
        Debug.Log("System.DateTime.Now: " + System.DateTime.Now);
        Debug.Log("System.DateTime.Now.Ticks (convert to days): " + System.DateTime.Now.Ticks / (36000000000 * 24));

        year = System.DateTime.Now.Year;
        month = System.DateTime.Now.Month;
        date = System.DateTime.Now.Day;
        hour = System.DateTime.Now.Hour;
        minutes = System.DateTime.Now.Minute;
        seconds = System.DateTime.Now.Second;
        Debug.Log("It has been " + ((System.DateTime.Now.Ticks - startTime)/(36000000000 * 24)).ToString() + " days since the start of this program.");
    }
}
