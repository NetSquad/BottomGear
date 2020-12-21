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
	[Tooltip("The vehicle's wheel count")]
	public int wheelCount = 4;
	[Tooltip("Maximum steering angle of the wheels")]
	public float maxAngle = 30f;
	[Tooltip("Maximum torque applied to the driving wheels")]
	public float maxTorque = 300f;
	[Tooltip("Maximum brake torque applied to the driving wheels")]
	public float brakeTorque = 30000f;
	//[Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
	//public GameObject wheelShape;

	[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
	public float criticalSpeed = 5f;
	[Tooltip("Simulation sub-steps when the speed is above critical.")]
	public int stepsBelow = 5;
	[Tooltip("Simulation sub-steps when the speed is below critical.")]
	public int stepsAbove = 1;

	[Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
	public DriveType driveType;

	private PhotonView photonView;
	//private bool controllable = true;


	private Wheel[] m_Wheels;

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
		if (!photonView.IsMine /*|| !controllable*/)
		{
			return;
		}

		m_Wheels[0].collider.ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

		float angle = maxAngle * Input.GetAxis("Horizontal");
		float torque = maxTorque * Input.GetAxis("Vertical");

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

                if (m_Wheels[i].mesh.name == "Wheel1Mesh" 
					|| m_Wheels[i].mesh.name == "Wheel2Mesh")
                {
                    shapeTransform.rotation = q * Quaternion.Euler(0, 180, 0);
                    //shapeTransform.position = p;
                }
                else
                {
                    //shapeTransform.position = p;
                    shapeTransform.rotation = q;
                }
            }
		}
	}
}
