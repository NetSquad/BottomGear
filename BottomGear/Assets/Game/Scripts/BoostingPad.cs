using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BottomGear;

public class BoostingPad : MonoBehaviour
{
    [Header("Boost properties")]
    [Tooltip("The boost's duration")]
    public float boostDuration = 3.0f;

    private bool fromFront = false;

    // RigidBody entering the Trigger
    private Rigidbody rb;
    private BottomGear.WheelDrive drive;


    public Vector3 carRelative;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(drive != null)
        {
            if (drive.isBoosting && boostDuration > 0)
            {
                boostDuration -= Time.deltaTime;
            }

            if (boostDuration <= 0)
            {
                boostDuration = 3.0f;
                fromFront = false;

                // When finishing boosting, simply forget about the car assiciated
                drive.isBoosting = false;
                drive = null;
                // Debug.Log("Stop boost");
            }
        }
    }

    // Collision callbacks
    private void OnTriggerEnter(Collider other)
    {
        rb = other.GetComponent<Rigidbody>();

        // Makes sure that the go is a player and he is coming from the right direction
        // If he comes from the wrong direction, boosting will not be applied to said go
        if(other.transform.root.gameObject.CompareTag("Player") && fromFront)
        {
            drive = rb.gameObject.GetComponent<WheelDrive>();
            drive.isBoosting = true;
           // Debug.Log("Boosting");
        }
           
    }

    // Check where is the player coming from
    public bool IsFront(Transform playerTrans)
    {
        // Transform car position into Booster's local space
        carRelative = transform.InverseTransformPoint(playerTrans.position);
       
        if (carRelative.y > 0)
        {
            fromFront = true;
           // Debug.Log("From front");
            return true;
        }
        else
        {
            fromFront = false;
        }
        return false;
    }

}
