using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastObject : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] string objectType;
    private int clicks = 0;
    
    public void addClicks()
    {
        clicks += 1;
    }

    public int getClicks()
    {
        return clicks;
    }

    public string getObjecType()
    {
        return objectType;
    }
}
