using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MusicManager : MonoBehaviour
    {

        public static MusicManager instance;
        AudioSource audioSource;

        [SerializeField] AudioClip lobbyMusic;
        [SerializeField] AudioClip audioStartRun;
        [SerializeField] AudioClip audioRun;

        enum MusicSelect { forestMusic, otherMusic }

        MusicSelect musicState = MusicSelect.forestMusic;

        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {


            if (SceneManager.GetActiveScene().name == "Relay Game" && musicState == MusicSelect.forestMusic && RelayGameManager.instance.isStartCountDown)
            {
                musicState = MusicSelect.otherMusic;
                StartCoroutine(PlayGamePlayMusic());
            }
            else if (SceneManager.GetActiveScene().name == "Relay Game" && musicState == MusicSelect.forestMusic && RelayGameManager.instance.isStartGame)
            {
                musicState = MusicSelect.otherMusic;
                PlayRunningMusic();
            }

            else if (SceneManager.GetActiveScene().name != "Relay Game" && musicState == MusicSelect.otherMusic)
            {
                musicState = MusicSelect.forestMusic;
                audioSource.clip = lobbyMusic;
                audioSource.Play();
                audioSource.loop = true;
            }
        }

        IEnumerator PlayGamePlayMusic()
        {
            audioSource.clip = audioStartRun;
            audioSource.Play();
            audioSource.loop = false;
            yield return new WaitForSeconds(audioSource.clip.length);

            PlayRunningMusic();
        }

        private void PlayRunningMusic()
        {
            audioSource.clip = audioRun;
            audioSource.Play();
            audioSource.loop = true;
        }

    }
}

