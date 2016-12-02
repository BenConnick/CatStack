using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

    public PersonManager personManager;
    public static Manager instance;
    //public TextMesh ScoreCounter;
    public GameObject[] WinBanners;
    public int CurrentLevelNum = 1; // 1-indexed CAUTION!
    public LevelData[] levels; // 0-indexed
    public Text[] scheduleCols; // list
    public Transform levelSlider;
    public GameObject playLevel;
    public AudioClip winSound;
    public ParticleSystem confetti;
    public GameObject gameDataPrefab;

    Transform player;
    public Transform Player
    {
        get
        {
            return player;
        }
    }
    TutorialManager tutManager;
    public TutorialManager tutorialManager
    {
        get
        {
            return tutManager;
        }
    }

    public LevelData CurrentLevel
    {
        get
        {
            return levels[CurrentLevelNum - 1];
        }
    }

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
    int selectedLevel = 1;
    AudioSource audioSource;

    // Use this for initialization
    void Start() {
        // Singleton
        if (Manager.instance == null)
        {
            Manager.instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }

        // game data
        if (GameData.instance == null)
        {
            GameObject.Instantiate(gameDataPrefab);
        }
        // this level
        selectedLevel = GameData.instance.currentLevel;
        CurrentLevelNum = GameData.instance.currentLevel ;
        // sync slider
        levelSlider.position = levelSlider.position + (CurrentLevelNum-1) * Vector3.right * 0.5f;

        // find player transform
        player = Camera.main.transform.parent;

        // component
        tutManager = GetComponent<TutorialManager>();

        PersonManager[] PMs = FindObjectsOfType<PersonManager>();

        foreach (PersonManager p in PMs)
        {
            if (p.isActiveAndEnabled)
            {
                personManager = p;
                break;
            }
        }

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("OnlyCat"), LayerMask.NameToLayer("Default"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("OnlyCat"), LayerMask.NameToLayer("SurfaceWorld"));

        audioSource = gameObject.AddComponent<AudioSource>();

        
    }

    // Update is called once per frame
    void Update() {
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
        Cat[] cats = FindObjectsOfType<Cat>();
        foreach (Cat c in cats)
        {
            GameObject.Destroy(c.gameObject);
        }
    }

    public void Win()
    {
        // text
        foreach (GameObject banner in WinBanners)
        {
            banner.SetActive(true);
        }
        //tutorial
        tutManager.LevelComplete();
        // confetti
        confetti.Play();
        // trumpets
        audioSource.clip = winSound;
        audioSource.Play();
        // unlock new level
        GameData.instance.numLevelsUnlocked++;
        // change visuals
        levelSlider.GetComponent<LevelSlider>().Refresh();
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
            t.rotation = Quaternion.Euler(0, 270, 0);
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

    // asdf
    public void ShowSchedule()
    {
        // world time goes from 9am to 5pm
        // game time goes from 0 to 240 s
        int l = CurrentLevel.returnTimes.Length;
        float totalTime = 240;
        float startTime = 9;
        float numHours = 9;
        float interval = totalTime / numHours; // turning point for each hour

        string timeStr = "Time\n---------";
        string dropoffStr = "Dropoff\n---------";
        string pickupStr = "Pickup\n---------";

        int[] dropPerHour = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] pickPerHour = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // collect number of spawns
        for (int i = 0; i < CurrentLevel.spawnTimes.Length; i++)
        {
            float sTime = CurrentLevel.spawnTimes[i] + 5;
            dropPerHour[(int)Mathf.Floor(sTime / interval)]++;
            float dTime = CurrentLevel.returnTimes[i] + 5;
            pickPerHour[(int)Mathf.Floor(dTime / interval)]++;
        }

        // strings
        for (int i = 0; i < numHours; i++)
        {
            timeStr += "\n" + (i + startTime) + ":00";
            dropoffStr += "\n" + dropPerHour[i];
            pickupStr += "\n" + pickPerHour[i];
        }

        scheduleCols[0].text = timeStr;
        scheduleCols[1].text = dropoffStr;
        scheduleCols[2].text = pickupStr;
    }

    // changes selected level back or forward
    public void ChangeSelectedLevel(bool next)
    {
        // if in range
        if ((next && selectedLevel < 5) || (!next && selectedLevel > 1))
        {
            float dir = next ? 0.5f : -0.5f;
            selectedLevel += (int)(dir * 2);
            Vector3 start = levelSlider.position;
            Vector3 end = levelSlider.position + dir * Vector3.right;
            StartCoroutine(lerpPosition(levelSlider, start, end, 20));
            if (selectedLevel <= GameData.instance.numLevelsUnlocked)
            {
                playLevel.SetActive(true);
            } else
            {
                playLevel.SetActive(false);
            }
        }
    }

    public void LoadSelectedLevel()
    {
        // set level
        GameData.instance.currentLevel = selectedLevel;
        // reload all
        SceneManager.LoadScene(0);
    }

    IEnumerator lerpPosition(Transform tf, Vector3 startPos, Vector3 endPos, int duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            timer++;
            tf.position = Vector3.Lerp(startPos, endPos, timer / duration);
            yield return new WaitForEndOfFrame();
        }
    }
}
