using UnityEngine;
using System.Collections;

public class Extender : MonoBehaviour {

    PickUpAndThrow grabber;

	// Use this for initialization
	void Start () {
        grabber = GetComponentInChildren<PickUpAndThrow>();
	}
	
	// Update is called once per frame
	void Update () {
        if (grabber.Grabbing)
        {
            transform.localPosition = new Vector3(0, 0, transform.localPosition.z + (Input.mouseScrollDelta.y) / 20);
        } else
        {
            Ray ray = new Ray(transform.parent.position, transform.forward);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            float d = (hit.point - transform.parent.position).magnitude;
            transform.localPosition = new Vector3(0, 0, d - 0.8f);
        }
        
	}
}
