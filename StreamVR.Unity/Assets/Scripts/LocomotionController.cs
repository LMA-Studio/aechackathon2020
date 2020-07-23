using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionController : MonoBehaviour
{
    public XRController leftTeleportRay;
    public XRController rightTeleportRay;
    public GameObject deleteRay;
    public GameObject moveRay;
    public GameObject placeRay;

    public InputHelpers.Button teleportActivationButton;
    public float activationThreshold = 0.1f;
    
    public XRRayInteractor leftInteractorRay;
    public XRRayInteractor rightInteractorRay;
    public bool enableLeftTeleport {get; set;} = true;
    public bool enableRightTeleport {get; set;}  = true;
    public bool enableDeleteRay {get; set;}  = false;
    public bool enableMoveRay {get; set;}  = false;
    public bool enablePlaceRay {get; set;}  = false;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = new Vector3();
        Vector3 norm = new Vector3();
        int index = 0;
        bool validTarget = false;
        
        if(leftTeleportRay)
        {
            bool isLeftInteractorRayHovering = leftInteractorRay.TryGetHitInfo(ref pos, ref norm, ref index, ref validTarget);
            leftTeleportRay.gameObject.SetActive(enableLeftTeleport && CheckIfActivated(leftTeleportRay) && !isLeftInteractorRayHovering);
        }

        if(rightTeleportRay)
        {
            bool isRightInteractorRayHovering = rightInteractorRay.TryGetHitInfo(ref pos, ref norm, ref index, ref validTarget);
            rightTeleportRay.gameObject.SetActive(enableRightTeleport && CheckIfActivated(rightTeleportRay) && !isRightInteractorRayHovering);
        }

        if(deleteRay)
        {
            bool isRightInteractorRayHovering = rightInteractorRay.TryGetHitInfo(ref pos, ref norm, ref index, ref validTarget);
            deleteRay.gameObject.SetActive(enableDeleteRay && !isRightInteractorRayHovering);
        }

        if(moveRay)
        {
            bool isRightInteractorRayHovering = rightInteractorRay.TryGetHitInfo(ref pos, ref norm, ref index, ref validTarget);
            moveRay.gameObject.SetActive(enableMoveRay && !isRightInteractorRayHovering);
        }

        if(placeRay)
        {
            bool isRightInteractorRayHovering = rightInteractorRay.TryGetHitInfo(ref pos, ref norm, ref index, ref validTarget);
            placeRay.gameObject.SetActive(enablePlaceRay && !isRightInteractorRayHovering);
        }

    }

    public bool CheckIfActivated(XRController controller)
    {
        InputHelpers.IsPressed(controller.inputDevice, teleportActivationButton, out bool isActivated, activationThreshold);
        return isActivated;
    }
}
