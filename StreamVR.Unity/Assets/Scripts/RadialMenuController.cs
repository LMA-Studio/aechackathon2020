using System.Collections;
using System.Collections.Generic;
using LMAStudio.StreamVR.Unity.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class RadialMenuController : MonoBehaviour
{
    public GameObject PlaceMenu;
    public GameObject GeneralMenu;
    public GameObject PaletteMenu;
    public GameObject MoveMenu;
    public GameObject Wand;
    public GameObject MoveRay;
    public GameObject PlaceRay;
    public GameObject DeleteRay;

    public GameObject PlaceSelectMenu;
    public GameObject PaintSelectMenu;

    public LocomotionController locomotionController;

    // Start is called before the first frame update
    void Start()
    {
        this.ShowGenMenu();
        locomotionController.enableMoveRay = false;
        locomotionController.enablePlaceRay = false;
        locomotionController.enableDeleteRay = false;
    }

    // Update is called once per frame
    void Update()
    {       
        
    }

    public void ShowMoveMenu()
    {
        GeneralMenu.SetActive(false);
        PaletteMenu.SetActive(false);
        Wand.SetActive(false);
        PlaceMenu.SetActive(false);
        MoveMenu.SetActive(true);
        ActivateMoveRay();
    }

    public void ActivateMoveRay()
    {
        locomotionController.enableMoveRay = true;
        locomotionController.enableDeleteRay = false;
    }

    public void ActivateDeleteRay()
    {
        locomotionController.enableDeleteRay = true;
        locomotionController.enableMoveRay = false;
    }

    public void ShowPlaceMenu()
    {
        GeneralMenu.SetActive(false);
        PaletteMenu.SetActive(false);
        Wand.SetActive(false);
        MoveMenu.SetActive(false);
        locomotionController.enableMoveRay = false;
        locomotionController.enableDeleteRay = false;
        locomotionController.enablePlaceRay = true;
        PlaceMenu.SetActive(true);

    }

    public void ShowGenMenu()
    {
        PaletteMenu.SetActive(false);
        Wand.SetActive(false);
        PlaceMenu.SetActive(false);
        MoveMenu.SetActive(false);
        PlaceSelectMenu.SetActive(false);
        PaintSelectMenu.SetActive(false);
        locomotionController.enableMoveRay = false;
        locomotionController.enablePlaceRay = false;
        locomotionController.enableDeleteRay = false;
        GeneralMenu.SetActive(true);
    }

    public void ShowPaletteMenu()
    {
        GeneralMenu.SetActive(false);
        PlaceMenu.SetActive(false);
        MoveMenu.SetActive(false);
        locomotionController.enableMoveRay = false;
        locomotionController.enablePlaceRay = false;
        locomotionController.enableDeleteRay = false;
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
