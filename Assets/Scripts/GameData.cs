using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour {

    public int currentLevel = 1;
    public int numLevels = 5;
    public int numLevelsUnlocked = 1;

	// Use this for initialization
	void Start () {
        GameData other = FindObjectOfType<GameData>();
        if (other != null)
        {
            Destroy(this);
        }
        // preserve this object
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
