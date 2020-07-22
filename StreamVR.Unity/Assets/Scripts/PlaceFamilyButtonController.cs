﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LMAStudio.StreamVR.Unity.Helpers;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Unity.Scripts;
using UnityEngine;

public class PlaceFamilyButtonController : MonoBehaviour
{
    public LMAStudio.StreamVR.Common.Models.Family buttonFamilyData;

    private GameObject modelInstance;

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

            modelInstance.transform.parent = this.transform;

            modelInstance.transform.localPosition = new Vector3(0,0,0);
            modelInstance.transform.localScale = new Vector3(5,5,5);
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