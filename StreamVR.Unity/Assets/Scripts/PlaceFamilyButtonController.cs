using System.Collections;
using System.Collections.Generic;
using System.IO;
using LMAStudio.StreamVR.Unity.Helpers;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;

public class PlaceFamilyButtonController : MonoBehaviour
{
    public LMAStudio.StreamVR.Common.Models.Family buttonFamilylData;

    private Material buttonMaterial;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Load());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Load()
    {
        
        CoroutineWithData<object> cd = new CoroutineWithData<object>(this, FamilyLibrary.ResolveFamilyOBJ(buttonFamilylData.Id, null));
        yield return cd.coroutine;
        object result = cd.result;

        if (result != null)
        {
            byte[] objData = cd.result as byte[];

            GameObject modelInstance;
            using (var textStream = new MemoryStream(objData))
            {
                modelInstance = new OBJLoader().Load(textStream);
            }

            modelInstance.transform.parent = this.transform;

            modelInstance.transform.localPosition = new Vector3(0,0,0);
            modelInstance.transform.localScale = new Vector3(5,5,5);
        }
        
        this.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = FormatName(buttonFamilylData.FamilyName);
    }

    public void ClickHandler()
    { 
        // var paintWandEnd = Resources.FindObjectsOfTypeAll(typeof(PaintWandEndController)).First() as PaintWandEndController;
        // paintWandEnd.SelectMaterial(buttonMaterial);
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
