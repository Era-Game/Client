using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Controllers
{

    public class TapButtonController : MonoBehaviour
    {
        [SerializeField] string Button_Name = "none";

        Animator anim;
        bool activated;
        // Start is called before the first frame update
        void Start()
        {
            activated = false;
            anim = gameObject.GetComponent<Animator>();
            anim.SetTrigger("game_start");
        }

        // Update is called once per frame
        void Update()
        {
            if (Button_Name == "Left Tap Button")
            {
                activated = RelayGameManager.instance.getLeftTapButtonStatus();
            }
            else if (Button_Name == "Right Tap Button")
            {
                activated = RelayGameManager.instance.getRightTapButtonStatus();
            }

            if (RelayGameManager.instance.getGameStart() && activated)
            {
                anim.SetInteger("state", 1);
            }
            else if (RelayGameManager.instance.getGameStart() && !activated)
            {
                anim.SetInteger("state", 0);
            }

        }
    }
}
