using UnityEngine;
using System.Collections;

public class LevelSelectButton : MonoBehaviour {

    public bool right; // set in inspector

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Grabbed()
    {
        Manager.instance.ChangeSelectedLevel(right);
    }
}
