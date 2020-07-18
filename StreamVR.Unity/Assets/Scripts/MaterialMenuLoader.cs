using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;

public class MaterialMenuLoader : MonoBehaviour
{
    public GameObject MaterialButton;

    private bool isLoaded = false;
    private IEnumerable<LMAStudio.StreamVR.Common.Models.Material> allMaterials;

    // Start is called before the first frame update
    void Start()
    {
        allMaterials = MaterialLibrary.GetAllMaterials();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoaded)
        {
            allMaterials = MaterialLibrary.GetAllMaterials();
            isLoaded = allMaterials.Count() > 0;

            if (isLoaded)
            {
                CreateButtons();
            }
        }
    }

    private void CreateButtons()
    {
        var sortedMaterial = allMaterials.OrderBy(m=>m.Name);
        int startX = 100;
        int distanceX = 174;
        int startY = -90;
        int distanceY = 174;

        Debug.Log("creating buttons");
        for (int i = 0; i < sortedMaterial.Count(); i++)
        {
            int column = i % 4;
            int row = i / 4;
            Vector3 position = new Vector3(
                startX + column * distanceX,
                startY - row * distanceY,
                0
            );

            var newButton = Instantiate(MaterialButton, Vector3.zero, MaterialButton.transform.rotation, this.transform);
            newButton.transform.localPosition = position;
            newButton.transform.localRotation = Quaternion.identity;
            newButton.GetComponent<PaintSelectButtonController>().buttonMaterialData = sortedMaterial.ElementAt(i);

            Debug.Log("got here " + i);
        }
    }
}
