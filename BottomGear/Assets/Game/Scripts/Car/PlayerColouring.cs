using BottomGear;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerColouring : MonoBehaviour
{
    public VisualEffect explosionEffect;
    public List<MeshRenderer> renderers;
    public List<TrailRenderer> trail_renderers;
    public GameManager manager;
    private int preset = 0;
    private PhotonView photonView;
    public RectTransform nameCanvas;
    public TMPro.TextMeshProUGUI playerName;
    public GameObject healthBar;
    private WheelDrive controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<WheelDrive>();
        photonView = GetComponent<PhotonView>();
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if (PhotonNetwork.IsMasterClient)
            GetComponent<PhotonView>().RPC("SetColouring", RpcTarget.AllBuffered, manager.GetPreset());

        playerName.SetText(photonView.Owner.NickName);
        Color color = manager.explosionColors[preset];
        color.a = 1;
        nameCanvas.GetChild(0).GetComponent<Image>().color = color;
        manager.SetPlayerOverviewPanelColor(photonView.Owner, preset);
    }

    private void FixedUpdate()
    {

        if (photonView.Owner.GetScore() > manager.maxScore)
        {
            manager.maxScore = photonView.Owner.GetScore();
            manager.ground.SetColor("_EmissionColor", manager.explosionColors[GetPreset()]);
            controller.pickup_flag.Post(gameObject);
        }
        //else if (photonView.Owner.GetScore() == manager.maxScore)
        //{
        //    manager.ground.SetColor("_EmissionColor", manager.groundDefault);
        //}

        if (photonView.IsMine)
        {
            nameCanvas.rotation = Quaternion.Euler(-transform.rotation.eulerAngles.x, (manager.clientCamera.transform.rotation * Quaternion.Euler(0, 180, 0)).eulerAngles.y, -transform.rotation.eulerAngles.z);
            healthBar.transform.rotation = Quaternion.Euler(-transform.rotation.eulerAngles.x, (manager.clientCamera.transform.rotation * Quaternion.Euler(0, 180, 0)).eulerAngles.y, -transform.rotation.eulerAngles.z);
        }
        else if (manager.clientCamera != null)
        {
            nameCanvas.rotation = Quaternion.Euler(-transform.rotation.eulerAngles.x, (manager.clientCamera.transform.rotation * Quaternion.Euler(0, 180, 0)).eulerAngles.y, -transform.rotation.eulerAngles.z);
            healthBar.transform.rotation = Quaternion.Euler(-transform.rotation.eulerAngles.x, manager.clientCamera.transform.rotation.eulerAngles.y, -transform.rotation.eulerAngles.z);
        }
    }

    [PunRPC]
    void SetColouring(int preset)
    {
        if (manager == null)
            manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        this.preset = preset;
        manager.SetPresets(preset, ref renderers, ref explosionEffect, ref trail_renderers);

        foreach(MeshRenderer renderer in renderers)
        {
            if (preset == 0)
                renderer.gameObject.layer = LayerMask.NameToLayer("CarYellow");
            else if (preset == 1)
                renderer.gameObject.layer = LayerMask.NameToLayer("CarOrange");
            else if (preset == 2)
                renderer.gameObject.layer = LayerMask.NameToLayer("CarYellow2");
            else if (preset == 3)
                renderer.gameObject.layer = LayerMask.NameToLayer("CarOrange2");
            else if (preset == 4)
                renderer.gameObject.layer = LayerMask.NameToLayer("CarGreen");
            else if (preset == 5)
                renderer.gameObject.layer = LayerMask.NameToLayer("CarBlue");
        }

     
        trail_renderers[0].gameObject.layer = LayerMask.NameToLayer("Volumes");
        trail_renderers[1].gameObject.layer = LayerMask.NameToLayer("Volumes");
        trail_renderers[2].gameObject.layer = LayerMask.NameToLayer("Volumes");

        renderers[2].gameObject.layer = LayerMask.NameToLayer("Volumes");
        renderers[3].gameObject.layer = LayerMask.NameToLayer("Volumes");
        renderers[4].gameObject.layer = LayerMask.NameToLayer("Volumes");
        renderers[5].gameObject.layer = LayerMask.NameToLayer("Volumes");
        renderers[6].gameObject.layer = LayerMask.NameToLayer("Volumes");
        renderers[7].gameObject.layer = LayerMask.NameToLayer("Volumes");

        explosionEffect.gameObject.layer = LayerMask.NameToLayer("Volumes");
    }

    public int GetPreset()
    {
        return preset;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
