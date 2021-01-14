using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyConstraint : MonoBehaviour
{
    Transform mTransform;
    private Quaternion m_Rotation = Quaternion.identity;

    bool start = false;

    // Start is called before the first frame update
    void Start()
    {
        mTransform = transform;
        m_Rotation = mTransform.rotation * Quaternion.Euler(0,10,0);
    }

    // Update is called once per frame
    void Update()
    {
        mTransform.rotation = m_Rotation;
    }

    private void LateUpdate()
    {
        mTransform.rotation = m_Rotation;

        if(start)
            mTransform.rotation = m_Rotation * Quaternion.Euler(0, -10, 0);

        start = true;
    }
}
