using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour {

    enum Lessons { OPEN_DOOR, PET_CAT, RETURN_CAT, LEVEL_SELECT }
    int lessonNum = 0;
    bool tutorialRunning = false;
    bool catPet = false;

    public bool TutorialRunning
    {
        get
        {
            return tutorialRunning;
        }
    }

    // refereces to every tutorial GO
    public GameObject[] tutorialParts;
    
    // refernces for checking conditions
    Door door;
    Cat cat;

    // Use this for initialization
    void Start () {
        HideAll();
	    if (Manager.instance.CurrentLevelNum == 1)
        {
            BeginTutorial();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (tutorialRunning && CheckCondition()) { NextPart(); }
	}

    // begin the tutorial
    void BeginTutorial()
    {
        tutorialRunning = true;

        // door ref
        door = FindObjectOfType<Door>();

        // start from the beginning
        lessonNum = 0;
        ShowCurrent();
    }

    void EndTutorial()
    {
        HideAll();
        tutorialRunning = false;
    }

    // check if condition has been met
    bool CheckCondition()
    {
        switch (lessonNum)
        {
            case (int)Lessons.OPEN_DOOR:
                if (door.Open) { return true; }
                break;
            case (int)Lessons.PET_CAT:
                if (cat.transform.localScale.x < 0.6f)
                {
                    HideAll();
                    catPet = true;
                }
                break;
        }

        // none
        return false;
    }

    // condition met, show next
    public void NextPart()
    {
        lessonNum++;
        if (lessonNum >= tutorialParts.Length)
        {
            EndTutorial();
            return;
        }
        ShowCurrent();
        SpecialActions();
    }

    // actions unique to specific steps
    void SpecialActions()
    {
        switch(lessonNum)
        {
            // put text above cat
            case (int)Lessons.PET_CAT:
                // first cat
                cat = FindObjectOfType<Cat>();
                // hover above cat (do not parent)
                tutorialParts[lessonNum].GetComponent<Floater>().hoverAboveThis = cat.transform;
                break;
        }
    }

    // show the current tutorial part (only)
    void ShowCurrent()
    {
        HideAll();
        // show current
        tutorialParts[lessonNum].SetActive(true);
    }

    // hide the tutorial
    void HideAll()
    {
        // hide all
        foreach (GameObject part in tutorialParts)
        {
            part.SetActive(false);
        }
    }

    public void CatRecieved()
    {
        if (lessonNum == (int)Lessons.RETURN_CAT)
        {
            HideAll();
        }
    }

    public void PersonReturned()
    {
        if (lessonNum == (int)Lessons.PET_CAT && catPet)
        {
            NextPart();
        }
    }

    public void LevelComplete()
    {
        if (lessonNum == (int)Lessons.RETURN_CAT)
        {
            NextPart();
        }
    }
}
