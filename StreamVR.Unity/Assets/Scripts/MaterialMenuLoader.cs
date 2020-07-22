using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialMenuLoader : MonoBehaviour
{
    public GameObject MaterialButton;
    public TMPro.TextMeshProUGUI pageNumberText;
    public Button previousButton;
    public Button nextButton;

    private bool isLoaded = false;
    private IEnumerable<LMAStudio.StreamVR.Common.Models.Material> allMaterials;
    private int pageNumber = 0;
    private int totalPages = 0;

    private readonly int pageSize = 8;


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
                totalPages = allMaterials.Count() / pageSize;
                RenderPage();
            }
            else 
            {
                pageNumberText.text = "LOADING ...";
            }
        }
    }

    private void RenderPage ()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
       
        if (pageNumber <= 0)
        {
            previousButton.enabled = false;
            pageNumber = 0;
        }

        else if(pageNumber >= totalPages)
        {
            nextButton.enabled = false;
            pageNumber = totalPages;
        }
        else if (!previousButton.enabled || !nextButton.enabled)
        {
            nextButton.enabled = true;
            previousButton.enabled = true;
        }

        this.CreateButtons();
        pageNumberText.text = (pageNumber + 1) + "/" + (totalPages + 1);
    }

    private void CreateButtons()
    {

        var sortedMaterial = allMaterials.
            OrderBy(m=>m.Name).
            Skip(pageNumber * pageSize).
            Take(pageSize);

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

    public void NextPage()
    {
        pageNumber += 1;
        RenderPage();
    }

    public void PreviousPage()
    {
        pageNumber -= 1;
        RenderPage();
    }
}
