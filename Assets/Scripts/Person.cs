using UnityEngine;
using System.Collections;

public class Person : MonoBehaviour {

    #region inspector variables

    public int index;

    public float WalkSpeed;

    public AudioClip[] sounds; // set in inspector
    // 0: correct, 1: incorrect, 2: throw cat

    public ParticleSystem money;

    public ParticleSystem X_Sign;

    public SkinnedMeshRenderer catThought;

    #endregion

    #region class variables

    float maxTossSpeed = 3f;

    CatSpawner spawnerComponent;

    enum PersonState { WALKING_TO_DOOR, WAITING_FOR_DOOR, IDLE, DELAYING, WALKING_AWAY, STANDING_IN_LINE }

    PersonState state;

    Vector3 doormatLocation;

    public Vector3 DoormatLoc
    {
        get
        {
            return doormatLocation;
        }
    }

    float targetDistanceTolerance = 1.11f;

    Cat heldCat;

    FixedJoint joint;

    GameObject thoughtBubble;

    Door door;

    AudioSource a;

    bool soundLocked;

    // reusable timer
    float timer;

    bool giveFeedback = false;

    // reusable timer limit
    float delay;

    // set when they drop off the cat
    bool returning = false;

    public bool Returning
    {
        get
        {
            return returning;
        }
    }

    #endregion

    // Use this for initialization
    void Start() {

        a = gameObject.AddComponent<AudioSource>();

        // get component reference
        spawnerComponent = GetComponent<CatSpawner>();

        // get thought bubble reference
        thoughtBubble = transform.FindChild("ThoughtBubble").gameObject;
        // start hidden
        HideThoughtBubble();

        // set door
        door = GameObject.Find("Door").GetComponent<Door>();

        // calculate doormat location
        doormatLocation = door.transform.position - Vector3.right + Vector3.forward * 0.5f;
        //Debug.DrawLine(doormatLocation, Vector3.zero, Color.red, 10);

        // start with a cat
        GetCat();

        // immediately go to the door
        GoToDoor();

        // let the manager know that this person is next in line
        Manager.instance.personManager.Queue(this);
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (state) {
            case PersonState.WALKING_TO_DOOR:
                // returns true if the destination was reached
                if (WalkingToLocation(Manager.instance.personManager.GetQueuedPosition(index)))
                {
                    // if the position is the front of the line
                    if (CheckDistanceToLocation(doormatLocation))
                    {
                        RingDoorbell();
                    }
                }
                break;
            case PersonState.WAITING_FOR_DOOR:
                // when the player opens the door
                if (door.Open)
                {
                    // drop off cat
                    if (!returning)
                    {
                        // throw the cat into the room
                        TossCat();
                        // and leave after 2 seconds
                        Leave(2);
                    }
                    // pick up cat
                    else
                    {
                        // wait until the player actually gives you the cat
                        giveFeedback = true;
                    }
                }
                break;
            case PersonState.DELAYING:
                timer += Time.fixedDeltaTime;
                if (timer >= delay)
                {
                    Leave();
                }
                break;
            case PersonState.WALKING_AWAY:
                WalkingToLocation(doormatLocation - 20 * Vector3.forward);
                break;
        }
    }

    bool WalkingToLocation(Vector3 location)
    {
        // get the vector to the location
        Vector3 toVec = location - transform.position;

        //Debug.DrawLine(transform.position, location, Color.blue, 10);

        // if distance is less than tolerance return
        if (CheckDistanceToLocation(location))
        {
            // arrived at destination
            return true;
        }

        // move towards the location in he XZ plane using speed
        transform.position += Vector3.ProjectOnPlane(toVec.normalized,Vector3.up) * Time.deltaTime * WalkSpeed;

        // moving to destination
        return false;
    }

    bool CheckDistanceToLocation(Vector3 location)
    {
        // get the vector to the location
        Vector3 toVec = location - transform.position;

        // if distance is less than tolerance return
        if (toVec.magnitude < targetDistanceTolerance)
        {
            return true;
        }
        return false;
    }

