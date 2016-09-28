using UnityEngine;
using System.Collections;

public class Person : MonoBehaviour {

    public int index;

    public float WalkSpeed;

    CatSpawner spawnerComponent;

    enum PersonState { WALKING_TO_DOOR, WAITING_FOR_RESPONSE, DELAYING, WALKING_AWAY }

    PersonState state;

    Vector3 doormatLocation;

    float targetDistanceTolerance = 0.25f;

    Cat heldCat;

    FixedJoint joint;

    GameObject thoughtBubble;

    public SkinnedMeshRenderer catThought;

    // reusable timer
    float timer;

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

    // Use this for initialization
    void Start() {
        // get component reference
        spawnerComponent = GetComponent<CatSpawner>();

        // get thought bubble reference
        thoughtBubble = transform.FindChild("ThoughtBubble").gameObject;
        // start hidden
        HideThoughtBubble();

        // calculate doormat location
        doormatLocation = GameObject.Find("Door").transform.position - Vector3.right + Vector3.forward * 0.5f;

        GetCat();
        GoToDoor();
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (state) {
            case PersonState.WALKING_TO_DOOR:
                // returns true if the destination was reached
                if (WalkingToLocation(doormatLocation))
                {
                    RingDoorbell();
                }
                break;
            case PersonState.WAITING_FOR_RESPONSE:
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

        // if distance is less than tolerance return
        if (toVec.magnitude < targetDistanceTolerance)
        {
            // arrived at destination
            return true;
        }

        // move towards the location in he XZ plane using speed
        transform.position += Vector3.ProjectOnPlane(toVec.normalized,Vector3.up) * Time.deltaTime * WalkSpeed;

        // moving to destination
        return false;
    }

    public void GetCat()
    {
        heldCat = spawnerComponent.SpawnCat(index);
        joint = heldCat.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();

        // set the skin of the cat thought to match the actual cat
        catThought.material = Manager.instance.CatSkins[heldCat.CatType];
    }

    // go to door
    public void GoToDoor()
    {
        state = PersonState.WALKING_TO_DOOR;
    }

    // ring doorbell
    public void RingDoorbell()
    {
        state = PersonState.WAITING_FOR_RESPONSE;
        Manager.instance.personManager.Arrived(this);
    }

    // toss cat
    public void TossCat()
    {
        if (heldCat != null)
        {
            DestroyImmediate(joint);
            spawnerComponent.LaunchCat(heldCat);
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
        state = PersonState.WALKING_AWAY;
    }

    public void Return()
    {
        returning = true;
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

        // done, leave
        Leave();
    }

    void OnCollisionEnter(Collision col)
    {
        print("hit " + col.collider.name);

        // if this is hit by a cat
        Cat cat = col.gameObject.GetComponent<Cat>();
        if (cat != null)
        {
            // if the cat is the right cat
            if (cat.ID == index)
            {
                // take the cat and leave
                CatRecieved(cat);
            }
        }
    }
}
