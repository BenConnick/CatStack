using UnityEngine;
using System.Collections;

public class LevelChooseButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Grabbed()
    {
        Manager.instance.LoadSelectedLevel();
    }
}
