using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour {

    public Transform hoverAboveThis;
    public float amplitude = 0.5f; // set in inspector

    Vector3 initialPosition;

	// Use this for initialization
	void Start () {
        initialPosition = transform.localPosition;
	}

    // Update is called once per frame
    void Update() {
        if (hoverAboveThis != null)
        {
            transform.position = hoverAboveThis.position + amplitude * Vector3.up * (1 + Mathf.Sin(Time.timeSinceLevelLoad));
        } else
        {
            transform.localPosition = initialPosition + amplitude * Vector3.up * Mathf.Sin(Time.timeSinceLevelLoad);
        }
	}
}
