using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Unity.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickStartUpMenu : MonoBehaviour
{
    public GameObject ConnectionMenu;
    public GameObject RoomCodeMenu;
    public GameObject LoadingMenu;
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
        LoadingMenu.SetActive(false);
        ConnectionMenu.SetActive(true);
    }

    public void URLEnterButtonClick()
    {
        urlError.text = "";

        try
        {
            string baseUrl = urlInput.text;

            StreamVR.Instance.Connect(new StreamVROptions()
            {
                NatsBusUrl = baseUrl + ":4222",
                ModelServerUrl = baseUrl + ":8080"
            });

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
        }

        catch (Exception e)
        {
            usernameError.text = e.Message;
            return;
        }

        ShowLoadingMenu();

        Task.Run(StreamVR.Instance.LoadAllAsync);
    }

    public void ShowRoomCodeMenu ()
    {
        ConnectionMenu.SetActive(false);
        LoadingMenu.SetActive(false);
        RoomCodeMenu.SetActive(true);
    }

    public void ShowLoadingMenu ()
    {
        ConnectionMenu.SetActive(false);
        RoomCodeMenu.SetActive(false);
        LoadingMenu.SetActive(true);
        LoadingMenu.GetComponent<WaitForLoad>().BeginWait();
    }
}
