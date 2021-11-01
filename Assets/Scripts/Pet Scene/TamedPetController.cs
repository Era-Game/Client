using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TamedPetController : MonoBehaviour
{

    [Header("Scene Objects")]
    public TMP_Text petNameText;
    public GameObject nameTag;

    private Animator anim; 
    private string petName;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        nameTag.SetActive(false); 
    }

    private void Update()
    {
        nameTag.transform.rotation = Quaternion.Euler(nameTag.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y - 180, nameTag.transform.localEulerAngles.z);
    }

    public void play_idle_animation()
    {
        anim.SetInteger("state", 0);
    }
    public void play_walking_animation()
    {
        anim.SetInteger("state", 1 );
    }

    public void play_eat_animation()
    {
        anim.SetInteger("state", 2);
    }


    public void displayName(string _name)
    {
        petNameText.text = _name;
        nameTag.SetActive(true);
    }
}
