﻿using UnityEngine;
using System;
using Photon.Pun;

namespace BottomGear
{
	[Serializable]
	public enum DriveType
	{
		RearWheelDrive,
		FrontWheelDrive,
		AllWheelDrive
	}

	[RequireComponent(typeof(Rigidbody))]

	public class WheelDrive : MonoBehaviour
	{
		// --------------------- Variables -------------------------
		public AK.Wwise.Event engine_sound;
		public AK.Wwise.Event crash_sound;
		bool play_crash = false;

		[Header("Wheels")]
		[Tooltip("The vehicle's wheel count")]
		public int wheelCount = 4;
		[Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
		public DriveType driveType;
		[Tooltip("Maximum steering angle of the wheels")]
		public float maxAngle = 30f;
		[Tooltip("Maximum torque applied to the driving wheels")]
		public float maxTorque = 300f;
		[Tooltip("Maximum brake torque applied to the driving wheels")]
		public float handBrakeTorque = 30000f;

		[Header("Controller")]
		[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s). Not editable in Play mode")]
		public float criticalSpeed = 5f;
		[Tooltip("The vehicle's limit speed  (in m/s).")]
		public float maxSpeed = 30;
		[Tooltip("The vehicle's acceleration multiplier.")]
		public float acceleration = 5.0f;
		[Tooltip("The vehicle's brake acceleration multiplier. Higher values will make the car stop faster.")]
		public float brakeAcceleration = 15.0f;
		[Tooltip("Simulation sub-steps when the speed is below critical. Not editable in Play mode.")]
		public int stepsBelow = 1;
		[Tooltip("Simulation sub-steps when the speed is above critical. Not editable in Play mode")]
		public int stepsAbove = 5;
		[Tooltip("The vehicle's jump force multiplier.")]
		public int jumpForce = 10000;
		[Tooltip("The vehicle's jump timer interval.")]
		public float jumpInterval = 2.0f;

		[Tooltip("Constant force towards the ground to keep the vehicle riding on it.")]
		public bool snapToGround = true;
		[Tooltip("The amount of snapToGround force to be applied.")]
		public float snapForce = 1500.0f;

		[Tooltip("Speed at which the vehicle rotates in x and y axis.")]
		public Vector3 rotationSpeed = new Vector3(0, 40, 0);
		[Tooltip("The linear drag coefficient override when object is flying. 0 means no damping.")]
		public float flyingLinearDrag = 0.0f;

		// --- Main components ---
		private PhotonView photonView;
		private Rigidbody rb;
		private Wheel[] m_Wheels;
		public Transform centerOfMass;
		public Camera camera;
		Transform mTransform;

		// --- Internal variables ---
		bool lockDown = false;
		float linearDragBackup = 0.0f;

		// Uncomment this to profile 
		//System.Diagnostics.Stopwatch watch;

		struct Wheel
		{
			public WheelCollider collider;
			public GameObject mesh;
			public Transform mTransform;
		}

		// --- Private gameplay variables ---
		private float jumpTimer = 0.0f;


		// --------------------- Main Methods -------------------------

		public void Awake()
		{
			mTransform = transform;
			photonView = GetComponent<PhotonView>();
			rb = GetComponent<Rigidbody>();

			// Uncomment this to profile 
			//watch = new System.Diagnostics.Stopwatch();
		}

		// Find all the WheelColliders down in the hierarchy.
		void Start()
		{
			//Play engine Sound
			engine_sound.Post(gameObject);

			// --- Deactivate camera if this is not the local player ---
			if (camera != null)
			{
				if (!photonView.IsMine && Photon.Pun.PhotonNetwork.IsConnectedAndReady)
					camera.enabled = false;
			}
			else
				Debug.LogError("There is no valid camera in WheelDrive");

			m_Wheels = new Wheel[wheelCount];
			WheelCollider[] cWheels = GetComponentsInChildren<WheelCollider>();

			for (int i = 0; i < cWheels.Length; ++i)
			{
				m_Wheels[i].collider = cWheels[i];

				if (m_Wheels[i].collider)
				{
					MeshRenderer mr = m_Wheels[i].collider.gameObject.GetComponentInChildren<MeshRenderer>();

					if (mr)
					{
						m_Wheels[i].mesh = mr.gameObject;
						m_Wheels[i].mTransform = m_Wheels[i].mesh.transform;
					}
					else
						Debug.LogError("No wheel mesh found in wheel collider's subtree");
				}
				else
					Debug.LogError("No wheel collider found in object's subtree");
			}

			// --- Set rigidbody's center of mass ---
			if (rb != null && centerOfMass != null)
			{
				rb.centerOfMass = centerOfMass.localPosition;
			}
			else
			{
				Debug.LogError("Rigidbody is null or center of mass object does not exist");
			}

			// --- Initialize jump timer to given interval ---
			jumpTimer = jumpInterval;

			// --- Configure wheel physics simulation steps depending on critical speed ---
			m_Wheels[0].collider.ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

			// --- Save rigidbody's drag ---
			linearDragBackup = rb.drag;
		}

		void Update()
		{
			// Uncomment this and timer log at the end of this function to profile 
			//watch.Start();

			// --- Only update if this is the local player ---
			if (!photonView.IsMine && Photon.Pun.PhotonNetwork.IsConnectedAndReady)
				return;

			// --- Obtain input ---
			Vector2 inputDirection;
			inputDirection.x = Input.GetAxis("Horizontal");
			inputDirection.y = Input.GetAxis("Vertical");

			// --- If no acceleration, release lock ---
			float inputRawY = Input.GetAxisRaw("Vertical");

			float angle = maxAngle * inputDirection.x;
			float torque = maxTorque * inputDirection.y;
			Vector3 direction = mTransform.forward * acceleration * inputDirection.y;

			if (inputRawY <= 0)
			{
				lockDown = false;

				// --- Apply manual brake, we want to go backwards ---
				if (inputRawY != 0 && mTransform.InverseTransformDirection(rb.velocity).z > 0)
				{
					rb.AddForce(mTransform.forward * brakeAcceleration * inputDirection.y, ForceMode.Acceleration);
				}
			}
            else
            {
                // --- Apply manual brake, we want to go forward ---
                if (mTransform.InverseTransformDirection(rb.velocity).z < 0)
                {
                    rb.AddForce(mTransform.forward * brakeAcceleration * inputDirection.y, ForceMode.Acceleration);
                }
            }

            // --- Limit car speed ---
            if (rb.velocity.magnitude >= maxSpeed)
                torque = 0;
            else if (rb.velocity.magnitude < maxSpeed && IsGrounded() && direction != Vector3.zero)
				rb.AddForce(direction, ForceMode.Acceleration);

			// ---Car jump-- -
			if (IsGrounded() && jumpTimer >= jumpInterval && Input.GetButtonDown("Jump"))
            {
				// --- If car jumps and has a forward acceleration, prevent it from rotating downwards ---
				if (inputDirection.y > 0)
					lockDown = true;

                rb.AddForce(mTransform.up * jumpForce, ForceMode.Impulse);
                jumpTimer = 0.0f;
            }
			// --- Car jump timer ---
			jumpTimer += Time.deltaTime;

			// --- Keep carattached to ground surface ---
			if (snapToGround)
				rb.AddForce(-mTransform.up * snapForce, ForceMode.Force);

			// --- Car on air rotation ---
			if (!IsGrounded())
			{
				rb.drag = flyingLinearDrag;

				float temp = inputDirection.y;
				inputDirection.y = inputDirection.x;
				inputDirection.x = temp;

				// --- If lock was activated and we are moving forward block down rotation ---
				if (lockDown && inputDirection.x > 0)
					inputDirection.x = 0;

				Quaternion deltaRot = Quaternion.Euler(inputDirection * rotationSpeed);
				mTransform.rotation = Quaternion.Slerp(mTransform.rotation, mTransform.rotation * deltaRot, Time.deltaTime * 2.0f);
			}
			else
				rb.drag = linearDragBackup;
			 
			// --- Car flip ---
			if (mTransform.up.y < -0.75)
            {
				rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, 180));
            }

