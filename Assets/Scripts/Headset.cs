using UnityEngine;
using System.Collections;

public class Headset : MonoBehaviour {

    Rigidbody r;
    bool active = false;

	// Use this for initialization
	void Start () {
        r = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Activate()
    {
        r.isKinematic = true;
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
        transform.parent = null;
        active = false;
    }

    public void Grabbed()
    {
        print("grabbed");
        gameObject.layer = LayerMask.NameToLayer("SurfaceWorld");
    }

    public void Released()
    {
        if (active)
        {
            transform.localPosition = new Vector3(0.1f, 0, 0);
            transform.localRotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            Activate();
        }
        if (other.name.ToLower().Contains("controller"))
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
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
            gameObject.layer = LayerMask.NameToLayer("SurfaceWorld");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
