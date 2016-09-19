using UnityEngine;
using System.Collections;

public class Lift : MonoBehaviour {

    float distPerTelport = 0.5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GoUp()
    {
        /*Cat[] cats = FindObjectsOfType<Cat>();
        foreach (Cat cat in cats)
        {
            cat.transform.position += distPerTelport * Vector3.up;
        }*/
        transform.position = new Vector3(0, distPerTelport, 0) + transform.position;
    }

    public void GoDown()
    {
        transform.position = new Vector3(0, -1*distPerTelport, 0) + transform.position;
    }
}
