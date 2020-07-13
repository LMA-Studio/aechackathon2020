using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClickPaintMenu : MonoBehaviour
{
    private string menuName;
    private Sprite icon;
    // Start is called before the first frame update
    void Start()
    {
        menuName = "Material Selection";
        icon = this.GetComponentInChildren<Image>().sprite; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickHandler()
    {
       var menu = Resources.FindObjectsOfTypeAll(typeof(PaintSelectMenuController)).First() as PaintSelectMenuController;
       menu.ShowMenu(menuName, icon);
    }
}
