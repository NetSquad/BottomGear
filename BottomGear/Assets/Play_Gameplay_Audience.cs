using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play_Gameplay_Audience : MonoBehaviour
{
    public AK.Wwise.Event Ambience_Sound;
    public AK.Wwise.Event Stop_All;
    // Start is called before the first frame update
    void Start()
    {
        Ambience_Sound.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Stop_All.Post(gameObject);
    }
}
