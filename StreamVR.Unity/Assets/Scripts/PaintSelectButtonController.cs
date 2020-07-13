using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaintSelectButtonController : MonoBehaviour
{
    public Material buttonMaterial;
    // Start is called before the first frame update
    void Start()
    {
        if (buttonMaterial == null)
        {
            buttonMaterial = this.GetComponentInChildren<MeshRenderer>().material;
        }
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
}
