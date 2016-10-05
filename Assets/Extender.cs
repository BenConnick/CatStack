using UnityEngine;
using System.Collections;

public class Extender : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.localPosition = new Vector3(0,0, transform.localPosition.z + (Input.mouseScrollDelta.y) / 20);
	}
}
