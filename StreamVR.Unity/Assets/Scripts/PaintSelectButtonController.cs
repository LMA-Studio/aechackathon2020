using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;

public class PaintSelectButtonController : MonoBehaviour
{
    public LMAStudio.StreamVR.Common.Models.Material buttonMaterialData;

    private Material buttonMaterial;

    // Start is called before the first frame update
    void Start()
    {
        buttonMaterial = MaterialLibrary.LookupMaterial(buttonMaterialData.Id);
        
        this.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = FormatName(buttonMaterialData.Name);
        this.gameObject.GetComponentInChildren<SpriteRenderer>().material = buttonMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickHandler()
    { 
        var paintWandEnd = Resources.FindObjectsOfTypeAll(typeof(PaintWandEndController)).First() as PaintWandEndController;
        paintWandEnd.SelectMaterial(buttonMaterial);
    }

    private string FormatName (string Name)
    {
        string reformatedName = Name.Replace('_', ' ');
        if (reformatedName.Length > 26)
        {
            reformatedName = reformatedName.Substring(0,26) + "...";
        }
        return reformatedName;
    }
}
