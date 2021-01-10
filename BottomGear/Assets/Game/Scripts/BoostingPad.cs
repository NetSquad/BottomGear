using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostingPad : MonoBehaviour
{
    [Header("Boost properties")]
    [Tooltip("The boost's duration")]
    public float boostDuration = 3.0f;
    [Tooltip("The boost's force")]
    public float boostAmount = 5.0f;

    private bool beginBoost = false;

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
    }

    // Collision callbacks
    private void OnTriggerEnter(Collider other)
    {
        rb = other.GetComponent<Rigidbody>();

        if(rb != null)
        {
            Debug.Log("Detected RigidBody");
            beginBoost = true;
        }
    }
}
