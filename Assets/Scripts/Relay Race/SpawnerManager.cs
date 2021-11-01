using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{

    [Header("Spawnables")]
    [SerializeField] GameObject[] animals;

    [Header("Spawnin Setting")]
    [SerializeField] float min_spawn_delay = 0f;
    [SerializeField] float spawn_delay_range = 8f;
    [SerializeField] float min_animal_lifetime = 30f;
    [SerializeField] float animal_lifetime_range = 100f;


    //Private Member Variables
    private float[] spawnChances = { };
    private GameObject clone;
    private int timer;
    private bool spawnerReady = true;
    float spawnTime;
    float lifeTime;

    //Private Contants

    private void Start()
    {
        spawnerReady = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnerReady)
        {
            StartCoroutine(spawn_coroutine());
        }

    }

    private void spawn()
    {
        int randNum = Random.Range(0, animals.Length - 1);
        clone = Instantiate(animals[randNum], transform.position, transform.rotation);
        clone.transform.parent = transform;

    }

    public void despawn()
    {
     
        Destroy(clone);
        spawnerReady = true;
    }
   
    private IEnumerator spawn_coroutine()
    {
        spawnerReady = false;
        yield return new WaitForEndOfFrame();

        spawnTime = Random.Range(min_spawn_delay, min_spawn_delay + spawn_delay_range);
        lifeTime = Random.Range(min_animal_lifetime, min_animal_lifetime + animal_lifetime_range);

        yield return new WaitForSecondsRealtime(spawnTime);
        spawn();
        yield return new WaitForSecondsRealtime(lifeTime);
        clone.GetComponent<WildAnimalController>().kill();
    }
}
