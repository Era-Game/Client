using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaycastTest : MonoBehaviour
{
    [SerializeField] Text raycastText;
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    Color newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
                    hit.collider.GetComponent<MeshRenderer>().material.color = newColor;
                    GameObject hitGO = hit.collider.gameObject;
                    hitGO.GetComponent<RaycastObject>().addClicks();
                    int clicks = hitGO.GetComponent<RaycastObject>().getClicks();
                    string gameObjectType = hitGO.GetComponent<RaycastObject>().getObjecType();
                    raycastText.text = "Object Type: " + gameObjectType + "\nAmount Clicked: " + clicks;
                }
            }
        }
    }
}
