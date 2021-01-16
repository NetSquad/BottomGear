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

    public float FollowCamInitialWeight = 10.0f;
    public float FrontCamInitialWeight = 10.0f;
    public float FollowFarCamInitialWeight = 10.0f;
    public float FollowUpCamInitialWeight = 10.0f;

    private GameObject CurrentCamera;
    private Cinemachine.CinemachineBrain brain;
    private Cinemachine.CinemachineMixingCamera mixingCamera;
    private GameObject Parent;

    // Start is called before the first frame update
    void Start()
    {
        Parent = transform.parent.gameObject;
        CurrentCamera = FollowCamera;
        brain = MainCamera.GetComponent<Cinemachine.CinemachineBrain>();
        mixingCamera = GetComponent<Cinemachine.CinemachineMixingCamera>();
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        if (brain.ActiveBlend != null && !brain.ActiveBlend.IsComplete)
            return;

        float distanceToTarget = Mathf.Abs(Vector3.Distance(Parent.transform.position, CurrentCamera.transform.position));

        mixingCamera.m_Weight2 = Mathf.Clamp(FollowFarCamInitialWeight - distanceToTarget, 0, FollowFarCamInitialWeight);
        mixingCamera.m_Weight3 = Mathf.Clamp(Mathf.Abs(Parent.transform.rotation.eulerAngles.GetXCorrectedEuler().x / 90 * FollowUpCamInitialWeight), 0, FollowUpCamInitialWeight);

        // --- Follow - Rear ---
        if (Input.GetButton("Rear") || Input.GetMouseButton(2))
        {
            mixingCamera.m_Weight0 = 0;
            mixingCamera.m_Weight2 = 0;
            mixingCamera.m_Weight3 = 0;

            if (CurrentCamera != FrontCamera)
            {
                mixingCamera.m_Weight1 = FrontCamInitialWeight;
                CurrentCamera = FrontCamera;
            }
        }
        else if (CurrentCamera == FrontCamera)
        {
            mixingCamera.m_Weight1 = 0;
            mixingCamera.m_Weight0 = FollowCamInitialWeight;
            CurrentCamera = FollowCamera;
        }
    }
}
