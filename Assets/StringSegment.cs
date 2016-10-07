using UnityEngine;
using System.Collections;

public class StringSegment : MonoBehaviour {

    Rigidbody r;
    LineRenderer line;
    Transform connectedSeg;

    bool allValid = true;

	// Use this for initialization
	void Start () {
        r = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        connectedSeg = GetComponent<ConfigurableJoint>().connectedBody.transform;

        if (r != null && line != null && connectedSeg != null)
        {
            allValid = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
	    if (allValid)
        {
            Vector3 toVec = connectedSeg.position - transform.position;
            toVec.Normalize();
            line.SetPosition(0, transform.position - 0.001f * toVec);
            line.SetPosition(1, connectedSeg.position + 0.001f * toVec);
        }
	}
}
