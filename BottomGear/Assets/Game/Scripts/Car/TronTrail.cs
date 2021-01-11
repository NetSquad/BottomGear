using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

public class TronTrail : MonoBehaviour
{
    float lastSpawn = 0.0f;
    public float timeBetweenSpawns = 1.0f;
    public float lifespan = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastSpawn > timeBetweenSpawns)
        {
            Vector3 worldTrailPos = transform.TransformPoint(new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 3.5f));
            PhotonNetwork.Instantiate("TronTrailSection", worldTrailPos, transform.rotation);      // avoid this call on rejoin (ship was network instantiated before)
            lastSpawn = Time.time;
        }
    }
}
