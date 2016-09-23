using UnityEngine;
using System.Collections;

public class VRButton : MonoBehaviour {

    public GameObject target;
    public string message;
    public AudioClip PressSound;

    float timer;
    float animationDuration = 0.1f;

    AudioSource aSource;
    
	// Use this for initialization
	void Start () {
        aSource = gameObject.AddComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        Animate();
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.Contains("Controller"))
        {
            if (timer > animationDuration)
                Press();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name.Contains("Controller"))
        {
            if (timer > animationDuration)
                Press();
        }
    }

    void OnBeginOverlap(Collider col)
    {
        if (col.name == "Grabber")
        {
            Press();
        }
    }

    void Press()
    {
        aSource.clip = PressSound;
        aSource.Play();
        target.SendMessage(message);
        timer = 0;
    }

    void Animate()
    {
        float d = 0.03f;
        float a = Mathf.Min(1, timer / animationDuration);
        transform.localPosition = new Vector3(transform.localPosition.x, 2 * d * Mathf.Abs(0.5f - a), transform.localPosition.z);
    }
}
