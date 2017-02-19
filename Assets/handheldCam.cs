using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handheldCam : MonoBehaviour {

    Vector3 p;
    float amount = 0.01f;

	// Use this for initialization
	void Start () {
        p = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(p.x + amount * Mathf.Sin(Time.timeSinceLevelLoad * 4f / 5f), p.y + amount * Mathf.Sin(Time.timeSinceLevelLoad), p.z);
	}
}
