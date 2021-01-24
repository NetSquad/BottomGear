using BottomGear;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerColouring : MonoBehaviour
{
    public VisualEffect explosionEffect;
    public List<MeshRenderer> renderers;
    public List<TrailRenderer> trail_renderers;
    public GameManager manager;
    private int preset = 0;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        preset = manager.GetPreset()*5;
        manager.SetPresets(preset, ref renderers, explosionEffect, ref trail_renderers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
