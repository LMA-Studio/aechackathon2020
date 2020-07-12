using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClickPlaceMenu : MonoBehaviour
{
    public string MenuName;
    public Sprite Icon;

    // Start is called before the first frame update
    void Start()
    {
        if (Icon == null)
        {
            Icon = this.GetComponentInChildren<Image>().sprite;
        }

        if (string.IsNullOrEmpty(MenuName))
        {
            MenuName=this.gameObject.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickHandler()
    {
        var menu = Resources.FindObjectsOfTypeAll(typeof(MenuController)).First() as MenuController;
        menu.ShowMenu(MenuName, Icon);
    }
}
