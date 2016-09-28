using UnityEngine;
using System.Collections;

public class Cat : MonoBehaviour {

    public int CatType = 0;
    public AudioClip MeowSound;
    public AudioClip ImpactSound;

    int numCatTypes = 10;

    // --- behavior traits ---
    // boredom - cat begins to move around after maxBoredom
    float boredom = 0;
    float maxBoredom = 10;
    // anger
    float angryTimer = 0; // how long the cat will rage
    // determine whether the player is attempting to pet the cat
    float pettingTimer = 0;

    // prevent repetitive meows
    float meowTimer;
    float meowInterval = 0.5f; // at least half a second
    // meow pitch
    float pitch = 1f;

    Rigidbody r;

    // id matches with person's index
    int id;

    public int ID
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }

    AudioSource aSource;

	// Use this for initialization
	void Start () {
        aSource = gameObject.AddComponent<AudioSource>();
        SetSkin(CatType);
        r = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        meowTimer += Time.fixedDeltaTime;
        if (r.velocity.sqrMagnitude > 100000000)
        {
            r.velocity = Vector3.zero;
        }
	}

    void SetSkin(int skinIndex)
    {
        // 1-indexed
        if (CatType == 0)
        {
            CatType = 1;
        }
        // pitch based on type
        pitch = 0.75f + 0.5f * (CatType - 1) / (float)numCatTypes;

        var mesh = transform.FindChild("Mesh").transform.GetChild(0);
        var renderer = mesh.GetComponentInChildren<SkinnedMeshRenderer>();
        renderer.material = Manager.instance.CatSkins[CatType];
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

    public void Pet()
    {

    }
}
