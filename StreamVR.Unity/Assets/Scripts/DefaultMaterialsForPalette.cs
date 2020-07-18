using System.Collections;
using System.Collections.Generic;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;

public class DefaultMaterialsForPalette : MonoBehaviour
{
    private bool initialized = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            Material defaultMaterial = MaterialLibrary.ReverseLookupMaterial("Default");
            Debug.Log(defaultMaterial == null);
            initialized = defaultMaterial != null;
            if (initialized)
            {
                foreach (Transform child in transform)
                {
                    if (child.gameObject.name.Contains("Paint"))
                    {
                        child.gameObject.GetComponentInChildren<MeshRenderer>().material = defaultMaterial;
                    }
                }
            }
        }
    }
}
