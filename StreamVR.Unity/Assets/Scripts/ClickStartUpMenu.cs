﻿using System;
using System.Collections;
using System.Collections.Generic;
using LMAStudio.StreamVR.Unity.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickStartUpMenu : MonoBehaviour
{
    public GameObject ConnectionMenu;
    public GameObject RoomCodeMenu;
    public TMPro.TMP_InputField urlInput;
    public TMPro.TMP_InputField usernameInput;
    public TMPro.TMP_InputField roomcodeInput;
    public TMPro.TMP_Text urlError;
    public TMPro.TMP_Text usernameError;

    // Start is called before the first frame update
    void Start()
    {
        ShowConnectionMenu ();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowConnectionMenu ()
    {
        RoomCodeMenu.SetActive(false);
        ConnectionMenu.SetActive(true);
    }

    public void URLEnterButtonClick()
    {
        urlError.text = "";

        BusConnector.ConfigureEndpoint(urlInput.text);
        try
        {
            StreamVR.Instance.Connect(new StreamVROptions());
            ShowRoomCodeMenu();
        }

        catch (Exception e)
        {
            urlError.text = e.Message;
        }

    }

    public void UsernameEnterButtonClick()
    {
        usernameError.text = "";

        BusConnector.ConfigureRoom(usernameInput.text, roomcodeInput.text);
        try
        {
            StreamVR.Instance.GetStartingOrientation();
            EnterStreamVR();
        }

        catch (Exception e)
        {
            usernameError.text = e.Message;
        }

    }

    public void ShowRoomCodeMenu ()
    {
        ConnectionMenu.SetActive(false);
        RoomCodeMenu.SetActive(true);
    }

    public void EnterStreamVR()
    {
        ConnectionMenu.SetActive(false);
        RoomCodeMenu.SetActive(false);

        SceneManager.LoadScene("VRGame");
    }
}
