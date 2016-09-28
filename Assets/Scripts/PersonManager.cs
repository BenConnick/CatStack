using UnityEngine;
using System.Collections;

public class PersonManager : MonoBehaviour {

    public Door door;
    public float[] spawnTimes;
    public float[] returnTimes;

    public Person PersonPrefab;

    // timer for when persons will be spawned
    float timer = 0;

    // number of people shorthand
    int n;

    bool waiting = false;

    Person current;

    Person[] people;

    // the index of the next person to be spawned
    int nextPersonIndex = 0;
    int nextReturningPersonIndex = 0;

	// Use this for initialization
	void Start () {
        n = spawnTimes.Length;
        people = new Person[n];
	}

    // create a person
    void SpawnPerson()
    {
        // instantiate GO
        Person p = GameObject.Instantiate(PersonPrefab);
        // store in array
        people[nextPersonIndex] = p;
        // set id
        p.index = nextPersonIndex;
        // set position
        p.transform.position = transform.position;
        // a person starts walking as soon as it is created
    }
	
	// Update is called once per frame
	void Update () {
        // pause the countdown while waiting 
        // for the player to open the door (temporary)
        if (waiting)
        {
            // when the player opens the door
            if (door.Open)
            {
                // drop off cat
                if (!current.Returning)
                {
                    // throw the cat into the room
                    current.TossCat();
                    // and leave after 2 seconds
                    current.Leave(2);
                    // continue timer
                    waiting = false;
                }
                // pick up cat
                else
                {
                    // wait until the player actually gives you the cat
                }
            }
        }
        else
        {
            // increment timer
            timer += Time.deltaTime;
            // spawn a person when the timer hits their spawn timer
            if (n > nextPersonIndex && 
                timer > spawnTimes[nextPersonIndex])
            {
                // create
                SpawnPerson();
                // wait until next person
                nextPersonIndex++;
            }
            // if it is time for a person to go back
            if (n > nextReturningPersonIndex && 
                timer > returnTimes[nextReturningPersonIndex])
            {
                // person come back
                people[nextReturningPersonIndex].Return();
                // advance
                nextReturningPersonIndex++;
            }
        }

	}

    public void Arrived(Person p)
    {
        // first time, don't show thought bubble
        if (!p.Returning)
        {
            door.RingDoorbell();
            waiting = true;
            current = p;
        } else
        {
            waiting = true;
            p.ShowThoughtBubble();
        }
    }
}
