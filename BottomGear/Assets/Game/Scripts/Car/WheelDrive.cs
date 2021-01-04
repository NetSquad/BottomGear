using UnityEngine;
using System;
using Photon.Pun;

[Serializable]
public enum DriveType
{
	RearWheelDrive,
	FrontWheelDrive,
	AllWheelDrive
}

public class WheelDrive : MonoBehaviour
{
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
	[Tooltip("The vehicle's limit speed.")]
	public float maxSpeed = 30;
	[Tooltip("Simulation sub-steps when the speed is above critical.")]
	public int stepsBelow = 5;
	[Tooltip("Simulation sub-steps when the speed is below critical.")]
	public int stepsAbove = 1;

	private PhotonView photonView;

	Rigidbody rigidbody;
	private Wheel[] m_Wheels;
	public Transform centerOfMass;

	struct Wheel
    {
		public WheelCollider collider;
		public GameObject mesh;
    }

	public void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	// Find all the WheelColliders down in the hierarchy.
	void Start()
	{
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


		rigidbody = GetComponent<Rigidbody>();

		if (rigidbody != null && centerOfMass != null)
		{
			rigidbody.centerOfMass = centerOfMass.localPosition;
		}

		//for (int i = 0; i < m_Wheels.Length; ++i) 
		//{
		//	var wheel = m_Wheels[i];

		//	// Create wheel shapes only when needed.
		//	if (wheelShape != null)
		//	{
		//		var ws = Instantiate (wheelShape);
		//		ws.transform.parent = wheel.collider.transform;
		//	}
		//}
	}

	// This is a really simple approach to updating wheels.
	// We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
	// This helps us to figure our which wheels are front ones and which are rear.
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
		if(rigidbody.velocity.magnitude > maxSpeed)
        {
			torque = 0;
        }

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

			// Update visual wheels if any.
			if (m_Wheels[i].mesh) 
			{
				Quaternion q;
				Vector3 p;
				wheel.GetWorldPose (out p, out q);

				// Assume that the only child of the wheelcollider is the wheel shape.
				Transform shapeTransform = m_Wheels[i].mesh.transform;
				Debug.Log(-wheel.rpm / 60 * 360 * Time.deltaTime);

				float rotation = wheel.rpm / 60 * 360 * Time.deltaTime;



				//Mathf.Clamp(rotation, 0.1f, rotation);

				if (m_Wheels[i].mesh.name == "Wheel1Mesh" 
					|| m_Wheels[i].mesh.name == "Wheel2Mesh")
                {
					//shapeTransform.rotation = q*Quaternion.Euler(wheel.rpm / 60 * 360 * Time.deltaTime, 0, 0);


					shapeTransform.Rotate(-rotation, 0, 0);
					shapeTransform.rotation = Quaternion.Euler(shapeTransform.rotation.eulerAngles.x,
						q.eulerAngles.y + 180, q.eulerAngles.z);

					//shapeTransform.rotation = q * Quaternion.Euler(0, 180, 0);
					//shapeTransform.position = p;
				}
				else
                {
					shapeTransform.Rotate(rotation, 0, 0);
					shapeTransform.rotation = Quaternion.Euler(shapeTransform.rotation.eulerAngles.x,
						q.eulerAngles.y, q.eulerAngles.z);
					//shapeTransform.rotation = q*Quaternion.Euler(wheel.rpm / 60 * 360 * Time.deltaTime, 0, 0);
					//shapeTransform.position = p;
					//shapeTransform.rotation = q;
				}
			}
		}
	}
}
