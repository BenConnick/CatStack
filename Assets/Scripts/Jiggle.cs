using UnityEngine;
using System.Collections;

public class Jiggle : MonoBehaviour {

    // magnitude of the scale change
    public float jiggleAmount; // set in inspector

    // the speed of each cycle
    public float jiggleSpeed; // set in inspector

    // number of times expanding and shrinking
    public int cycles; // set in inspector

    // how much to reduce each succesive jiggle amount (if cycles > 1)
    public float decay; // set in spector

    // how many jiggles before done
    int jigglesLeft = 0;

    // save local scale
    Vector3 originalScale;

    // has the object been restored to its correct scale upon completion
    bool restored = true;

    // timer
    float timer = 0;

	// Use this for initialization
	void Start () {
        originalScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
	    if (jigglesLeft > 0)
        {
            timer += Time.deltaTime * jiggleSpeed;
            Resize();
            CheckCycle();
        } else
        {
            // restore object to original scale
            if (!restored)
            {
                transform.localScale = originalScale;
                restored = true;
            }
        }
	}

    public void Begin()
    {
        jigglesLeft = cycles;
        restored = false;
    }

    void Resize()
    {
        float s = 1 + jiggleAmount * Mathf.Sin(timer * Mathf.PI * 2);
        transform.localScale = new Vector3(
            originalScale.x * s,
            originalScale.y * s,
            originalScale.z * s
            );
    }

    void CheckCycle()
    {
        // track number of cycles
        if (timer > 1)
        {
            timer -= 1;
            jigglesLeft -= 1;
        }
    }
}
