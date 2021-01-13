using UnityEngine;
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
		public float brakeTorque = 30000f;

		[Header("Controller")]
		[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
		public float criticalSpeed = 5f;
		[Tooltip("The vehicle's limit speed  (in m/s).")]
		public float maxSpeed = 30;
		[Tooltip("The vehicle's acceleration multiplier.")]
		public float acceleration = 5.0f;
		[Tooltip("Simulation sub-steps when the speed is below critical.")]
		public int stepsBelow = 1;
		[Tooltip("Simulation sub-steps when the speed is above critical.")]
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

		// --- Main components ---
		private PhotonView photonView;
		private Rigidbody rb;
		private Wheel[] m_Wheels;
		public Transform centerOfMass;
		public Camera camera;
		Transform mTransform;

		// --- Internal variables ---
		bool lockDown = false;
		int currentSteps = 1;

		System.Diagnostics.Stopwatch watch;

		struct Wheel
		{
			public WheelCollider collider;
			public GameObject mesh;
		}

		// --- Private gameplay variables ---
		private float jumpTimer = 0.0f;


		// --------------------- Main Methods -------------------------

		public void Awake()
		{
			mTransform = GetComponent<Transform>();
			photonView = GetComponent<PhotonView>();
			rb = GetComponent<Rigidbody>();
			watch = new System.Diagnostics.Stopwatch();
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
						m_Wheels[i].mesh = mr.gameObject;
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
		}

		void Update()
		{
			watch.Start();

			// --- Only update if this is the local player ---
			if (!photonView.IsMine && Photon.Pun.PhotonNetwork.IsConnectedAndReady)
				return;

            // --- If speed is above critical increase simulation steps ---
            //if (rb.velocity.magnitude > criticalSpeed)
            //{
                m_Wheels[0].collider.ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);
            //}

            // --- Obtain input ---
            Vector2 inputDirection;
			inputDirection.x = Input.GetAxis("Horizontal");
			inputDirection.y = Input.GetAxis("Vertical");

			float angle = maxAngle * inputDirection.x;
			float torque = maxTorque * inputDirection.y;
			Vector3 direction = mTransform.forward * acceleration * inputDirection.y;

			// --- Limit car speed ---
			if (rb.velocity.magnitude > maxSpeed)
			{
				torque = 0;
			}
			else if (IsGrounded())
				rb.AddForce(direction, ForceMode.Acceleration);

			// --- If no acceleration, release lock ---
			if (Input.GetAxisRaw("Vertical") <= 0)
				lockDown = false;

			// --- Car on air rotation ---
			if (!IsGrounded())
			{
				Vector2 inputDirectionRot;
				inputDirectionRot.y = inputDirection.x;
				inputDirectionRot.x = inputDirection.y;

				// --- If lock was activated and we are moving forward block down rotation ---
				if (lockDown && inputDirectionRot.x > 0)
					inputDirectionRot.x = 0;

				Quaternion deltaRot = Quaternion.Euler(inputDirectionRot * rotationSpeed);
				mTransform.rotation = Quaternion.Slerp(mTransform.rotation, mTransform.rotation * deltaRot, Time.deltaTime * 2.0f);
			}

			// ---Car jump-- -
			if (IsGrounded() && jumpTimer >= jumpInterval && Input.GetButtonDown("Jump"))
            {
				// --- If car jumps and has a forward acceleration, prevent it from rotating downwards ---
				if (inputDirection.y > 0)
					lockDown = true;

                rb.AddForce(mTransform.up * jumpForce, ForceMode.Impulse);
                jumpTimer = 0.0f;
            }

			// --- Keep carattached to ground surface ---
			if(snapToGround)
				rb.AddForce(-mTransform.up * snapForce, ForceMode.Force);

			// --- Car flip ---
			if (mTransform.up.y < -0.75)
            {
				rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, 180));
            }

            // --- Car jump timer ---
            jumpTimer += Time.deltaTime;

			float handBrake = Input.GetButton("R2") ? brakeTorque : 0;

			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				ref WheelCollider wheel = ref m_Wheels[i].collider;

				// --- A simple car where front wheels steer while rear ones drive ---
				if (m_Wheels[i].mesh.name == "Wheel1Mesh"
						|| m_Wheels[i].mesh.name == "Wheel2Mesh")
					wheel.steerAngle = angle;

				if (m_Wheels[i].mesh.name == "Wheel3Mesh"
						|| m_Wheels[i].mesh.name == "Wheel4Mesh")
				{
					wheel.brakeTorque = handBrake;
				}

				if (m_Wheels[i].mesh.name == "Wheel1Mesh"
						|| m_Wheels[i].mesh.name == "Wheel2Mesh"
						&& driveType != DriveType.FrontWheelDrive)
				{
					wheel.motorTorque = torque;
					
				}

				if (m_Wheels[i].mesh.name == "Wheel1Mesh"
						|| m_Wheels[i].mesh.name == "Wheel2Mesh"
						&& driveType != DriveType.RearWheelDrive)
				{
					wheel.motorTorque = torque;
				}

			}

			Debug.Log(watch.Elapsed.TotalMilliseconds);

			watch.Reset();

			//Update the RTPC for the engine sound
			float speed = rb.velocity.magnitude / maxSpeed;
			AkSoundEngine.SetRTPCValue("Speed",speed*100);
		}

        private void LateUpdate()
        {
			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				ref WheelCollider wheel = ref m_Wheels[i].collider;

				// --- Update visual wheel's mesh --- 
				if (m_Wheels[i].mesh)
				{
					Quaternion q;
					Vector3 p;
					wheel.GetWorldPose(out p, out q);

					// --- Rotate wheel according to collider's rotation ---
					Transform shapeTransform = m_Wheels[i].mesh.transform;

					if (m_Wheels[i].mesh.name == "Wheel1Mesh"
					 || m_Wheels[i].mesh.name == "Wheel3Mesh")
						shapeTransform.position = p + transform.right * m_Wheels[i].collider.radius / 2.0f;
					else
						shapeTransform.position = p - transform.right * m_Wheels[i].collider.radius / 2.0f;


					if (m_Wheels[i].mesh.name == "Wheel1Mesh"
                        || m_Wheels[i].mesh.name == "Wheel2Mesh")
                    {						                        
						shapeTransform.rotation = q*Quaternion.Euler(0,180, 0);
					}
					else
                    {
						shapeTransform.rotation = q;
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
