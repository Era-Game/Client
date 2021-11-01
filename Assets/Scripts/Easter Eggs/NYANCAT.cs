using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NYANCAT : MonoBehaviour
{

    private bool memeIT;
    private bool startPlaying;

    public Transform StartPOS;
    public Transform StopPOS;

    private Rigidbody2D rb;

    public float speed = 8f;
    private float startTime;
    private float journeyLength;

    private bool musicInit;

    AudioSource audioSource;
    [SerializeField] AudioClip NYAN_CAT_MUSIC;

    void Start()
    {
        memeIT = false;
        startPlaying = false;
        musicInit = true;
        int randIndex = UnityEngine.Random.Range(0, 200);

        audioSource = GetComponent<AudioSource>();
        rb = gameObject.GetComponent<Rigidbody2D>();

        Debug.Log("Your Number: " + randIndex);

        if (randIndex == 69)
        {
            memeIT = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (memeIT)
        {
            StartCoroutine(countDownCoroutine());
        }

        if (startPlaying)
        {

            /*float distCovered = (Time.time - startTime) * speed;
            float fracOfJourney = distCovered / journeyLength;

            transform.position = Vector3.Lerp(StartPOS.position, StopPOS.position, fracOfJourney);*/

            rb.velocity = new Vector2(30f, 0.0f);
        }

        if (startPlaying && musicInit)
        {
            play_NYAN_CAT_MUSIC();
            StartCoroutine(stop_NYAN_CAT_MUSIC());
        }
    }

    private IEnumerator countDownCoroutine()
    {
        yield return new WaitForSecondsRealtime(10.0f);
        startTime = Time.time;
        journeyLength = Vector3.Distance(StartPOS.position, StopPOS.position);
        yield return new WaitForSecondsRealtime(0.5f);
        startPlaying = true;
    }

    public void play_NYAN_CAT_MUSIC()
    {
        musicInit = false;
        audioSource.clip = NYAN_CAT_MUSIC;
        audioSource.Play();
    }

    private IEnumerator stop_NYAN_CAT_MUSIC()
    {
        yield return new WaitForSecondsRealtime(12.0f);
        audioSource.Stop();
        rb.velocity = new Vector2(0.0f, 0.0f);
    }
}
