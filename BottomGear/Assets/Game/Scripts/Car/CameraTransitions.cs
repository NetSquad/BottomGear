using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitions : MonoBehaviour
{
    public GameObject FollowCamera;
    public GameObject FollowCameraFar;
    public GameObject FollowCameraUp;
    public GameObject FrontCamera;

    private GameObject CurrentCamera;

    // Start is called before the first frame update
    void Start()
    {
        CurrentCamera = FollowCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Rear"))
        {
            CurrentCamera.SetActive(false);
            CurrentCamera = FrontCamera;
            CurrentCamera.SetActive(true);
        }
        else if (CurrentCamera == FrontCamera)
        {
            CurrentCamera.SetActive(false);
            CurrentCamera = FollowCamera;
            CurrentCamera.SetActive(true);
        }
    }
}
