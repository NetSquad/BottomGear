using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.Log("BOOOOOOOOOOOOOOOOOOSTO");
        }

        if (boostDuration <= 0)
        {
            beginBoost = false;
            boostDuration = 3.0f;
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
        {
            Debug.Log("Detected RigidBody");
            beginBoost = true;
            fromFront = false;
        }
    }

    // Check where is the player coming from
    public void IsFront()
    {
        fromFront = true;
    }

}
