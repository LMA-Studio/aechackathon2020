﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class RadialMenuController : MonoBehaviour
{
    public GameObject PlaceMenu;
    public GameObject GeneralMenu;
    public GameObject PaletteMenu;
    public GameObject Wand;

    // Start is called before the first frame update
    void Start()
    {
        this.ShowGenMenu();
    }

    // Update is called once per frame
    void Update()
    {       
        
    }

    public void ShowPlaceMenu()
    {
        GeneralMenu.SetActive(false);
        PaletteMenu.SetActive(false);
        Wand.SetActive(false);
        PlaceMenu.SetActive(true);

    }

    public void ShowGenMenu()
    {
        PaletteMenu.SetActive(false);
        Wand.SetActive(false);
        PlaceMenu.SetActive(false);
        GeneralMenu.SetActive(true);
    }

    public void ShowPaletteMenu()
    {
        GeneralMenu.SetActive(false);
        PlaceMenu.SetActive(false);
        PaletteMenu.SetActive(true);
        Wand.SetActive(true);

    }

    public void HideWristMenu()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowWristMenu()
    {
        this.gameObject.SetActive(true);
    }

    public void MasterExit()
    {
        SceneManager.LoadScene("Lobby");
    }


}
