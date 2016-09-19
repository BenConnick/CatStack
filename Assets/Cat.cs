using UnityEngine;
using System.Collections;

public class Cat : MonoBehaviour {

    public int CatType = 0;
    public AudioClip MeowSound;
    public AudioClip ImpactSound;

    int numCatTypes = 10;

    // prevent repetitive meows
    float meowTimer;
    float meowInterval = 0.5f; // at least half a second
    float pitch = 1f;

    AudioSource aSource;

	// Use this for initialization
	void Start () {
        aSource = gameObject.AddComponent<AudioSource>();
        // 1-indexed
        if (CatType == 0)
        {
            CatType = 1;
        }
        // pitch based on type
        pitch = 0.75f + 0.5f * (CatType - 1) / (float)numCatTypes;

        var mesh = transform.FindChild("Mesh").transform.GetChild(0);
        var renderer = mesh.GetComponent<SkinnedMeshRenderer>();
        print(renderer.material);
        renderer.material = Manager.instance.CatSkins[CatType];
	}
	
	// Update is called once per frame
	void Update () {
        meowTimer += Time.deltaTime;
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.Contains("Controller"))
        {
            Meow();
        } else
        {
            // play impact sound
            //aSource.pitch = collision.relativeVelocity.magnitude;
            aSource.pitch = 1f;
            aSource.clip = ImpactSound;
            aSource.Play();
        }
    }

    public void Meow()
    {
        // if there has been enough time since last meow
        if (meowTimer > meowInterval)
        {
            aSource.pitch = pitch;
            aSource.clip = MeowSound;
            meowTimer = 0;
            aSource.Play();
        }
    }
}
