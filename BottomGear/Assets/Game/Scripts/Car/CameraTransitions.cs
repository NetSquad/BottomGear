using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitions : MonoBehaviour
{
    public Cinemachine.CinemachineVirtualCamera FollowCamera;
    public Cinemachine.CinemachineVirtualCamera FollowCameraFar;
    public Cinemachine.CinemachineVirtualCamera FollowCameraUp;
    public Cinemachine.CinemachineVirtualCamera FrontCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("LookBack"))
        {

        }
    }
}
