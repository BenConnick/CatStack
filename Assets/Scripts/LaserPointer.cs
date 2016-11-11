using UnityEngine;
using System.Collections;

public class LaserPointer : MonoBehaviour {

    // TODO: Make raycast to determine laser length
    // make (public accessor) transform for hit point of raycast
    // turn off line renderer when held

    LineRenderer laser;
    bool on = false;
    public Transform dot;
    ParticleSystem particles;

    RaycastHit hit;

	// Use this for initialization
	void Start () {
        laser = GetComponent<LineRenderer>();
        particles = dot.GetComponent<ParticleSystem>();
        TurnOff();
	}
	
	// Update is called once per frame
	void Update () {
	    if (on)
        {
            // raycast to hit point
            Ray ray = new Ray(transform.position, transform.up);
            Physics.Raycast(ray, out hit);
            if (hit.transform != null)
            {
                float length = (hit.point - transform.position).magnitude;
                laser.SetPositions(new Vector3[] { Vector3.zero, new Vector3(0, length / transform.localScale.y, 0) });
                dot.transform.position = hit.point;
            }
        }
	}

    void Grabbed()
    {
        // turn on automatically
        TurnOn();
        Manager.instance.ActiveToy = dot;
    }

    void Released()
    {
        // turn off automatically
        TurnOff();
        if (Manager.instance.ActiveToy == dot)
        {
            Manager.instance.ActiveToy = null;
        }
    }

    void TurnOn()
    {
        laser.enabled = true;
        particles.Play();
        on = true;
        
    }

    void TurnOff()
    {
        laser.enabled = false;
        particles.Stop();
        on = false;
    }
}
