using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour {

    public Transform hoverAboveThis; // set in inspector
    public float hoverHeight = 0f; // set in inspector
    public float amplitude = 0.5f; // set in inspector
    public float speed = 1f; // set in inspector
    public bool faceCamera; // set in inspector

    Vector3 initialPosition;

	// Use this for initialization
	void Start () {
        initialPosition = transform.localPosition;
	}

    // Update is called once per frame
    void Update() {
        if (hoverAboveThis != null)
        {
            transform.position = hoverAboveThis.position + amplitude * Vector3.up * (hoverHeight / amplitude + 1 + Mathf.Sin(Time.timeSinceLevelLoad * speed));
            if (faceCamera)
            {
                transform.forward = Vector3.ProjectOnPlane(transform.position - Manager.instance.Player.position, Vector3.up);
            }
        } else
        {
            transform.localPosition = initialPosition + amplitude * Vector3.up * Mathf.Sin(Time.timeSinceLevelLoad * speed);
        }
	}
}
