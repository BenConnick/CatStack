﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class PickUpAndThrow: MonoBehaviour
{
	public Rigidbody attachPoint;
	public GameObject rotationTool;

	SteamVR_TrackedObject trackedObj;
	FixedJoint joint;
    Collider disabledCollider;
    int prevLayer;

	Quaternion startRot;
	Vector2 TouchStart;

	public Sprite OpenHandSprite;
	public Sprite GraspingHandSprite;

	GameObject overlappingObj;

    Animator animator;

    bool grabbedAnim = false;

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
        if (GetComponentInChildren<Animator>())
        {
            animator = GetComponentInChildren<Animator>();
        }
	}

	void FixedUpdate()
	{
		// keep collisions alive
		transform.Translate(Vector3.zero);

		var device = SteamVR_Controller.Input((int)trackedObj.index);

        // ------ Animate Hand ---------

        if (animator != null)
        {
            // grab animation
            if (!grabbedAnim && device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                animator.SetBool(Animator.StringToHash("Grabbed"), true);
                grabbedAnim = true;

            }
            // let go animation
            if (grabbedAnim && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                animator.SetBool(Animator.StringToHash("Grabbed"), false);
                grabbedAnim = false;

            }
        }

        // ----- Grabbing Objects

        if (joint == null && device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
		{
            // no object
			if (overlappingObj == null) return;

            // shorthand
            var go = overlappingObj;

            // rigidbodies only
            if (go.GetComponent<Rigidbody>() == null) return;

            // immovable tag exception
            if (go.CompareTag("Immovable")) return;

            // create the joint
            joint = go.AddComponent<FixedJoint>();
            joint.connectedBody = attachPoint;
            // disable collision
            prevLayer = go.layer;
            go.layer = LayerMask.NameToLayer("OnlyCat");


            device.TriggerHapticPulse(1000);

            //rotationTool.GetComponent<SpriteRenderer> ().sprite = GraspingHandSprite;

            
		}
		else if (joint != null && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
		{
			// let go
			var go = joint.gameObject;
            Object.DestroyImmediate(joint);
            go.layer = prevLayer;
            joint = null;

            // rigidbody for throw
            var rigidbody = go.GetComponent<Rigidbody>();

            // We should probably apply the offset between trackedObj.transform.position
            // and device.transform.pos to insert into the physics sim at the correct
            // location, however, we would then want to predict ahead the visual representation
            // by the same amount we are predicting our render poses.

            var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
			if (origin != null)
			{
				rigidbody.velocity = origin.TransformVector(device.velocity);
				rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
			}
			else
			{
				rigidbody.velocity = device.velocity;
				rigidbody.angularVelocity = device.angularVelocity;
			}

			rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

			rigidbody.WakeUp ();
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

	void OnTriggerEnter(Collider col) {
        SteamVR_Controller.Input((int)trackedObj.index).TriggerHapticPulse(1000);
        // rigidbodies only
        if (col.GetComponent<Rigidbody>() == null) return;
        // immovable tag exception
        if (col.CompareTag("Immovable")) return;
        overlappingObj = col.gameObject;
        
    }
	void OnTriggerExit(Collider col) {
		if (col.gameObject == overlappingObj) overlappingObj = null;
	}

    
}
