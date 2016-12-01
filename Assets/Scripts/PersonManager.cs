using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PersonManager : MonoBehaviour {

    public Door door;
    float[] spawnTimes;
    float[] returnTimes;
    List<Person> PersonQueue;

    public Person PersonPrefab;

    // timer for when persons will be spawned
    float timer = 0;

    // number of people shorthand
    int n;

    bool paused = true;

    bool done = false;

    Person[] people;

    // the index of the next person to be spawned
    int nextPersonIndex = 0;
    int nextReturningPersonIndex = 0;

	// Use this for initialization
	void Start () {
        spawnTimes = Manager.instance.CurrentLevel.spawnTimes;
        returnTimes = Manager.instance.CurrentLevel.returnTimes;

        n = spawnTimes.Length;
        // create a n-length array
        people = new Person[n];

        // create the queue for people standing in line for the door
        PersonQueue = new List<Person>();

        // ready for schedule
        Manager.instance.ShowSchedule();

        // create the first person
        SpawnPerson();
        // wait until next person
        nextPersonIndex++;
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
        if (paused)
        {
            // do nothing
            Manager.instance.Reset();
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

            // all done
            if (nextReturningPersonIndex == n && PersonQueue.Count == 0 && !done)
            {
                Manager.instance.Win();
                done = true;
            }
        }

	}

    public int GetQueueIndex(int idx)
    {
        return PersonQueue.IndexOf(people[idx]);
    }

    public void RemoveFromQueue(int idx)
    {
        PersonQueue.Remove(people[idx]);
        paused = false;
    }

    public Vector3 GetQueuedPosition(int idx)
    {
        Vector3 pos = Vector3.zero;
        // the position in line starting at the front (1)
        int posInLine = 0;
        foreach (Person p in PersonQueue) {
            posInLine++;
            if (p.index == idx)
            {
                pos = p.DoormatLoc + 2 * Vector3.right - 2 * Vector3.right * posInLine;
            }
        }
        return pos;
    }

    // add a person to the back of the line
    public void Queue(Person p)
    {
        PersonQueue.Add(p);
    }

    // remove the person in the front of the line
    public void Dequeue()
    {
        PersonQueue.RemoveAt(0);
        paused = false;
    }
}
