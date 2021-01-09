using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play_Music : MonoBehaviour
{
    // Start is called before the first frame update
    public AK.Wwise.Event Music;
    void Start()
    {
        Music.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
