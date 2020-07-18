using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookMenuInteraction : MonoBehaviour
{
    public Camera cameraView;
    public RadialMenuController radialMenu;
    public Material hitMaterial;
    public Material waitingMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isCameraOnMenu())
        {
            radialMenu.ShowWristMenu();
        }

        else
        {
            radialMenu.HideWristMenu();
        }
        
    }

    private bool isCameraOnMenu()
    {
        Ray ray = new Ray(cameraView.transform.position, cameraView.transform.rotation * Vector3.forward);
        int mask = 1 << 11;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            GameObject hitObject = hit.transform.gameObject;
            if (hitObject.GetInstanceID() == this.gameObject.GetInstanceID())
            {
                return true;
            }
        }
        
        return false;
    }
}
