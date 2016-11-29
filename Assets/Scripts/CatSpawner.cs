using UnityEngine;
using System.Collections;

public class CatSpawner : MonoBehaviour {

    public GameObject CatPrefab; // set in inspector
    public float SpawnInterval = 1f; // set in inspector
    float timer = 0f;
    GameObject currentCat;
    public float SpawnAnimationDuration = 1f; // set in inspector
    float spawnProgress = 0f;
    public Transform spawnLocation;
    public Vector3 spawnOffset;
    public Vector3 LaunchVelocity;
    bool on;

    // begin spawning cats
    public void Activate()
    {
        on = true;
        timer = SpawnInterval;
        Manager.instance.PlayGame();
    }
    public void Deactivate()
    {
        on = false;
    }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (on)
        {
            HandleTimer();
        }
	}

    void HandleTimer()
    {
        timer += Time.deltaTime;
        if (SpawnInterval > 0 && timer > SpawnInterval)
        {
            timer -= SpawnInterval;
            SpawnCat();
        }
    }

    public Cat SpawnCat()
    {
        return SpawnCat(-1);
    }
    public Cat SpawnCat(int idx)
    {
        currentCat = (GameObject)GameObject.Instantiate(CatPrefab);
        currentCat.transform.forward = spawnLocation.right;
        currentCat.GetComponent<Collider>().enabled = true;
        currentCat.GetComponent<Cat>().CatType = 
            Mathf.FloorToInt(Random.Range(0, currentCat.GetComponent<Cat>().CatSkins.Length));
        currentCat.GetComponent<Cat>().SetSkin();
        currentCat.GetComponent<Cat>().ID = idx;
        currentCat.transform.position = spawnLocation.position + spawnOffset;
        return currentCat.GetComponent<Cat>();
    }

    // launch the cat
    public void LaunchCat(Cat cat, Vector3 velocity)
    {
        LaunchVelocity = velocity;
        LaunchCat(cat);
    }

    // launch the cat
    public void LaunchCat(Cat cat)
    {
        Rigidbody r = cat.GetComponent<Rigidbody>();
        r.AddForce(LaunchVelocity);
    }

    // show the cat slowly emerging from the tube
    void AnimateCatSpawn()
    {
        // exit if no cat
        if (currentCat == null) {
            spawnProgress = 0;
            return;
        }

        // done?
        if (spawnProgress >= SpawnAnimationDuration)
        {
            currentCat.GetComponent<Collider>().enabled = true;
            currentCat.GetComponent<Rigidbody>().velocity = Vector3.zero;
            currentCat.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            currentCat.GetComponent<Cat>().Meow();
            currentCat = null;
            // add a point
            Manager.instance.AddPoints(1);
            return;
        }

        // progress
        spawnProgress += Time.deltaTime;

        // lock rotation
        currentCat.transform.rotation = Quaternion.identity;
        // LERP position
        currentCat.transform.position = Vector3.Lerp(spawnLocation.position,
            spawnLocation.position - transform.up, 
            spawnProgress / SpawnAnimationDuration);
    }
}
