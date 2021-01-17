using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BottomGear;

public class BoostingPad : MonoBehaviour
{
    [Header("Boost properties")]
    [Tooltip("The boost's duration")]
    public float boostDuration = 3.0f;
    [Tooltip("The boost's force")]
    public float boostAmount = 100.0f;

    private bool beginBoost = false;
    private bool fromFront = false;

    // RigidBody entering the Trigger
    private Rigidbody rb;

    public Vector3 carRelative;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (beginBoost && boostDuration > 0)
        {
            rb.AddForce(rb.transform.forward * boostAmount, ForceMode.Acceleration);

            boostDuration -= Time.deltaTime;
        }

        if (boostDuration <= 0)
        {
            boostDuration = 3.0f;
            beginBoost = false;
            fromFront = false;
        }
        //Speed=rigidbody.velocity.magnitude*3.6; 
    }

    // Collision callbacks
    private void OnTriggerEnter(Collider other)
    {
        rb = other.GetComponent<Rigidbody>();

        // Makes sure that the go is a player and he is coming from the right direction
        // If he comes from the wrong direction, boosting will not be applied to said go
        if(other.transform.root.gameObject.CompareTag("Player") && fromFront)
            beginBoost = true;
    }

    // Check where is the player coming from
    public bool IsFront(Transform playerTrans)
    {
        // Transform car position into Booster's local space
        carRelative = transform.InverseTransformPoint(playerTrans.position);
       
        if (carRelative.x > 0)
        {
            fromFront = true;
            return true;
        }
        else
        {
            fromFront = false;
        }
        return false;
    }

}
