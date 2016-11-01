using UnityEngine;
using System.Collections;

public class Cat : MonoBehaviour {

    public int CatType = 0;
    public AudioClip MeowSound;
    public AudioClip ImpactSound;
    public AudioClip PurrSound;
    public Material[] CatSkins;
    public Material[] Faces;

    int numCatTypes;

    // --- behavior timers ---
    // boredom - cat begins to move around after maxBoredom
    float boredom = 0;
    float maxBoredom = 12;
    // anger
    float angryTimer = 0; // how long the cat will rage
    // lovey timer
    float loveyTimer = 0; // how long the cat will have heart eyes
    // timer for how long the cat has been upside-down
    float flipTimer = 0;
    float waitBeforeFlipDuration = 2;

    // prevent repetitive meows
    float meowTimer;
    float meowInterval = 0.5f; // at least half a second
    // meow pitch
    float pitch = 1f;

    // wander in a direction corresponding to an angle on the XZ plane
    float wanderAngle = 0;

    // rigidbody
    Rigidbody rbody;

    // renderer
    SkinnedMeshRenderer SMRenderer;
    // current materials
    Material[] currentMats;

    // all faces
    enum FACE { LOVEY, HAPPY, PISSED };

    // hopping cats
    float hopTimer = 0;

    // emotions
    enum EMOTIONS {  LOVEY, HAPPY, BORED, PISSED }

    // orientation
    enum ORIENTATION { SIDEWAYS_LEFT, UPSIDE_DOWN, SIDEWAYS_RIGHT, RIGHT_SIDE_UP, FACE_DOWN, ON_BACK }
    ORIENTATION orientation = ORIENTATION.RIGHT_SIDE_UP;

    // mood
    EMOTIONS mood = EMOTIONS.HAPPY;

    public float Boredom
    {
        set
        {
            AutoSetFace();
            boredom = value;
        }
    }

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
        var mesh = transform.FindChild("Mesh");
        SMRenderer = mesh.GetComponentInChildren<SkinnedMeshRenderer>();
        numCatTypes = CatSkins.Length;
        aSource = gameObject.AddComponent<AudioSource>();
        SetSkin();
        rbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        // TIMERS
        meowTimer += Time.fixedDeltaTime;
        boredom += Time.fixedDeltaTime;
        loveyTimer -= Time.fixedDeltaTime;
        angryTimer -= Time.fixedDeltaTime;

        // STATE MACHINE
        AutoSetMood(); // determine state machine state
        AutoSetFace(); // face correlates with mood

        // BEHAVIOR
        if (boredom > maxBoredom)
        {
            Wander();
        }
        // RIGHT SIDE UP
        CheckOrientation();
        if (flipTimer > waitBeforeFlipDuration)
        {
            flipTimer = 0;
            FlipUpwards();
        }
	}

    void CheckOrientation()
    {
        // only when not held
        if (GetComponent<FixedJoint>() != null) return;

        // use dot product
        float dot = 0;

        // check which vector is the closest to up
        dot = Vector3.Dot(transform.up, Vector3.up);
        if (dot > 0.5)
        {
            // no need to flip
            orientation = ORIENTATION.RIGHT_SIDE_UP;
            return;
        } else
        {
            // if the cat is not right side up, flip
            flipTimer += Time.fixedDeltaTime;
        }
        if (dot < -0.5)
        {
            orientation = ORIENTATION.UPSIDE_DOWN;
        }
        // left right
        dot = Vector3.Dot(transform.right, Vector3.up);
        if (dot > 0.5)
        {
            orientation = ORIENTATION.SIDEWAYS_RIGHT;
            return;
        }
        if (dot < -0.5)
        {
            orientation = ORIENTATION.SIDEWAYS_LEFT;
            return;
        }
        // back front
        dot = Vector3.Dot(transform.forward, Vector3.up);
        if (dot > 0.5)
        {
            orientation = ORIENTATION.ON_BACK;
            return;
        }
        if (dot < -0.5)
        {
            orientation = ORIENTATION.FACE_DOWN;
            return;
        }
    }

    public void SetSkin()
    {
        // pitch based on type
        pitch = 0.75f + 0.5f * (CatType - 1) / (float)numCatTypes;
        currentMats = new Material[] { CatSkins[CatType], Faces[1] };
        if (SMRenderer == null) {
            var mesh = transform.FindChild("Mesh");
            SMRenderer = mesh.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        SMRenderer.materials = currentMats;
    }

    void AutoSetMood()
    {
        // pissed?
        if (angryTimer > 0)
        {
            mood = EMOTIONS.PISSED;
            return;
        }
        // love?
        if (loveyTimer > 0)
        {
            mood = EMOTIONS.LOVEY;
            return;
        }
        // default
        mood = EMOTIONS.HAPPY;
    }

    void AutoSetFace()
    {
        if (mood == EMOTIONS.LOVEY)
        {
            SetFace(FACE.LOVEY);
        } else if (mood != EMOTIONS.PISSED) {
            SetFace(FACE.HAPPY);
        } else {
            SetFace(FACE.PISSED);
        }
    }

    void SetFace(FACE face)
    {
        currentMats[1] = Faces[(int)face];
        SMRenderer.materials = currentMats;
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
        loveyTimer = 1;
        angryTimer = 0;
    }

    public void ResetBoredom()
    {
        boredom = 0;
    }

    public void Pet()
    {
        Purr();
        // max 12
        if (boredom > 12) boredom = 12;
        boredom -= 4;
        // min 0
        if (boredom < 0) boredom = 0;
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
            Vector3 force = 0.5f * (dir + Vector3.up);
            rbody.AddForce(force);
        }
    }

    void FlipUpwards()
    {
        // how much force to rotate with
        float force = 1200f;
        // jump up
        //rbody.AddForce(new Vector3(0, 2f, 0));

        // max angular velocity
        rbody.maxAngularVelocity = 10000;

        print(orientation);

        // rotate
        switch (orientation)
        {
            case ORIENTATION.SIDEWAYS_LEFT:
                rbody.AddRelativeTorque(new Vector3(0, 0, force),ForceMode.Acceleration);
                break;
            case ORIENTATION.SIDEWAYS_RIGHT:
                rbody.AddRelativeTorque(new Vector3(0, 0, -force), ForceMode.Acceleration);
                break;
            case ORIENTATION.UPSIDE_DOWN:
                rbody.AddRelativeTorque(new Vector3(1.5f*force, 0, 0), ForceMode.Acceleration);
                break;
            case ORIENTATION.FACE_DOWN:
                rbody.AddRelativeTorque(new Vector3(-force, 0, 0), ForceMode.Acceleration);
                break;
            case ORIENTATION.ON_BACK:
                rbody.AddRelativeTorque(new Vector3(force, 0, 0), ForceMode.Acceleration);
                break;
        }
        
        
    }
}
