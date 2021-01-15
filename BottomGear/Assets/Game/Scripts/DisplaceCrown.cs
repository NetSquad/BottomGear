using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplaceCrown : MonoBehaviour
{
    public float rotateSpeed = 1.0f;
    public float verticalSpeed = 1.0f;
    public float maxVerticalOscillation = 0.5f;

    private float sinusCounter = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sinusCounter += verticalSpeed * Time.deltaTime;

        if (Mathf.PI * 2 < sinusCounter)
            sinusCounter -= 2 * Mathf.PI;

        transform.localPosition = new Vector3(0, Mathf.Sin(sinusCounter) * maxVerticalOscillation, 0);
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.Self);
    }
}
