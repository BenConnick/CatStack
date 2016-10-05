using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {

    public Material[] CatSkins;
    public PersonManager personManager;
    public static Manager instance;
    //public TextMesh ScoreCounter;

    bool gameOver = true;
    int score = 0;

    // Use this for initialization
    void Start () {

        // Singleton
	    if (Manager.instance == null)
        {
            Manager.instance = this;
        } else
        {
            GameObject.Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        // VR disabled, play override
        if (Input.GetKeyDown(KeyCode.P))
        {
            FindObjectOfType<CatSpawner>().Activate();
        }
	}

    public void PlayGame()
    {
        if (gameOver)
        {
            gameOver = false;
            score = 0;
        }
    }

    public void GameOver()
    {
        gameOver = true;
        FindObjectOfType<CatSpawner>().Deactivate();
        Cat[] cats = FindObjectsOfType<Cat> ();
        foreach (Cat c in cats)
        {
            GameObject.Destroy(c.gameObject);
        }
    }

    public void AddPoints(int points)
    {
        score += points;
        //ScoreCounter.text = score.ToString();
    }

    public void LogTime()
    {
        print("button pressed: " + Time.timeSinceLevelLoad);
    }
}
