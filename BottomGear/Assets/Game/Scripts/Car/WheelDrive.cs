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
		[Tooltip("Simulation sub-steps when the speed is above critical.")]
		public int stepsBelow = 5;
		[Tooltip("Simulation sub-steps when the speed is below critical.")]
		public int stepsAbove = 1;
		[Tooltip("The vehicle's jump force multiplier.")]
		public int jumpForce = 10000;
		[Tooltip("The vehicle's jump timer interval.")]
		public float jumpInterval = 2.0f;

		// --- Main components ---
		private PhotonView photonView;
		private Rigidbody rb;
		private Wheel[] m_Wheels;
		public Transform centerOfMass;
		public Camera camera;

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
			photonView = GetComponent<PhotonView>();
			rb = GetComponent<Rigidbody>();
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
			// --- Only update if this is the local player ---
			if (!photonView.IsMine && Photon.Pun.PhotonNetwork.IsConnectedAndReady)
			{
				return;
			}

			m_Wheels[0].collider.ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

			float angle = maxAngle * Input.GetAxis("Horizontal");
			float torque = maxTorque * Input.GetAxis("Vertical");

			// --- Limit car speed ---
			if (rb.velocity.magnitude > maxSpeed)
			{
				torque = 0;
			}
			else if(IsGrounded())
				rb.AddForce(transform.forward * acceleration * Input.GetAxis("Vertical"), ForceMode.Acceleration);

            // ---Car jump-- -

            if (IsGrounded() && jumpTimer >= jumpInterval && Input.GetButtonDown("Jump"))
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                jumpTimer = 0.0f;
            }



            // --- Car flip ---
            if (/*Physics.Raycast(centerOfMass.position,transform.up, 3) && */!IsGrounded() && Math.Abs(transform.eulerAngles.z) > 150 && Math.Abs(transform.eulerAngles.z) < 190)
            {
				rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, 180));

                //if (Math.Abs(transform.eulerAngles.z) > 150 && Math.Abs(transform.eulerAngles.z) < 190)
                //    rb.MovePosition(rb.position - transform.up);
            }

            // --- Car jump timer ---
            jumpTimer += Time.deltaTime;

			float handBrake = Input.GetKey(KeyCode.X) ? brakeTorque : 0;

			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				ref WheelCollider wheel = ref m_Wheels[i].collider;

				// A simple car where front wheels steer while rear ones drive.
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
            if(collision.collider.tag == "Car")
            {
				int test = 0;
            }
        }

        // ----------------------------------------------
    }
}
