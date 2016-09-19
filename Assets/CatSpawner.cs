using UnityEngine;
using System.Collections;

public class CatSpawner : MonoBehaviour {

    public GameObject CatPrefab; // set in inspector
    public float SpawnInterval = 1f; // set in inspector
    float timer = 0f;
    GameObject currentCat;
    public float SpawnAnimationDuration = 1f; // set in inspector
    public TextMesh display;
    float spawnProgress = 0f;
    public Transform spawnLocation;
    bool on;

    // begin spawning cats
    public void Activate()
    {
        on = true;
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
            AnimateCatSpawn();
        }
	}

    void HandleTimer()
    {
        timer += Time.deltaTime;
        if (timer > SpawnInterval)
        {
            timer -= SpawnInterval;
            SpawnCat();
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        string text = "";
        
        text += (SpawnInterval - timer).ToString("00.00");
        display.text = text;
    }

    public void SpawnCat()
    {
        currentCat = (GameObject)GameObject.Instantiate(CatPrefab);
        currentCat.GetComponent<Collider>().enabled = false;
        currentCat.GetComponent<Cat>().CatType = Mathf.FloorToInt(Random.Range(1, 11));
    }

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
