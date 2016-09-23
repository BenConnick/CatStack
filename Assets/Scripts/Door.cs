using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

    AudioSource a;
    public AudioClip doorbellSound;

	// Use this for initialization
	void Start () {
        a = gameObject.AddComponent<AudioSource>();
        a.clip = doorbellSound;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void RingDoorbell()
    {
        if (a == null)
        {
            print("no audio source");
        } else {
            // nothing
            print("fuck");
        }
        a.Play();
    }
}
