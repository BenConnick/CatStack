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
    float pissCounter = 0;
    // lovey timer
    float loveyTimer = 0; // how long the cat will have heart eyes
    // timer for how long the cat has been upside-down
    float flipTimer = 0;
    float waitBeforeFlipDuration = 2;
    // how long between touches is still considered petting
    float pettingTimer = 0;
    float pettingInterval = 2f;
    // how long before the cat leaps at the toy
    float attackTimer = 0f;
    float minAttackWait = 1f;
    float maxAttackWait = 5f;
    // how long is a toy entertaining?
    float toyTimer = 0f;
    float toyAttentionThreshold = 5f;
    float maxToyTimer = 10f;

    // is a toy visible?
    bool watchingPrey = false;

    // prevent repetitive meows
    float meowTimer;
    float meowInterval = 0.5f; // at least half a second
    // meow pitch
    float pitch = 1f;

    // wander in a direction corresponding to an angle on the XZ plane
    float wanderAngle = 0;

    // the joint that attaches a cat to its prey
    FixedJoint claws;

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

    float turnSpeed = 5;

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
        pettingTimer -= Time.fixedDeltaTime;
        toyTimer -= Time.fixedDeltaTime;
        if (toyTimer < 0) toyTimer = 0;

        CalculateBoredom();

        // STATE MACHINE
        AutoSetMood(); // determine state machine state
        AutoSetFace(); // face correlates with mood

        // BEHAVIOR
        if (pissCounter > 0)
        {
            Rage();
        } else {
            if (boredom > maxBoredom)
            {
                Wander();
            }
            if (watchingPrey)
            {
                Stare();
                if (attackTimer < 0)
                {
                    Attack();
                } else
                {
                    attackTimer -= Time.fixedDeltaTime;
                }
            }
            else
            {
                LookAround();
            }
        }
        

        // RIGHT SIDE UP
        CheckOrientation();
        if (flipTimer > waitBeforeFlipDuration)
        {
            flipTimer = 0;
            FlipUpwards();
        }
	}

    // jump towards the target
    void Attack()
    {
        if (!watchingPrey || Manager.instance.ActiveToy == null) return;
        watchingPrey = false;

        Vector3 dir = (Manager.instance.ActiveToy.position - transform.position).normalized;
        Vector3 force = 2.5f * dir;
        rbody.AddForce(force);
    }

    // stare at a cat toy
    void Stare()
    {
        // if there is no toy, give up
        if (Manager.instance.ActiveToy == null)
            watchingPrey = false;
        // if not watching, then you are here by mistake
        if (!watchingPrey)
            return;

        // deplete interest in toy
        toyTimer += 2 * Time.fixedDeltaTime;
        // attention span depleted
        if (toyTimer > maxToyTimer)
        {
            toyTimer = maxToyTimer;
            // give up
            watchingPrey = false;
            boredom = maxBoredom;
        }

        //RotateTowardsTarget(Manager.instance.ActiveToy.position - transform.position);
        // rotate to face the direction of the "prey"
        rbody.AddTorque(new Vector3(turnSpeed * Time.fixedDeltaTime, 0, 0));
    }

    // checks to see if there is a cat toy visible
    void LookAround()
    {
        // no distractions
        if (Manager.instance.ActiveToy == null)
        {
            return;
        }

        // do not look for new toys until the attention replenishes
        if (toyTimer > toyAttentionThreshold)
        {
            return;
        }

        // can it see?
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Manager.instance.ActiveToy.position - transform.position);
        Physics.Raycast(ray, out hit);

        // hit?
        if (hit.transform != null)
        {
            if (Vector3.Dot(Manager.instance.ActiveToy.position - transform.position, transform.forward) > 0)
            {
                watchingPrey = true;
                attackTimer = Random.Range(minAttackWait, maxAttackWait);
            }
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

    void CalculateBoredom()
    {
        if (pettingTimer > 0 || watchingPrey)
        {
            // max 12
            if (boredom > 12) boredom = 12;
            boredom -= Time.fixedDeltaTime;
            // min 0
            if (boredom < 0) boredom = 0;
        }
    }

    void AutoSetMood()
    {
        // pissed?
        if (pissCounter > 0)
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

    public void LetGo()
    {
        claws.connectedBody = null;
        Destroy(claws);
        claws = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > 25)
        {
            pissCounter = 10;
        }
        // grabbed toy
        if (collision.transform == Manager.instance.ActiveToy)
        {
            // grab on to the toy
            claws = collision.gameObject.AddComponent<FixedJoint>();
            claws.connectedBody = rbody;
            StringToy stringToyHandle = FindObjectOfType<StringToy>();
            if (stringToyHandle != null)
            {
                stringToyHandle.BeginTugOfWar();
            }
        }
        if (collision.collider.name.Contains("Controller"))
        {
            //Meow();
        } else
        {
            // play impact sound
            //aSource.pitch = collision.relativeVelocity.magnitude
            if (!aSource.isPlaying) {
                aSource.pitch = 1f;
                aSource.clip = ImpactSound;
                aSource.Play();
            }
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
    }

    public void ResetBoredom()
    {
        boredom = 0;
    }

    public void Pet()
    {
        pissCounter -= 10;
        Purr();

        // reset the timer
        pettingTimer = pettingInterval;
    }

    void Wander()
    {
        float hopInterval = 0.5f;

        // randomly change the wander angle in radians
        wanderAngle += Random.Range(-0.1f, 0.1f);

        // rotate to face the direction of hop
        //RotateTowardsTarget(rbody.velocity);

        
        
        /*transform.forward = Vector3.RotateTowards(
            transform.forward, 
            Vector3.ProjectOnPlane(rbody.velocity,Vector3.up), 
            turnSpeed * Time.deltaTime, 
            turnSpeed * Time.deltaTime);*/

        // hop every so often
        hopTimer += Time.fixedDeltaTime;

        // move the cat
        if (hopTimer > hopInterval)
        {
            hopTimer -= hopInterval;
            Vector3 dir = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle));
            Vector3 force = 0.5f * (dir + Vector3.up);
            rbody.AddForce(force);
        }
    }

    void RotateTowardsTarget(Vector3 dir)
    {
        // get direction to rotate
        if (Vector3.Dot(transform.right, dir) > 0.1)
        {
            rbody.AddRelativeTorque(new Vector3(0, turnSpeed * Time.fixedDeltaTime, 0));
        }
        else if (Vector3.Dot(transform.right, dir) < -0.1)
        {
            rbody.AddRelativeTorque(new Vector3(0, -turnSpeed * Time.fixedDeltaTime, 0));
        }
    }

    void Rage()
    {
        if (orientation != ORIENTATION.RIGHT_SIDE_UP)
        {
            flipTimer += Time.deltaTime; // double speed
            return;
        }

        // rotate to face the direction of hop
        /*transform.forward = Vector3.RotateTowards(
            transform.forward,
            Vector3.ProjectOnPlane(rbody.velocity, Vector3.up),
            turnSpeed * Time.deltaTime,
            turnSpeed * Time.deltaTime);*/

        float hopInterval = 0.5f;

        // randomly change the wander angle in radians
        wanderAngle += Random.Range(-0.5f, 0.5f);

        // hop every so often
        hopTimer += Time.fixedDeltaTime;

        // move the cat
        if (hopTimer > hopInterval)
        {
            hopTimer -= hopInterval;
            Vector3 dir = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle));
            Vector3 force = 1.5f * (dir + Vector3.up);
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
