using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaintSelectMenuController : MonoBehaviour
{
    public GameObject HeadingIcon;
    public GameObject HeadingText;

    public PaintWandEndController wand;
    
    // Start is called before the first frame update
    void Start()
    {
        // this.HideMenu(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMenu (string text, Sprite icon)
    {
        HeadingText.GetComponent<TMPro.TextMeshProUGUI>().text = text;

        HeadingIcon.GetComponent<SpriteRenderer>().sprite = icon;

        this.gameObject.SetActive(true);

        wand.StartSelecting();

    }

    public void HideMenu ()
    {
        this.gameObject.SetActive(false);
        wand.EndSelecting();
    }
}
