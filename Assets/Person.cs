using UnityEngine;
using System.Collections;

public class Person : MonoBehaviour {

    public float WalkSpeed;

    CatSpawner spawnerComponent;

    enum PersonState { WALKING_TO_DOOR, WAITING_FOR_RESPONSE, WALKING_AWAY }

    PersonState state;

    Vector3 doormatLocation;

    float targetDistanceTolerance = 0.5f;

    Cat heldCat;

    // Use this for initialization
    void Start() {
        // get component reference
        spawnerComponent = GetComponent<CatSpawner>();

        // calculate doormat location
        doormatLocation = GameObject.Find("Door").transform.position - Vector3.right;

        GoToDoor();
    }

    // Update is called once per frame
    void Update() {
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

    // go to door
    void GoToDoor()
    {
        heldCat = spawnerComponent.SpawnCat();
        state = PersonState.WALKING_TO_DOOR;
    }

    // ring doorbell
    void RingDoorbell()
    {
        state = PersonState.WAITING_FOR_RESPONSE;
    }

    // toss cat
    void TossCat()
    {

    }

    // leave
    void Leave()
    {

    }

    // show thought bubble
    void ShowThoughtBubble()
    {

    }

    // recieve cat
    void CatRecieved(Cat cat)
    {

    }
}
