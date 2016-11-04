using UnityEngine;
using System.Collections;

public class PickUpAndDrop : MonoBehaviour {

    public Rigidbody attachPoint;
    public GameObject rotationTool;

    FixedJoint joint;

    Quaternion startRot;
    Vector2 TouchStart;
    int prevLayer;

    public Sprite OpenHandSprite;
    public Sprite GraspingHandSprite;

    GameObject overlappingObj;

    ParticleSystem ps;

    void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }

    protected void FixedUpdate()
    {
        // keep collisions alive
        transform.Translate(Vector3.zero);

        if (joint == null && Input.GetMouseButtonDown(0))
        {
            // no object
            if (!overlappingObj) return;

            // shorthand
            var go = overlappingObj;

            // rigidbodies only
            if (go.GetComponent<Rigidbody>() == null) return;

            // immovable tag exception
            if (go.CompareTag("Immovable")) return;

            joint = go.AddComponent<FixedJoint>();
            joint.connectedBody = attachPoint;
            prevLayer = go.layer;
            go.layer = LayerMask.NameToLayer("OnlyCat");

            //rotationTool.GetComponent<SpriteRenderer>().sprite = GraspingHandSprite;
        }
        else if (joint != null && Input.GetMouseButtonUp(0))
        {
            rotationTool.GetComponent<SpriteRenderer>().sprite = OpenHandSprite;

            var go = joint.gameObject;
            Object.DestroyImmediate(joint);
            joint = null;
            go.layer = prevLayer;

            var rigidbody = go.GetComponent<Rigidbody>();

            // We should probably apply the offset between trackedObj.transform.position
            // and device.transform.pos to insert into the physics sim at the correct
            // location, however, we would then want to predict ahead the visual representation
            // by the same amount we are predicting our render poses.

            var origin = transform;
            if (origin != null)
            {
                //rigidbody.velocity = origin.TransformVector(device.velocity);
                //rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
            }
            else
            {
                //rigidbody.velocity = device.velocity;
                //rigidbody.angularVelocity = device.angularVelocity;
            }

            rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

            rigidbody.WakeUp();
        }
        /* Rotate the held object with the touch pad
		 * if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad)) {
			startRot = rotationTool.transform.rotation;
			TouchStart = device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0);
		}
		if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad)) {
				
		}
		if (device.GetTouch (SteamVR_Controller.ButtonMask.Touchpad)) {
			Vector2 touch = device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0);
			rotationTool.transform.rotation = startRot;
			rotationTool.transform.RotateAround (rotationTool.transform.position, transform.forward, (touch.x - TouchStart.x) * (-180));
			rotationTool.transform.RotateAround (rotationTool.transform.position, transform.right, (touch.y - TouchStart.y) * 180);

			if (joint != null) {
				attachPoint.transform.rotation = rotationTool.transform.rotation;
			}
		}*/
    }

    protected void OnTriggerEnter(Collider col)
    {

        // rigidbodies only
        if (col.GetComponent<Rigidbody>() == null) return;

        // immovable tag exception
        if (col.CompareTag("Immovable")) return;

        overlappingObj = col.gameObject;

        if (ps != null)
        {
            ps.Play();
        }
    }
    protected void OnTriggerExit(Collider col)
    {
        if (col.gameObject == overlappingObj) overlappingObj = null;
        if (ps != null)
        {
            ps.Stop();
        }
    }
}
