using UnityEngine;
using System.Collections;

public class Headset : MonoBehaviour {

    Rigidbody r;
    bool active = false;
    bool grabbed = false;

    Vector3 normalScale;

	// Use this for initialization
	void Start () {
        normalScale = transform.localScale;
        r = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Activate()
    {
        r.isKinematic = true;
        transform.localScale = normalScale * 2;
        transform.parent = Camera.main.transform;
        Manager.instance.Player.position = new Vector3(Manager.instance.Player.position.x, 10, Manager.instance.Player.position.z);
        Camera.main.cullingMask = ~(1 << 10);
        active = true;
    }

    public void Deactivate()
    {
        Manager.instance.Player.position = new Vector3(Manager.instance.Player.position.x, 0, Manager.instance.Player.position.z);
        Camera.main.cullingMask = ~(1 << 11);
        r.isKinematic = false;
        transform.localScale = normalScale;
        transform.parent = null;
        active = false;
    }

    public void Grabbed()
    {
        grabbed = true;
        gameObject.layer = LayerMask.NameToLayer("SurfaceWorld");
    }

    public void Released()
    {
        grabbed = false;
        if (active)
        {
            transform.localPosition = new Vector3(0.1f, 0, 0);
            transform.localRotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!grabbed) return;
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            Activate();
        }
        if (other.name.ToLower().Contains("controller"))
        {
            show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            Deactivate();
        }
        if (other.name.ToLower().Contains("controller"))
        {
            hideActiveOnly();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    public void show()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void hideActiveOnly()
    {
        gameObject.layer = LayerMask.NameToLayer("SurfaceWorld");
    }
}
