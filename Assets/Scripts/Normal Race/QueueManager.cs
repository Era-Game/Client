using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Managers
{

    public class QueueManager : MonoBehaviour
    {

        private int queueSize;
        private string player_uid;
        private bool isPressed = true;

        [Header("Queue Utilities")]
        public TMP_Text queueNumberText;

        bool finishUpdate;

        public void quit_Button()
        {
            if (isPressed)
            {
                isPressed = false;
                StartCoroutine(JumpToLobby());
            }
        }

        IEnumerator JumpToLobby()
        {
            FirebaseManager.instance.cancelNormalRaceQueue(player_uid);
            yield return new WaitForSeconds(0.5f);
            PlayerManager.instance.setGameStatus("placeholder", "placeholder");
            SceneManager.LoadScene("Lobby");
        }

        // Start is called before the first frame update
        void Start()
        {
            queueSize = 1;
            finishUpdate = true;
            //Debug.Log("Player Stats Reset when Queue");
            player_uid = PlayerManager.instance.getData("uid");
        }

        // Update is called once per frame
        void Update()
        {
            if (finishUpdate)
            {
                StartCoroutine(update_Queue());
            }

        }

        private IEnumerator update_Queue()
        {
            finishUpdate = false;

            int localQueueCount = 1;

            FirebaseManager.instance.UpdateNormalQueue();
            yield return new WaitForSeconds(0.5f);

            queueNumberText.text = queueSize + "/3";

            localQueueCount = FirebaseManager.instance.getNormalQueueCount();
            if (localQueueCount >= 3)
            {
                localQueueCount = 3;
            }

            queueSize = localQueueCount;
            //Debug.Log("Queue Size: " + queueSize);

            StartCoroutine(getInNormalGame());
        }

        private IEnumerator getInNormalGame()
        {
            yield return new WaitForSeconds(0.5f);
            FirebaseManager.instance.getNormalQueueData(player_uid);
            yield return new WaitForSeconds(0.5f);
            if (FirebaseManager.instance.getEnterNormalGame())
            {
                Debug.Log("Join New Normal Race Game");
                //quit = true;
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Game Play");
            }

            finishUpdate = true;
        }
    }
}
