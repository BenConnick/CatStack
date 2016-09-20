using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

    AudioSource a;

	// Use this for initialization
	void Start () {
        a = GetComponent<AudioSource>();
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void RingDoorbell()
    {
        a.Play();
    }
}
