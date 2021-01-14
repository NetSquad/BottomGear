using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitions : MonoBehaviour
{
    public GameObject FollowCamera;
    public GameObject FollowCameraFar;
    public GameObject FollowCameraUp;
    public GameObject FrontCamera;
    public GameObject MainCamera;

    private GameObject CurrentCamera;
    private Cinemachine.CinemachineBrain brain;

    public float minDistance = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        CurrentCamera = FollowCamera;
        FollowCamera.SetActive(true);
        FollowCameraFar.SetActive(false);
        FollowCameraUp.SetActive(false);
        FrontCamera.SetActive(false);
        brain = MainCamera.GetComponent<Cinemachine.CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (brain.ActiveBlend != null && !brain.ActiveBlend.IsComplete)
            return;

        if (Input.GetButton("Rear") || Input.GetMouseButton(2))
        {
            if (!FrontCamera.activeSelf)
            {
                CurrentCamera.SetActive(false);
                CurrentCamera = FrontCamera;
                CurrentCamera.SetActive(true);
            }
        }
        else if (CurrentCamera == FrontCamera)
        {
            CurrentCamera.SetActive(false);
            CurrentCamera = FollowCamera;
            CurrentCamera.SetActive(true);
        }

        //Debug.Log(Mathf.Abs(Vector3.Distance(transform.position, CurrentCamera.transform.position)));

        if (Mathf.Abs(Vector3.Distance(transform.position,CurrentCamera.transform.position)) < minDistance)
        {
            CurrentCamera.SetActive(false);
            CurrentCamera = FollowCameraFar;
            CurrentCamera.SetActive(true);
        }
        else if (CurrentCamera == FollowCameraFar)
        {
            CurrentCamera.SetActive(false);
            CurrentCamera = FollowCamera;
            CurrentCamera.SetActive(true);
        }
    }
}
