using UnityEngine;
using System.Collections;

public class MusicDisk : MonoBehaviour {

    bool playing = false;
    bool grabbed = false;
    float angle = 0;
    float speed = 100;
    public AudioClip song;
    AudioSource a;

	// Use this for initialization
	void Start () {
        a = gameObject.AddComponent<AudioSource>();
        a.loop = true;
        a.volume = 1;
        a.clip = song;
	}
	
	// Update is called once per frame
	void Update () {
	    if (playing)
        {
            angle += speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(new Vector3(0,angle,0));
        }
	}

    void PlayDisk() {
        playing = true;
        GetComponent<Rigidbody>().isKinematic = true;
        a.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!playing && other.gameObject.GetComponent<RecordPlayer>())
        {
            RecordPlayer rp = other.gameObject.GetComponent<RecordPlayer>();
            transform.position = rp.centerLocation.position;
            PlayDisk();
        }
    }

    public void Grabbed()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        playing = false;
        a.Pause();
    }
}
