using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play_Tonight_Bottom : MonoBehaviour
{
    public AK.Wwise.Event Tonight;
    public uint time = 3;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TimerCoroutine(time));
    }
    IEnumerator TimerCoroutine(uint time)
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(time);

        //Play Audio
        Tonight.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
