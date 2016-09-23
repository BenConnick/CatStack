using UnityEngine;
using System.Collections;

public class Person : MonoBehaviour {

    CatSpawner spawnerComponent;

    enum PersonState { WALKING_TO_DOOR, WAITING_FOR_RESPONSE, WALKING_AWAY }

    // Use this for initialization
    void Start() {
        spawnerComponent = GetComponent<CatSpawner>();
    }

    // Update is called once per frame
    void Update() {

    }

    // go to door
    void GoTodoor()
    {

    }

    // ring doorbell
    void RingDoorbell()
    {

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
