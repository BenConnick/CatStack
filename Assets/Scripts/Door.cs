using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

    AudioSource a;
    public AudioClip doorbellSound;
    public AudioClip closeSound;
    public AudioClip slamSound;
    public Rigidbody handle;

    Vector3 handleLocalPos;

    // the angle of the door
    float angle = 0;
    // the diff between angle since last frame
    float angleDelta = 0;

    // door hinge friction
    float frictionCoeff = 3;

    // the speed the door is moving at
    float angularSpeed;

    // debug
    public bool doorbellMuted; // set in inspector

    // was the handle just released?
    bool doorSpeedApplied = true;

    public bool Open {
        get {
            return angle > 20;
        }
    }

    public bool Closed
    {
        get
        {
            return angle == 0;
        }
    }

    bool closeSoundPlayed = false;

	// Use this for initialization
	void Start () {
        a = gameObject.AddComponent<AudioSource>();
        a.clip = doorbellSound;
        handleLocalPos = handle.transform.localPosition;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        //transform.LookAt(new Vector3(handle.transform.position.x, transform.position.y, handle.transform.position.z));
        if (handle.GetComponent<FixedJoint>())
        {
            // this only matters once ther joint is gone
            doorSpeedApplied = false;
            angularSpeed = 0;

            // store prev
            float prevAngle = angle;

            // find how far the door should be open
            angle = Vector3.Angle(
                Vector3.ProjectOnPlane(handle.position, Vector3.up) 
                - Vector3.ProjectOnPlane(transform.position, Vector3.up), 
                Vector3.forward
                );

            // flipped angle
            if (handle.position.x  < transform.position.x)
            {
                angle *= -1;
            }

            // keeps the angle in bounds
            ClampAngle();

            // rotate so that the handle matches the player's hand
            transform.eulerAngles = new Vector3(-90, angle, 0);

            // delta
            //angleDelta = (angle - prevAngle) * Time.fixedDeltaTime;

            // set speed
            //angularSpeed = angleDelta / Time.fixedDeltaTime;
        } else
        {
            if (!doorSpeedApplied)
            {
                int direction = handle.velocity.x > 0 ? 1 : -1;
                angularSpeed = handle.velocity.magnitude * 2 * direction;
                handle.velocity = Vector3.zero;
                doorSpeedApplied = true;
            }

            // recenter handle collider
            handle.transform.localPosition = handleLocalPos;

            // decelerate due to friction
            if (angularSpeed > 0) {
                angularSpeed -= Time.fixedDeltaTime * frictionCoeff;
            } else if (angularSpeed < 0)
            {
                angularSpeed += Time.fixedDeltaTime * frictionCoeff;
            }

            if (Mathf.Abs(angularSpeed) < Time.fixedDeltaTime * frictionCoeff)
            {
                angularSpeed = 0;
            }

            // rotate from inerita
            angle += angularSpeed;
            ClampAngle();

            // rotate so that the handle matches the player's hand
            transform.eulerAngles = new Vector3(-90, angle, 0);
        }
	}

    void ClampAngle()
    {
        if (angle > 80)
        {
            angle = 80;
            angularSpeed = 0;
        }

        if (angle < 0.1f)
        {
            DoorClosed();
            angle = 0;
            angularSpeed = 0;
        } else
        {
            if (closeSoundPlayed)
            {
                // open sound
                DoorOpened();
                // door can be closed
                closeSoundPlayed = false;
            }
        }
    }

    public void RingDoorbell()
    {
        if (doorbellMuted) return;

        if (a == null)
        {
            print("no audio source");
            return;
        }
        a.pitch = 1;
        a.clip = doorbellSound;
        a.Play();
    }

    void DoorClosed()
    {
        if (!closeSoundPlayed)
        {
            // click
            if (angularSpeed > -10)
            {
                a.pitch = 1;
                a.clip = closeSound;
                a.Play();
            }
            // SLAM!!!!
            else
            {
                a.pitch = 1;
                a.clip = slamSound;
                a.Play();
            }
            closeSoundPlayed = true;
        }
    }

    void DoorOpened()
    {
        a.pitch = 3;
        a.clip = closeSound;
        a.Play();
    }
}
