using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class FamilyMovementPointerController : MonoBehaviour
    {
        public Material ghostMaterial;
        public XRNode inputSource;
        public float defaultLength = 3.0f;
        public float defaultLengthPlace = 7.0f;
        public float rotationSpeed = 50.0f;

        private LineRenderer lineRenderer = null;

        private GameObject selectedObject = null;
        private GameObject origionalObject = null;

        private Quaternion currentRotation = Quaternion.identity;

        private bool clicking = false;
        private bool inHand = false;

        private bool colliderFloorHit = false;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);

            if (selectedObject != null)
            {
                if (inHand)
                {
                    Vector2 stickMove;
                    device.TryGetFeatureValue(CommonUsages.primary2DAxis, out stickMove);

                    currentRotation *= Quaternion.Euler(Vector3.up * stickMove.y * rotationSpeed);

                    Vector3 collisionPoint = CalculatedEndFloor();

                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, collisionPoint);

                    selectedObject.transform.position = collisionPoint;
                    selectedObject.transform.rotation = currentRotation;

                    selectedObject.SetActive(colliderFloorHit);
                }
                else
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, CalculatedEnd());
                }

                bool clicked;
                device.TryGetFeatureValue(CommonUsages.triggerButton, out clicked);

                if (clicked)
                {
                    Debug.Log("CLICKED " + clicking.ToString() + "," + inHand.ToString() + "," + colliderFloorHit.ToString());
                    if (!clicking)
                    {
                        clicking = true;

                        if (inHand && colliderFloorHit)
                        {
                            PutDown();
                        }
                        else if (!inHand)
                        {
                            PickUp();
                        }
                        else
                        {
                            Debug.Log("Clicked into space");
                            Debug.Log(this.transform.position);
                        }
                    }
                }
                else
                {
                    clicking = false;
                }
            }
            else
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, CalculatedEnd());
            }

            bool gripped;
            device.TryGetFeatureValue(CommonUsages.gripButton, out gripped);

            if (gripped)
            {
                Cancel();
            }
        }

        private void PickUp()
        {
            Debug.Log("PICKING UP");
            if (selectedObject != null && selectedObject.transform.parent != null)
            {
                origionalObject = selectedObject.transform.parent.parent.gameObject;

                selectedObject = GameObject.Instantiate(origionalObject, origionalObject.transform.position, origionalObject.transform.rotation);

                foreach (Transform child in selectedObject.transform)
                {
                    var allGeometry = child.gameObject.GetComponentsInChildren<MeshRenderer>();
                    foreach (var geometry in allGeometry)
                    {
                        geometry.material = ghostMaterial;
                    }
                }

                inHand = true;
            }
        }
        private void PutDown()
        {
            Debug.Log("PUTTING DOWN");

            if (selectedObject != null)
            {
                origionalObject.transform.position = selectedObject.transform.position;
                origionalObject.transform.rotation = selectedObject.transform.rotation;

                GameObject.Destroy(selectedObject);
                selectedObject = null;

                origionalObject.GetComponent<FamilyController>().UpdatePosition();
                origionalObject = null;

            }

            selectedObject = null;
            inHand = false;
        }
        private void Cancel()
        {
            Debug.Log("CANCELLING " + (selectedObject == null).ToString());

            if (selectedObject != null)
            {
                GameObject.Destroy(selectedObject);
                selectedObject = null;
                origionalObject = null;

            }

            selectedObject = null;
            inHand = false;

            // this.gameObject.SetActive(false);
        }

        private Vector3 CalculatedEnd()
        {
            RaycastHit hit = CreateForwardRaycast();
            Vector3 endPosition = DefaultEnd(defaultLength);

            if (hit.collider)
            {
                Debug.Log("COLLIDING");
                selectedObject = hit.transform.gameObject;
            }
            else if (selectedObject != null)
            {
                selectedObject = null;
            }

            return endPosition;
        }

        private Vector3 CalculatedEndFloor()
        {
            RaycastHit hit = CreateForwardRaycastFloor();

            colliderFloorHit = hit.collider;

            if (hit.collider)
            {
                return hit.point;
            }

            return DefaultEnd(defaultLengthPlace);
        }

        private RaycastHit CreateForwardRaycast()
        {
            RaycastHit hit;

            Ray ray = new Ray(transform.position, transform.forward);

            Physics.Raycast(ray, out hit, defaultLength, 1 << Helpers.Constants.LAYER_FAMILY);

            return hit;
        }

        private RaycastHit CreateForwardRaycastFloor()
        {
            RaycastHit hit;

            Ray ray = new Ray(transform.position, transform.forward);

            Physics.Raycast(ray, out hit, defaultLengthPlace, 1 << 9);

            return hit;
        }


        private Vector3 DefaultEnd(float length)
        {
            return transform.position + (transform.forward * length);
        }
    }
}