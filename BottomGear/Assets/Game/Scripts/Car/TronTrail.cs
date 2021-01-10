using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TronTrail : MonoBehaviour
{
    uint waitTime = 5;
    uint spawnCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnCounter > 5)
        {

        }
        else
        {
            spawnCounter += Time.deltaTime;
        }
    }
}
