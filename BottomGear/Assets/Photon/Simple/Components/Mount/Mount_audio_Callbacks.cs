using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mount_audio_Callbacks : MonoBehaviour
{

    public AK.Wwise.Event pickup_flag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playPickUp()
    {
        pickup_flag.Post(gameObject);
    }
}
