using UnityEngine;
using System.Collections;

public class Cat : MonoBehaviour {

    public int CatType = 0;
    public AudioClip MeowSound;
    public AudioClip ImpactSound;
    public AudioClip PurrSound;

    int numCatTypes = 10;

    // --- behavior traits ---
    // boredom - cat begins to move around after maxBoredom
    float boredom = 0;
    float maxBoredom = 12;
    // anger
    float angryTimer = 0; // how long the cat will rage
    // determine whether the player is attempting to pet the cat
    float pettingTimer = 0;

    // prevent repetitive meows
    float meowTimer;
    float meowInterval = 0.5f; // at least half a second
    // meow pitch
    float pitch = 1f;

    // wander in a direction corresponding to an angle on the XZ plane
    float wanderAngle = 0;

    // hopping cats
    float hopTimer = 0;

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
        boredom += Time.fixedDeltaTime;
        // bored cats wander
        if (boredom > maxBoredom)
        {
            Wander();
        }
        // prevent glitchy rigidbody behavior
        if (r.velocity.sqrMagnitude > 1000)
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

    void OnTriggerEnter(Collider col)
    {
        if (col.name.Contains("Controller"))
        {
            Pet();
        }
        else
        {
            // do nothing yet
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

    public void Purr()
    {
        // if there has been enough time since last meow
        aSource.pitch = pitch;
        aSource.clip = PurrSound;
        aSource.Play();
    }

    public void ResetBoredom()
    {
        boredom = 0;
    }

    public void Pet()
    {
        Purr();
        if (boredom > 12)
        {
            boredom = 12;
        }
        boredom -= 4;
    }

    void Wander()
    {
        // randomly change the wander angle in radians
        wanderAngle += Random.Range(-0.1f, 0.1f);

        // hop every so often
        hopTimer += Time.fixedDeltaTime;

        // move the cat
        // yeah yeah the hoptimer limit is hardcoded, I know it sucks
        if (hopTimer > 0.5f)
        {
            hopTimer -= 0.5f;
            Vector3 dir = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle));
            Vector3 force = 50 * (dir + Vector3.up);
            r.AddForce(force);
        }
    }
}
