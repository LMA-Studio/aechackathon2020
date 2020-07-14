using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickStartUpMenu : MonoBehaviour
{
    public GameObject ConnectionMenu;
    public GameObject RoomCodeMenu;
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
