using UnityEngine;
using System.Collections;

public class KillZ : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    

    void OnTriggerEnter(Collider col)
    {
        print("hit");
        if (col.name.Contains("Cat")) {
            Manager.instance.GameOver();
        }
    }
}
