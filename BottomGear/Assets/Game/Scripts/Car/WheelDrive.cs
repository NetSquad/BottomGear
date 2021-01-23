using UnityEngine;
using System;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections.Generic;
using Photon.Pun.Simple;

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
	[RequireComponent(typeof(WheelRefs))]

	public class WheelDrive : MonoBehaviour
    {
		// --------------------- Variables -------------------------
		[Header("Audio")]
		public AK.Wwise.Event engine_sound;
		public AK.Wwise.Event crash_sound;
		public AK.Wwise.Event enemy_destroyed;
		public AK.Wwise.Event play_boost_loop;
		public AK.Wwise.Event stop_boost_loop;
		public AK.Wwise.Event jump;
		public AK.Wwise.Event play_neon_loop;
		public AK.Wwise.Event stop_neon_loop;
		public AK.Wwise.Event pickup_flag;
		public AK.Wwise.Event tyre_hit;
		public AK.Wwise.Event explosion;


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
		[Tooltip("Amount of position to correct using WheelReferences. Higher value will keep wheels more attached to the reference while lower to the WheelCollider.")]
		[Range(0.0f, 1.0f)]
		public float wheelRefsWeight = 0.75f;

		[Header("Controller")]
		[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s). Not editable in Play mode")]
		public float criticalSpeed = 5f;
		[Tooltip("The vehicle's limit speed  (in m/s).")]
		public int maxSpeed = 30;
		[Tooltip("The vehicle's limit speed while boosting (in m/s).")]
		public int maxBoostingSpeed = 60;
		[Tooltip("The vehicle's acceleration multiplier.")]
		public int maxTurboSpeed = 45;
		[Tooltip("The vehicle's acceleration multiplier.")]
		public float acceleration = 5.0f;
		[Tooltip("The vehicle's boosting acceleration multiplier.")]
		public float boostAcceleration = 2.0f;
		[Tooltip("The vehicle's turbo acceleration multiplier.")]
		public float turboAcceleration = 3.0f;
		[Tooltip("Simulation sub-steps when the speed is below critical. Not editable in Play mode.")]
		public int stepsBelow = 1;
		[Tooltip("Simulation sub-steps when the speed is above critical. Not editable in Play mode")]
		public int stepsAbove = 5;
		[Tooltip("Speed at which the vehicle rotates in x and y axis.")]
		public Vector3 rotationSpeed = new Vector3(30, 30, 30);
		[Tooltip("Rate at which fuel is consumed")]
		public float consumptionRate = 20.0f;

		[Header("Forces")]
		[Tooltip("The vehicle's jump force multiplier.")]
		public int jumpForce = 10000;
		[Tooltip("The vehicle's jump timer interval.")]
		public float jumpInterval = 2.0f;
		[Tooltip("Constant force towards the ground to keep the vehicle riding on it.")]
		public bool snapToGround = true;
		[Tooltip("The amount of snapToGround force to be applied.")]
		public float snapForce = 1500.0f;
		[Tooltip("The linear drag coefficient override when object is flying. 0 means no damping.")]
		public float flyingLinearDrag = 0.0f;
		[Tooltip("Constant force towards the world -up to simulate a higher gravity without touching the global parameter.")]
		public float flyingFakeGravity = 9.81f;

		//[Header("TestVelocity")]
		//[Tooltip("Debug velocity")]
		public float velocity = 0;
		//public double energy = 0.0f;

		// --- Main components ---
		private PhotonView photonView;
		private Rigidbody rb;
		private Wheel[] m_Wheels;
		public Transform centerOfMass;
		public Camera camera;
		Transform mTransform;

		// --- Network ---
        Photon.Pun.Simple.SyncVitals vitals;
        Photon.Pun.Simple.BasicInventory basicInventory;
		float initialScoreTime = 0.0f;

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
			public Transform refTransform;
			public bool wasGrounded;
		}

		// --- Public gameplay variables
		public bool isBoosting = false;
		// --- Private gameplay variables ---
		private float jumpTimer = 0.0f;
		private bool isTurbo = false;

        // --------------------- Main Methods -------------------------

        public void Awake()
		{
			basicInventory = GetComponent<Photon.Pun.Simple.BasicInventory>();
			vitals = GetComponent<Photon.Pun.Simple.SyncVitals>();
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
					camera.gameObject.SetActive(false);
			}
			else
				Debug.LogError("There is no valid camera in WheelDrive");

			m_Wheels = new Wheel[wheelCount];
			WheelCollider[] cWheels = GetComponentsInChildren<WheelCollider>();

			WheelRefs wheelRefs = GetComponent<WheelRefs>();

			for (int i = 0; i < cWheels.Length; ++i)
			{
				m_Wheels[i].collider = cWheels[i];

				if (m_Wheels[i].collider)
				{
					MeshRenderer mr = m_Wheels[i].collider.gameObject.GetComponentInChildren<MeshRenderer>();

					if (mr && wheelRefs.wheelRefs[i])
					{
						m_Wheels[i].refTransform = wheelRefs.wheelRefs[i].transform;
						m_Wheels[i].mesh = mr.gameObject;
						m_Wheels[i].mTransform = m_Wheels[i].mesh.transform;
					}
					else
						Debug.LogError("No wheel mesh found in wheel collider's subtree or no wheel reference");
				}
				else
					Debug.LogError("No wheel collider found in object's subtree");

				m_Wheels[i].wasGrounded = false;
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
			// --- Only update if this is the local player ---
			if (!photonView.IsMine && Photon.Pun.PhotonNetwork.IsConnectedAndReady)
				return;
			// Used to debug car speed
			velocity = rb.velocity.magnitude;

			if (Time.time - initialScoreTime >= 1.0f &&
				photonView.IsMine
				&& basicInventory.DefaultMount.mountedObjs.Count > 0)
			{
				initialScoreTime = Time.time;
				PhotonNetwork.LocalPlayer.AddScore(1);
			}

			initialScoreTime += Time.deltaTime;

			// Uncomment this and timer start to profile
			//Debug.Log(watch.Elapsed.TotalMilliseconds);
			//watch.Reset();
		}

		private void FixedUpdate()
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

			// --- Support both mouse/keyboard and gamepad ---
			float accelerator = (Input.GetAxis("R2") - (-1)) / (2);

			if (Input.GetKey(KeyCode.W))
				accelerator = 1.0f;
			else if ((Input.GetAxis("R2") == 0))
				accelerator = 0.0f;

			float decelerator = (Input.GetAxis("L2") - (-1)) / (2);

			if (Input.GetKey(KeyCode.S))
				decelerator = 1.0f;
			else if ((Input.GetAxis("L2") == 0))
				decelerator = 0.0f;

			if (Input.GetKey(KeyCode.LeftShift) && vitals.vitals.VitalArray[1].Value > 0 && IsGrounded())
            {
				vitals.vitals.VitalArray[1].Value -= Time.fixedDeltaTime * consumptionRate;
				isTurbo = true;
			}
			else
				isTurbo = false;


			//energy = vitals.vitals.VitalArray[1].Value;

			float outputAcceleration = accelerator - decelerator;
			float angle = maxAngle * inputDirection.x;
			float torque = outputAcceleration * maxTorque * Time.fixedDeltaTime;

			// --- Deactivate rotation lock if new player input is detected ---
			if (inputRawY <= 0)
				lockDown = false;

			// --- Limit car speed ---
			if (isBoosting)
			{
				//Vector3 direction = mTransform.forward * acceleration * boostAcceleration * outputAcceleration * Time.fixedDeltaTime;

				//if (rb.velocity.magnitude >= Mathf.Abs(60 * outputAcceleration))
				//	torque = 0;
				//else if (rb.velocity.magnitude < 60 && IsGrounded() && direction != Vector3.zero)
				//	rb.AddForce(direction, ForceMode.Acceleration);

				LimitSpeed(outputAcceleration, 60, torque, boostAcceleration);
			}
			else if (isTurbo)
			{
				//Vector3 direction = mTransform.forward * acceleration * turboAcceleration * outputAcceleration * Time.fixedDeltaTime;

				//if (rb.velocity.magnitude >= Mathf.Abs(45 * outputAcceleration))
				//	torque = 0;
				//else if (rb.velocity.magnitude < 45 && IsGrounded() && direction != Vector3.zero)
				//	rb.AddForce(direction, ForceMode.Acceleration);

				LimitSpeed(outputAcceleration, 45, torque, turboAcceleration);
			}
			else
            {
				//Vector3 direction = mTransform.forward * acceleration * outputAcceleration * Time.fixedDeltaTime;

				//if (rb.velocity.magnitude >= Mathf.Abs(30 * outputAcceleration))
				//	torque = 0;
				//else if (rb.velocity.magnitude < 30 && IsGrounded() && direction != Vector3.zero)
				//	rb.AddForce(direction, ForceMode.Acceleration);

				LimitSpeed(outputAcceleration, 30, torque);
			}


            // ---Car jump-- -
            if (IsGrounded() && jumpTimer >= jumpInterval && Input.GetButtonDown("Jump"))
			{
				// --- If car jumps and has a forward acceleration, prevent it from rotating downwards ---
				if (accelerator > 0)
					lockDown = true;

				rb.AddForce(mTransform.up * jumpForce, ForceMode.Impulse);

				jumpTimer = 0.0f;
				jump.Post(gameObject);
			}
			// --- Car jump timer ---
			jumpTimer += Time.fixedDeltaTime;

			// --- Keep car attached to ground surface ---
			if (snapToGround)
				rb.AddForce(-mTransform.up * snapForce, ForceMode.Force);

			// --- Car on air rotation ---
			if (!IsGrounded())
			{
				// --- Fake gravity towards world -up ---
				rb.AddForce(-Vector3.up * flyingFakeGravity, ForceMode.Acceleration);

				// --- Override rigidbody's drag ---
				rb.drag = flyingLinearDrag;

				float temp = inputDirection.y;
				inputDirection.y = inputDirection.x;
				inputDirection.x = temp;

				// --- If lock was activated and we are moving forward block down rotation ---
				if (lockDown && inputDirection.x > 0)
					inputDirection.x = 0;

				Vector3 orientationX = camera.transform.up;
				Vector3 orientationY = camera.transform.right;
				Vector3 orientationZ = camera.transform.forward;

				if (rb.angularVelocity.magnitude < Mathf.Abs(30))
				{
					// --- Rotate X and Y separately ---
					Vector3 orientation = orientationX;
					orientation.Scale(rotationSpeed * Time.fixedDeltaTime);

					rb.AddTorque(orientation * inputDirection.y, ForceMode.Acceleration);

					orientation = orientationY;
					orientation.Scale(rotationSpeed * Time.fixedDeltaTime);

					rb.AddTorque(orientation * inputDirection.x, ForceMode.Acceleration);

					orientation = orientationZ;
					orientation.Scale(rotationSpeed * Time.fixedDeltaTime);

					rb.AddTorque(orientation * Mathf.Clamp(Input.GetAxis("HorizontalRS"), -1, 1), ForceMode.Acceleration);
				}

            }
			else
				rb.drag = linearDragBackup;

			// --- Car flip ---
			if (mTransform.up.y < -0.6 && !IsGrounded())
			{
				rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, 180));
			}

			// --- Wheel physics ---
			float handBrake = decelerator > 0.5f && mTransform.InverseTransformDirection(rb.velocity).z > 0 ? handBrakeTorque : 0;

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

			//Update the RTPC for the engine sound
			float speed = rb.velocity.magnitude / maxSpeed;
			AkSoundEngine.SetRTPCValue("Speed", speed * 100);

			//Check wheel collisions with ground
			for (int i = 0; i < m_Wheels.Length; ++i)
			{
				if (m_Wheels[i].collider.isGrounded && m_Wheels[i].wasGrounded == false) //we just grounded
                {
					m_Wheels[i].wasGrounded = true;
					tyre_hit.Post(gameObject);
				}
				else if (m_Wheels[i].collider.isGrounded == false && m_Wheels[i].wasGrounded == true)
				{
					m_Wheels[i].wasGrounded = false;
				}
			}
			// Raycast to detect BoostPad and collides only with Boost Pad layer
			int layerMask = 1 << 9;

			RaycastHit hit;

			if (Physics.Raycast(transform.position, transform.forward, out hit, 20f, layerMask))
            {
				BoostingPad bp = hit.collider.gameObject.GetComponent<BoostingPad>();

				// If car is in front, the moment it touches the booster, it will accelerate
				if (bp.IsFront(transform)) // ----> This if statement and the else below will be deleted when no longer needed
					Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green);
			}
			else
				Debug.DrawRay(transform.position, transform.forward, Color.red);
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

					if (m_Wheels[i].mesh.name == "Wheel1Mesh"
					 || m_Wheels[i].mesh.name == "Wheel3Mesh")
						m_Wheels[i].mTransform.position = Vector3.Lerp(p, m_Wheels[i].refTransform.position, wheelRefsWeight);
					else
						m_Wheels[i].mTransform.position = Vector3.Lerp(p, m_Wheels[i].refTransform.position, wheelRefsWeight);


					if (m_Wheels[i].mesh.name == "Wheel1Mesh"
                        || m_Wheels[i].mesh.name == "Wheel2Mesh")
						m_Wheels[i].mTransform.rotation = q*Quaternion.Euler(0,180, 0);
					else
						m_Wheels[i].mTransform.rotation = q;

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

		private void LimitSpeed(float output, int limit, float tq, float extraAcceleration = 1.0f)
    {
			Vector3 direction = mTransform.forward * acceleration * extraAcceleration * output * Time.fixedDeltaTime;

			if (rb.velocity.magnitude >= Mathf.Abs(limit * output))
				tq = 0;
			else if (rb.velocity.magnitude < limit && IsGrounded() && direction != Vector3.zero)
				rb.AddForce(direction, ForceMode.Acceleration);
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

        private void OnTriggerEnter(Collider other)
        {
			if (other.tag == "Bullet")
			{
				ContactProjectile contact = other.gameObject.GetComponent<ContactProjectile>();
				Debug.Log("Collided with bullet");

				Debug.Log(vitals.vitals.VitalArray[0].Value);

				if (contact && vitals.vitals.VitalArray[0].Value - 20 <= 0) // bullet damage
				{
					contact.Owner.PhotonView.Owner.AddScore(10);
					Debug.Log(contact.Owner.PhotonView.Owner.GetScore());
				}
			}
		}

        private void OnParticleCollision(GameObject other)
		{
			// --- Kill car if it is another's trail ---
			if (!photonView.IsMine && !other.GetComponent<ParentRef>().photonView.IsMine)
			{
				vitals.vitals.ApplyCharges(-vitals.vitals.VitalArray[0].Value, false, true);

				Debug.Log("Collided with trail");

				Debug.Log(vitals.vitals.VitalArray[0].Value);

				if (vitals.vitals.VitalArray[0].Value <= 0)
				{
					other.GetComponent<ParentRef>().photonView.Owner.AddScore(15);
					Debug.Log(other.GetComponent<ParentRef>().photonView.Owner.GetScore());
				}
			}
		}

		// ----------------------------------------------
	}
}
