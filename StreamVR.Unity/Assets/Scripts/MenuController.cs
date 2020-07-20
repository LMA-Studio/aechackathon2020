using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject HeadingIcon;
    public GameObject HeadingText;

    public PlaceElementsMenuController menuController;

    // Start is called before the first frame update
    void Start()
    {
        //this.HideMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMenu (string text, string itemTag, Sprite icon)
    {
        HeadingText.GetComponent<TMPro.TextMeshProUGUI>().text = text;

        HeadingIcon.GetComponent<SpriteRenderer>().sprite = icon;

        menuController.filterTag = itemTag;

        this.gameObject.SetActive(true);
    }

    public void HideMenu ()
    {
        this.gameObject.SetActive(false);
    }

    public void GoBack()
    {
        this.HideMenu();
        var radialMenu = Resources.FindObjectsOfTypeAll(typeof(RadialMenuController)).First() as RadialMenuController;
        radialMenu.ShowPlaceMenu();
    }
}
