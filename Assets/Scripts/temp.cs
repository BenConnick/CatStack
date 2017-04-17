using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temp : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time > 5)
        transform.position = new Vector3(transform.position.x  - Time.deltaTime, transform.position.y, transform.position.z);
	}
}
