using UnityEngine;
using System.Collections;

public class StringSegment : MonoBehaviour {

    Rigidbody r;
    LineRenderer line;
    Transform connectedSeg;

    SpringJoint sj;

    public bool useSpringJointInstead = false; // set in inspector

    bool allValid = true;

    // Use this for initialization
    void Start() {
        r = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        if (useSpringJointInstead)
        {
            sj = GetComponent<SpringJoint>();
            connectedSeg = sj.connectedBody.transform;
            
        }
        else
        {
            connectedSeg = GetComponent<ConfigurableJoint>().connectedBody.transform;
        }

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
            if (useSpringJointInstead)
            {
                line.SetPosition(0, transform.position - transform.forward * 0.05f);
                line.SetPosition(1, connectedSeg.position + connectedSeg.forward * 0.08f);
            }
            else
            {
                line.SetPosition(0, transform.position - 0.001f * toVec);
                line.SetPosition(1, connectedSeg.position + 0.001f * toVec);
            }
        }
	}
}
