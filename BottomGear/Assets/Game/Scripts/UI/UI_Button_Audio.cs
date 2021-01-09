using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Button_Audio : MonoBehaviour
{
    // Start is called before the first frame update
    public AK.Wwise.Event button_hover;
    public AK.Wwise.Event button_clicked;

    private void Awake()
    {
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayButtonHover()
    {
        button_hover.Post(gameObject);
    }

    public void PlayButtonClicked()
    {
        button_clicked.Post(gameObject);
    }
}
