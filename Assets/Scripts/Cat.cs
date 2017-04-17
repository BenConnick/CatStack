using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cat : MonoBehaviour {

    #region inspector variables
    public int CatType = 0;
    public AudioClip MeowSound;
    public AudioClip ImpactSound;
    public AudioClip PurrSound;
    public AudioClip HissSound;
    public Material[] CatSkins;
    public Material[] Faces;
    public float rollForce;
    #endregion

    #region behavior timers
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
    float minAttackWait = 0.1f;
    float maxAttackWait = 2f;
    // how long is a toy entertaining?
    float toyTimer = 0f;
    float toyAttentionThreshold = 5f;
    float maxToyTimer = 1f;
    // prevent repetitive meows
    float meowTimer;
    float meowInterval = 0.5f; // at least half a second
    #endregion

    #region class variables
    // number of possible skins
    int numCatTypes;

    Vector3 wanderPrevPos = Vector3.zero;

    // cat flags { flipping }
    Dictionary<string, bool> flags;

    // is a toy visible?
    bool watchingPrey = false;

    // debug
    bool debugOn = true;
    
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
    enum FACE { LOVEY, HAPPY, PISSED, ASLEEP };

    // hopping cats
    float hopTimer = 0;

    // emotions
    enum EMOTIONS {  LOVEY, HAPPY, BORED, PISSED, ASLEEP, GRABBED }

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

    Transform modelRoot;

    bool belongsToPlayer = false;
    public bool BelongsToPlayer
    {
        get
        {
            return belongsToPlayer;
        }
        set
        {
            belongsToPlayer = value;
        }
    }

    #endregion

    // Use this for initialization
    void Start () {
        modelRoot = transform.Find("root");
        var mesh = transform.FindChild("Mesh");
        SMRenderer = mesh.GetComponentInChildren<SkinnedMeshRenderer>();
        numCatTypes = CatSkins.Length;
        aSource = gameObject.AddComponent<AudioSource>();
        SetSkin();
        rbody = GetComponent<Rigidbody>();
        flags = new Dictionary<string, bool>();
        flags.Add("flipping", false);
        flags.Add("activated", false);
    }

    public void Activate()
    {
        flags["activated"] = true;
    }

    public void Deactivate()
    {
        flags["activated"] = false;
    }

    // Update is called once per frame
    void FixedUpdate () {
        // only update if active
        if (!flags["activated"]) return;

        // DEBUG
        if (debugOn)
        {
            Color debugColor = Color.green;
            switch (mood)
            {
                case (EMOTIONS.ASLEEP):
                    debugColor = Color.blue;
                    break;
                case (EMOTIONS.BORED):
                    debugColor = Color.magenta;
                    break;
                case (EMOTIONS.LOVEY):
                    debugColor = Color.red;
                    break;
                case (EMOTIONS.PISSED):
                    debugColor = Color.yellow;
                    break;
            }
            Debug.DrawRay(transform.position, Vector3.up, debugColor);
        }

        // TIMERS
        UpdateTimers();
        LookForDistractions(); // interrupt boredom

        // STATE MACHINE
        AutoSetMood(); // determine state machine state
        AutoSetFace(); // face correlates with mood

        // BEHAVIOR BASED ON MOOD
        HandleBehaviors();

        // TRY TO BE RIGHT SIDE UP
        CheckOrientation();
	}

    void UpdateTimers()
    {
        meowTimer += Time.fixedDeltaTime;
        boredom += 5 * Time.fixedDeltaTime; if (boredom > 12) boredom = 12;
        loveyTimer -= Time.fixedDeltaTime;
        pettingTimer -= Time.fixedDeltaTime;
        toyTimer -= Time.fixedDeltaTime; if (toyTimer < 0) toyTimer = 0;
    }

    void HandleBehaviors()
    {
        // prey found?
        if (watchingPrey)
        {
            StalkPreyThenAttack();
        } else
        {
            // always watchful
            LookForToys();
        }

        // mood based behaviors
        switch (mood)
        {
            case EMOTIONS.LOVEY:
                boredom -= Time.deltaTime * 2;
                break;
            case EMOTIONS.PISSED:
                Rage();
                break;
            case EMOTIONS.HAPPY:
            case EMOTIONS.BORED:
                Wander();
                break;
        }
    }

    // handles timer for "attack"
    void StalkPreyThenAttack()
    {
        // stare
        Stare();
        attackTimer -= Time.fixedDeltaTime;
        // attack in intervals
        if (attackTimer < 0) { Attack(); }
    }

    // jump towards the current toy
    void Attack()
    {
        if (!watchingPrey || Manager.instance.ActiveToy == null) return;
        watchingPrey = false;

        Vector3 dir = (Manager.instance.ActiveToy.position - transform.position).normalized;
        Vector3 force = 2.5f * dir + Vector3.up * 0.5f;
        rbody.AddForce(force,ForceMode.VelocityChange);

        attackTimer = Random.Range(minAttackWait, maxAttackWait); ;
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
        //toyTimer += 2 * Time.fixedDeltaTime;

        // attention span depleted
        if (toyTimer > maxToyTimer)
        {
            toyTimer = maxToyTimer;
            // give up
            watchingPrey = false;
            boredom = maxBoredom;
        }

        // RotateTowardsTarget(Manager.instance.ActiveToy.position - transform.position);
        // rotate to face the direction of the "prey"
        if (rbody.velocity.sqrMagnitude == 0)
        {
            // to vec
            Vector3 toVec = Manager.instance.ActiveToy.position - transform.position;

            Debug.DrawRay(transform.position, toVec);

            Debug.DrawRay(transform.position, transform.forward, Color.black);

            float flip = -1;
            if (toVec.x > 0)
            {
                flip = 1;
            }

            float angle = flip * Vector3.Angle(Vector3.ProjectOnPlane(toVec,Vector3.up), Vector3.forward);

            //modelRoot.localRotation = Quaternion.Euler(0, -angle, -90);

            transform.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }

    // checks to see if there is a cat toy visible
    void LookForToys()
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
            if (Vector3.Dot((Manager.instance.ActiveToy.position - transform.position).normalized, transform.forward) > 0.5f)
            {
                watchingPrey = true;
                attackTimer = Random.Range(minAttackWait, maxAttackWait);
            }
        }
    }

    void CheckOrientation()
    {
        // only when NOT held or holding
        if (GetComponent<FixedJoint>() != null ||
            claws != null) return;

        // only when NOT MOVING
        if (rbody.velocity.sqrMagnitude > 0.1f &&
            mood != EMOTIONS.ASLEEP && mood != EMOTIONS.BORED)
        {
            return;
        }

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
            flipTimer += 2f*Time.fixedDeltaTime;
            // flip
            if (flipTimer > waitBeforeFlipDuration)
            {
                flipTimer = 0;
                FlipUpwards();
            }
        }
        if (dot < -0.5)
        {
            orientation = ORIENTATION.UPSIDE_DOWN;
            return;
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

    void LookForDistractions()
    {
        if (pettingTimer > 0 || watchingPrey)
        {
            boredom = 0;
        }
    }

    void AutoSetMood()
    {
        // pissed?
        if (pissCounter > 1)
        {
            if (mood != EMOTIONS.PISSED)
            {
                Meow();
            }
            mood = EMOTIONS.PISSED;
            return;
        }
        // asleep?
        if (mood == EMOTIONS.ASLEEP)
        {
            // keep sleeping until interrupted
            return;
        }
        // love?
        if (loveyTimer > 0)
        {
            mood = EMOTIONS.LOVEY;
            return;
        }
        // bored
        /*if (boredom >= maxBoredom)
        {
            mood = EMOTIONS.BORED;
            return;
        }*/
        // default
        mood = EMOTIONS.BORED;
    }

    void AutoSetFace()
    {
        switch(mood)
        {
            case EMOTIONS.LOVEY:
                SetFace(FACE.LOVEY);
                break;
            case EMOTIONS.PISSED:
                SetFace(FACE.PISSED);
                break;
            case EMOTIONS.ASLEEP:
                SetFace(FACE.ASLEEP);
                break;
            case EMOTIONS.BORED:
            default:
                SetFace(FACE.HAPPY);
                break;
        }
    }

    void SetFace(FACE face)
    {
        currentMats[1] = Faces[(int)face];
        SMRenderer.materials = currentMats;
    }

    void Grab(Collision collision)
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

    public void LetGo()
    {
        claws.connectedBody = null;
        Destroy(claws);
        claws = null;
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
        if (aSource.clip != PurrSound || !aSource.isPlaying)
        {
            aSource.pitch = pitch;
            aSource.clip = PurrSound;
            aSource.Play();
            loveyTimer = 1;
        }
    }

    public void ResetBoredom()
    {
        boredom = 0;
    }

    public void Pet()
    {
        pissCounter -= 10;
        Purr();
        Bounce();

        // reset the timer
        pettingTimer = pettingInterval;
    }

    void Bounce()
    {
        GetComponent<Jiggle>().Begin();
    }

    void Wander()
    {
        float hopInterval = 0.2f;

        // hop every so often
        hopTimer += Time.fixedDeltaTime;

        // move the cat
        if (hopTimer > hopInterval && rbody.velocity.sqrMagnitude == 0)
        {
            // reset interval
            hopTimer -= hopInterval;

            // turn if stuck
            if ((transform.position - wanderPrevPos).sqrMagnitude < 0.001f)
            {
                transform.Rotate(new Vector3(0, Random.value > 1 ? 90 : -90, 0));
            }

            // record position
            wanderPrevPos = transform.position;

            // hop
            rbody.AddForce((Vector3.ProjectOnPlane(transform.forward,Vector3.up) + Vector3.up) * 1.1f,ForceMode.VelocityChange);
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
        // bounce when you hit the walls / floor anything
    }

    void RageHop()
    {
        Vector3 dir = Random.insideUnitSphere.normalized;
        Vector3 force = dir + Vector3.up * 5 * Mathf.Clamp(1 - transform.position.y,0,1);
        rbody.AddForce(force,ForceMode.VelocityChange);
    }

    void FlipUpwards()
    {
        // FOR CRYING OUT LOUD STOP FLIPPING IN MIDAIR
        if (rbody.velocity.sqrMagnitude > 0.1) return;

        // jump up
        rbody.AddForce(new Vector3(0, 2f, 0),ForceMode.VelocityChange);

        // max angular velocity
        rbody.maxAngularVelocity = 10000;

        // rotate
        //Roll(orientation);
        StartCoroutine(AlignWithFloor(orientation));
    }

    IEnumerator AlignWithFloor(ORIENTATION currentFace)
    {
        flags["flipping"] = true;

        yield return new WaitForSeconds(0.15f);
        rbody.AddRelativeTorque(+0.004f*GetRollForceVector(currentFace),ForceMode.VelocityChange);

        yield return new WaitForSeconds(0.15f);
        rbody.AddRelativeTorque(-0.004f * GetRollForceVector(currentFace), ForceMode.VelocityChange);

        flags["flipping"] = false;
    }

    void Roll(ORIENTATION face)
    {
        switch (face)
        {
            case ORIENTATION.SIDEWAYS_LEFT:
                rbody.AddRelativeTorque(new Vector3(0, 0, rollForce), ForceMode.Acceleration);
                break;
            case ORIENTATION.SIDEWAYS_RIGHT:
                rbody.AddRelativeTorque(new Vector3(0, 0, -rollForce), ForceMode.Acceleration);
                break;
            case ORIENTATION.UPSIDE_DOWN:
                rbody.AddRelativeTorque(new Vector3(2.0f * rollForce, 0, 0), ForceMode.Acceleration);
                break;
            case ORIENTATION.FACE_DOWN:
                rbody.AddRelativeTorque(new Vector3(-rollForce, 0, 0), ForceMode.Acceleration);
                break;
            case ORIENTATION.ON_BACK:
                rbody.AddRelativeTorque(new Vector3(rollForce, 0, 0), ForceMode.Acceleration);
                break;
        }
    }

    Vector3 GetRollForceVector(ORIENTATION facing)
    {
        switch (facing)
        {
            case ORIENTATION.SIDEWAYS_LEFT:
                return new Vector3(0, 0, rollForce);
            case ORIENTATION.SIDEWAYS_RIGHT:
                return new Vector3(0, 0, -rollForce);
            case ORIENTATION.UPSIDE_DOWN:
                return new Vector3(1.5f * rollForce, 0, 0);
            case ORIENTATION.FACE_DOWN:
                return new Vector3(-rollForce, 0, 0);
            case ORIENTATION.ON_BACK:
                return new Vector3(rollForce, 0, 0);
        }
        return Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        // hit another cat
        if (collision.gameObject.GetComponent<Cat>())
        {
            // hit another cat quickly
            if (collision.relativeVelocity.sqrMagnitude > 10)
            {
                pissCounter = 10;
                RageHop();
            }
            // already angry cat, auto angry
            else if (collision.gameObject.GetComponent<Cat>().pissCounter >= 10)
            {
                pissCounter = 10;
                RageHop();
            }
            // hit a sleepy cat
            else if (collision.gameObject.GetComponent<Cat>().mood == EMOTIONS.ASLEEP)
            {
                boredom = 0;
                mood = EMOTIONS.ASLEEP;
            }
        }
        else
        {
            // wander if not on sleepy thing
            if (collision.collider.tag == "Soft")
            {
                boredom = 0;
                mood = EMOTIONS.ASLEEP;
            }
            else
            {
                if (mood == EMOTIONS.PISSED)
                {
                    RageHop();
                }
                boredom = 12;
                mood = EMOTIONS.BORED;
            }
        }

        // angry if hit too hard
        if (collision.relativeVelocity.sqrMagnitude > 15)
        {
            pissCounter = 10;
        }

        // grabbed toy
        if (collision.transform == Manager.instance.ActiveToy)
        {
            Grab(collision);
        }
        // old petting
        if (collision.collider.name.Contains("Controller"))
        {
            //Meow();
        }
        else
        {
            // play impact sound
            // reuse meow interval for stopping repetitive hit sound
            if (!aSource.isPlaying && meowTimer > meowInterval/2)
            {
                aSource.pitch = 1f;
                aSource.clip = ImpactSound;
                aSource.Play();
                meowTimer = 0; // reuse meow timer 
            }
        }
    }

    public void Grabbed()
    {
        Deactivate();
    }

    public void Released()
    {
        Activate();
    }

    void OnTriggerEnter(Collider col)
    {
        // touched by player (not held)
        if (col.name.Contains("Controller") && GetComponent<FixedJoint>() == null)
        {
            // pet
            if (pettingTimer > 0)
                Pet();
            pettingTimer = pettingInterval;
        }
        if (col.name.Contains("Bed"))
        {
            Meow();
            pissCounter = 0;
        }

        if (belongsToPlayer && col.name.Contains("CatBounds"))
        {
            print("cat lost! " + id);
            // move back to center
            transform.position = new Vector3(0, 2, 0);
            //Manager.instance.personManager.CatLost(id);
        }
    }
}
