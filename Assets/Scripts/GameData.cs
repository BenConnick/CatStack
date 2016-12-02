using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour {

    public static GameData instance;
    public int currentLevel = 1;
    public int numLevels = 5;
    public int numLevelsUnlocked = 1;

    public void Start()
    {
        // Singleton
        if (GameData.instance == null)
        {
            GameData.instance = this;
            // preserve this object
            DontDestroyOnLoad(this);
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
