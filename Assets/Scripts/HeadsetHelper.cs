using UnityEngine;
using System.Collections;

public class HeadsetHelper : MonoBehaviour {

    Headset owner;

	// Use this for initialization
	void Start () {
        owner = GetComponent<Headset>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.ToLower().Contains("controller"))
        {
            owner.show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.ToLower().Contains("controller"))
        {
            owner.hideActiveOnly();
        }
    }
}
