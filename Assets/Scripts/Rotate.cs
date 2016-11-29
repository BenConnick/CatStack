using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    bool singleAxisRotation = true;
    public Vector3 RotationAxis; // set in inspector
    public float InitialRotationDeg = 0f; // set in inspector
    public float CyclesPerSecond = 1f; // set in inspector
    public bool useLocalRotation = false; // set in inspector

    float timeElapsed = 0;
    float rotationDeg = 0;

    // Use this for initialization
    void Start () {
        rotationDeg = InitialRotationDeg;
	}

    // Update is called once per frame
    void Update() {
        if (singleAxisRotation)
        {
            timeElapsed += Time.deltaTime;
            rotationDeg = InitialRotationDeg + CyclesPerSecond * 360 * timeElapsed;
            if (useLocalRotation)
            {
                transform.localRotation = Quaternion.AngleAxis(rotationDeg, RotationAxis);
            }
            else
            {
                transform.rotation = Quaternion.AngleAxis(rotationDeg, RotationAxis);
            }
        }
	}

    public void Reset()
    {
        timeElapsed = 0;
    }
}
