using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {

    public PersonManager personManager;
    public static Manager instance;
    //public TextMesh ScoreCounter;
    public GameObject[] WinBanners;

    Transform activeToy;

    public Transform ActiveToy
    {
        get
        {
            return activeToy;
        }

        set
        {
            activeToy = value;
        }
    }

    bool gameOver = true;
    int score = 0;

    // Use this for initialization
    void Start () {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("OnlyCat"), LayerMask.NameToLayer("Default"));

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

    public void Win()
    {
        foreach (GameObject banner in WinBanners)
        {
            banner.SetActive(true);
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

    // 180 the room
    public void Flip()
    {
        Transform t = FindObjectOfType<SteamVR_PlayArea>().transform;
        if (t.rotation.eulerAngles.y == 90)
        {
            t.rotation = Quaternion.Euler(0,270,0);
        } else
        {
            t.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    // reset clock and sun
    public void Reset()
    {
        Rotate[] rotators = (Rotate[])FindObjectsOfType(typeof(Rotate));
        foreach (Rotate r in rotators)
        {
            r.Reset();
        }
    }
}
