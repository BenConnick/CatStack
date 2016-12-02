using UnityEngine;
using System.Collections;

public class LevelSlider : MonoBehaviour {

    public GameObject LockedPrefab;
    public GameObject UnlockedPrefab;

    // Use this for initialization
    void Start () {
        Clear();
        Fill();
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Clear()
    {
        for (var t = 0; t < transform.childCount; t++)
        {
            Destroy(transform.GetChild(t).gameObject);
        }
    }

    public void Fill()
    {
        int numLevels = 5;
        for (var i=0; i<numLevels; i++)
        {
            GameObject newCat = null;
            if (i<Manager.instance.gameData.numLevelsUnlocked)
            {
                newCat = (GameObject)GameObject.Instantiate(UnlockedPrefab, transform, false);
            } else
            {
                newCat = (GameObject)GameObject.Instantiate(LockedPrefab, transform, false);
            }
            newCat.transform.localPosition = new Vector3(-0.5f * i, 0, 0);
            newCat.transform.FindChild("LevelNum").gameObject.GetComponent<TextMesh>().text = "Lvl. " + (i+1);
        }
    }
}
