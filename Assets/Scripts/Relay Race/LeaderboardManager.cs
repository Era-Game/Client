using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("LeaderBoard Manager")]
    public Text FirstTeamCodeText;
    public Text SecondTeamCodeText;
    public Text ThirdTeamCodeText;
    public Text FirstTotalStepsText;
    public Text SecondTotalStepsText;
    public Text ThirdTotalStepsText;

    // Start is called before the first frame update
    void Start()
    {
        //FirebaseManager.instance.getLeaderBoardData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
