using LMAStudio.StreamVR.Unity.Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class DeletePointerController : MonoBehaviour
    {
        public XRNode inputSource;
        public float defaultLength = 5.0f;

        private LineRenderer lineRenderer = null;

        private GameObject selectedObject = null;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, CalculatedEnd());

            if (selectedObject != null)
            {
                Debug.Log($"Device {device == null}");

                bool clicked;
                device.TryGetFeatureValue(CommonUsages.triggerButton, out clicked);

                if (clicked)
                {
                    Debug.Log("CLICKED!");
                    GameObject selectedFamily = selectedObject.transform.parent.parent.gameObject;
                    FamilyController selectedFamilyController = selectedFamily.GetComponent<FamilyController>();
                    Common.Models.FamilyInstance selectedInstanceData = selectedFamilyController.GetInstanceData();

                    List<string> tree = new List<string>();
                    while(selectedInstanceData.SuperComponent != null)
                    {
                        // Cyclical hierarchy
                        if (tree.Contains(selectedInstanceData.SuperComponent))
                        {
                            break;
                        }
                        tree.Add(selectedInstanceData.SuperComponent);

                        selectedFamily = FamilyInstanceLibrary.GetFamily(selectedInstanceData.SuperComponent);
                        selectedFamilyController = selectedFamily.GetComponent<FamilyController>();
                        selectedInstanceData = selectedFamilyController.GetInstanceData();
                    }
                    
                    Debug.Log($"Controller {selectedFamilyController == null}");

                    selectedFamilyController.MarkForDeletion();

                    selectedObject = null;
                }
            }

            bool gripped;
            device.TryGetFeatureValue(CommonUsages.gripButton, out gripped);

            if (gripped)
            {
                Cancel();
            }
        }

        private void Cancel()
        {
            Debug.Log("CANCELLING " + (selectedObject == null).ToString());

            if (selectedObject != null)
            {
                selectedObject = null;
            }

            this.gameObject.SetActive(false);
        }

        private Vector3 CalculatedEnd()
        {
            RaycastHit hit = CreateForwardRaycast();
            Vector3 endPosition = DefaultEnd(defaultLength);

            if (hit.collider)
            {
                Debug.Log("COLLIDING");
                selectedObject = hit.transform.gameObject;
                endPosition = hit.point;
            }
            else if (selectedObject != null)
            {
                Debug.Log("NOT COLLIDING");
                selectedObject = null;
            }

            return endPosition;
        }

        private RaycastHit CreateForwardRaycast()
        {
            RaycastHit hit;

            Ray ray = new Ray(transform.position, transform.forward);

            Physics.Raycast(ray, out hit, defaultLength, 1 << Helpers.Constants.LAYER_FAMILY);

            return hit;
        }

        private Vector3 DefaultEnd(float length)
        {
            return transform.position + (transform.forward * length);
        }
    }
}