using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceElementsMenuController : MonoBehaviour
{
    public GameObject ElementButton;
    public TMPro.TextMeshProUGUI pageNumberText;
    public Button previousButton;
    public Button nextButton;

    public string filterTag;

    private bool isLoaded = false;
    private IEnumerable<LMAStudio.StreamVR.Common.Models.Family> allElements;
    private int pageNumber = 0;
    private int totalPages = 0;

    private readonly int pageSize = 8;

    // Start is called before the first frame update
    void Start()
    {
        allElements = FamilyLibrary.GetFamiliesForTag(filterTag);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoaded)
        {
            allElements = FamilyLibrary.GetFamiliesForTag(filterTag);
            isLoaded = allElements.Count() > 0;

            if (isLoaded)
            {
                totalPages = allElements.Count() / pageSize;
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
            previousButton.interactable = false;
            pageNumber = 0;
        }

        else if(pageNumber >= totalPages)
        {
            nextButton.interactable = false;
            pageNumber = totalPages;
        }
        else if (!previousButton.interactable || !nextButton.interactable)
        {
            nextButton.interactable = true;
            previousButton.interactable = true;
        }

        this.CreateButtons();
        pageNumberText.text = (pageNumber + 1) + "/" + (totalPages + 1);
    }

    private void CreateButtons()
    {

        var sortedElements = allElements.
            OrderBy(m=>m.Name).
            Skip(pageNumber * pageSize).
            Take(pageSize);

        int startX = 100;
        int distanceX = 174;
        int startY = -90;
        int distanceY = 174;

        Debug.Log("creating buttons");
        for (int i = 0; i < sortedElements.Count(); i++)
        {
            int column = i % 4;
            int row = i / 4;
            Vector3 position = new Vector3(
                startX + column * distanceX,
                startY - row * distanceY,
                0
            );

            var newButton = Instantiate(ElementButton, Vector3.zero, ElementButton.transform.rotation, this.transform);
            newButton.transform.localPosition = position;
            newButton.transform.localRotation = Quaternion.identity;
            newButton.GetComponent<PlaceFamilyButtonController>().buttonFamilylData = sortedElements.ElementAt(i);

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