    public void GetCat()
    {
        heldCat = spawnerComponent.SpawnCat(index);
        joint = heldCat.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();

        // set the skin of the cat thought to match the actual cat
        catThought.materials = new Material[] { heldCat.GetComponent<Cat>().CatSkins[heldCat.CatType], heldCat.GetComponent<Cat>().Faces[1] };
    }

    // go to door
    public void GoToDoor()
    {
        state = PersonState.WALKING_TO_DOOR;
    }

    // ring doorbell
    public void RingDoorbell()
    {
        state = PersonState.WAITING_FOR_DOOR;
        Arrived(this);
    }

    // toss cat
    public void TossCat()
    {
        if (heldCat != null)
        {
            heldCat.ResetBoredom();
            DestroyImmediate(joint);
            // not actually height, but proportional to it
            float tossHeight = Random.Range(0, maxTossSpeed);
            playSound(sounds[2]);
            spawnerComponent.LaunchCat(heldCat, new Vector3(1,tossHeight,0));
            heldCat = null;
        }
    }

    // leave after a delay
    public void Leave(float delay)
    {
        // reset timer
        timer = 0;
        // set delay
        this.delay = delay;
        // set state
        state = PersonState.DELAYING;
    }

    // leave immediately
    public void Leave()
    {
        giveFeedback = false;
        state = PersonState.WALKING_AWAY;
        Manager.instance.personManager.RemoveFromQueue(index);
    }

    public void Return()
    {
        returning = true;
        if (Manager.instance.personManager.GetQueuedPosition(index) != Vector3.zero)
        {
            GiveUp();
            return;
        }
        Manager.instance.personManager.Queue(this);
        GoToDoor();
    }

    // show thought bubble
    public void ShowThoughtBubble()
    {
        thoughtBubble.SetActive(true);
    }

    public void HideThoughtBubble()
    {
        thoughtBubble.SetActive(false);
    }

    // recieve cat
    public void CatRecieved(Cat cat)
    {
        // grab the cat
        heldCat = cat;
        // if the player is still holding on
        if (heldCat.GetComponent<FixedJoint>())
        {
            Destroy(heldCat.GetComponent<FixedJoint>());
        }
        joint = heldCat.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();

        // joy
        playSound(sounds[0], true);
        money.Play();

        // done, leave
        Leave();
    }

    void OnCollisionEnter(Collision col)
    {
        print("hit " + col.collider.name);

        // if this is hit by a cat
        Cat cat = col.gameObject.GetComponent<Cat>();
        if (returning && cat != null)
        {
            // if the cat is the right cat
            if (cat.ID == index)
            {
                // take the cat and leave
                CatRecieved(cat);
            } else
            {
                if (state == PersonState.WAITING_FOR_DOOR && returning)
                {
                    // incorrect cat sound
                    playSound(sounds[1]);
                    X_Sign.Play();
                }
            }
        }
    }

    public void Arrived(Person p)
    {
        // first time, don't show thought bubble
        if (!p.Returning)
        {
            door.RingDoorbell();
        }
        else
        {
            ShowThoughtBubble();
        }
    }

    // when the player is too slow
    public void GiveUp()
    {
        // remove from line
        Manager.instance.personManager.RemoveFromQueue(index);
        playSound(sounds[1], true); // lose sound
        Leave();
    }

    void playSound(AudioClip sound, bool overrideLock)
    {
        if (overrideLock)
        {
            a.clip = sound;
            a.Play();
        } else
        {
            playSound(sound);
        }
    }

    void playSound(AudioClip sound)
    {
        // repetitive sound guard
        if (soundLocked) return;

        a.clip = sound;
        a.Play();

        // lock sound
        StartCoroutine(BeginSoundCooldown(0.5f));
    }

    IEnumerator BeginSoundCooldown(float time)
    {
        soundLocked = true;
        yield return new WaitForSeconds(time);
        soundLocked = false;
    }
}
