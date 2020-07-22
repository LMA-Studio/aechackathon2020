using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LMAStudio.StreamVR.Unity.Helpers;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Unity.Scripts;
using Newtonsoft.Json;
using UnityEngine;

public class PlaceFamilyButtonController : MonoBehaviour
{
    public float rotateSpeed = 10;
    public string familyData;

    public LMAStudio.StreamVR.Common.Models.Family buttonFamilyData;

    private GameObject modelInstance;

    // Start is called before the first frame update
    void Start()
    {
        familyData = JsonConvert.SerializeObject(buttonFamilyData);
        StartCoroutine(Load());
    }

    // Update is called once per frame
    void Update()
    {
        modelInstance.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private IEnumerator Load()
    {
        CoroutineWithData<object> cd = new CoroutineWithData<object>(this, FamilyLibrary.ResolveFamilyOBJ(buttonFamilyData.Id, null));
        yield return cd.coroutine;
        object result = cd.result;

        if (result != null)
        {
            byte[] objData = cd.result as byte[];

            using (var textStream = new MemoryStream(objData))
            {
                modelInstance = new OBJLoader().Load(textStream);
            }

            double maxX = buttonFamilyData.BoundingBoxMax.X - buttonFamilyData.BoundingBoxMin.X;
            double maxY = buttonFamilyData.BoundingBoxMax.Y - buttonFamilyData.BoundingBoxMin.Y;
            double maxZ = buttonFamilyData.BoundingBoxMax.Z - buttonFamilyData.BoundingBoxMin.Z;

            double largestDim = maxX;

            if (maxY > largestDim)
            {
                largestDim = maxY;
            }

            if (maxZ > largestDim)
            {
                largestDim = maxZ;
            }

            float newScale = 5;

            if (largestDim <= 7)
            {
                newScale = 15;
            }

            else if (largestDim <= 10)
            {
                newScale = 7;
            }

            float depth = Mathf.Sqrt(Mathf.Pow((float)maxX, 2) + Mathf.Pow((float)maxY, 2)) / 2;
            Vector3 translation = Vector3.forward * -depth;

            modelInstance.transform.parent = this.transform;

            modelInstance.transform.localPosition = translation * newScale;
            modelInstance.transform.localScale = new Vector3(newScale, newScale, newScale);
            
        }
        
        this.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = FormatName(buttonFamilyData.FamilyName);
    }

    public void ClickHandler()
    { 
        var placePointer = Resources.FindObjectsOfTypeAll(typeof(FamilyPlacementPointerController)).First() as FamilyPlacementPointerController;
        placePointer.BeginPlacing(modelInstance, buttonFamilyData);
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