			float handBrake = Input.GetButton("R2") ? handBrakeTorque : 0;

			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				ref WheelCollider wheel = ref m_Wheels[i].collider;

				// --- A simple car where front wheels steer while rear ones drive ---
				if (m_Wheels[i].mesh.name == "Wheel1Mesh"
						|| m_Wheels[i].mesh.name == "Wheel2Mesh")
					wheel.steerAngle = angle;

				if (m_Wheels[i].mesh.name == "Wheel3Mesh"
						|| m_Wheels[i].mesh.name == "Wheel4Mesh")
					wheel.brakeTorque = handBrake;
				
				if (m_Wheels[i].mesh.name == "Wheel1Mesh"
						|| m_Wheels[i].mesh.name == "Wheel2Mesh"
						&& driveType != DriveType.FrontWheelDrive)
					wheel.motorTorque = torque;
					
				if (m_Wheels[i].mesh.name == "Wheel1Mesh"
						|| m_Wheels[i].mesh.name == "Wheel2Mesh"
						&& driveType != DriveType.RearWheelDrive)
					wheel.motorTorque = torque;
			}
			 
			// Uncomment this and timer start to profile 
			//Debug.Log(watch.Elapsed.TotalMilliseconds);
			//watch.Reset();

			//Update the RTPC for the engine sound
			float speed = rb.velocity.magnitude / maxSpeed;
			AkSoundEngine.SetRTPCValue("Speed",speed*100);
		}

        private void LateUpdate()
        {
			Quaternion q;
			Vector3 p;

			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				ref WheelCollider wheel = ref m_Wheels[i].collider;

				// --- Update visual wheel's mesh --- 
				if (m_Wheels[i].mesh)
				{
					wheel.GetWorldPose(out p, out q);

					// --- Rotate wheel according to collider's rotation ---
					//Transform shapeTransform = m_Wheels[i].mesh.transform;

					if (m_Wheels[i].mesh.name == "Wheel1Mesh"
					 || m_Wheels[i].mesh.name == "Wheel3Mesh")
						m_Wheels[i].mTransform.position = p + transform.right * m_Wheels[i].collider.radius / 2.0f;
					else
						m_Wheels[i].mTransform.position = p - transform.right * m_Wheels[i].collider.radius / 2.0f;


					if (m_Wheels[i].mesh.name == "Wheel1Mesh"
                        || m_Wheels[i].mesh.name == "Wheel2Mesh")
                    {
						m_Wheels[i].mTransform.rotation = q*Quaternion.Euler(0,180, 0);
					}
					else
                    {
						m_Wheels[i].mTransform.rotation = q;
					}
				}
			}


		}

		// ----------------------------------------------

		// --------------------- Utilities -------------------------

		private bool IsGrounded()
		{
			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				if (!m_Wheels[i].collider.isGrounded)
					return false;
			}

			return true;
		}

		//If there's a collision
        private void OnCollisionEnter(Collision collision)
        {
            if(collision.collider.tag == "Player")
            {
				float val = rb.velocity.magnitude + collision.collider.attachedRigidbody.velocity.magnitude;
				val /= maxSpeed * 2;

				if (val >= 1.0f)
					val = 1.0f;
				else if (val < 0.0f)
					val = 0.0f;

				AkSoundEngine.SetRTPCValue("Crash_Energy", val * 100);
				crash_sound.Post(gameObject);
			}
        }

        // ----------------------------------------------
    }
}
