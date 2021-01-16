using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play_Music : MonoBehaviour
{
    // Start is called before the first frame update
    public AK.Wwise.Event Music;
    public AK.Wwise.Event Stop_all;
    void Start()
    {
        Music.Post(gameObject);
    }

    private void OnDestroy()
    {
        Stop_all.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
