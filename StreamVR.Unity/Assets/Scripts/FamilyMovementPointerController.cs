using LMAStudio.StreamVR.Unity.Logic;
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
        private FamilyController originalObjectController = null;
        private GameObject origionalObject = null;

        private Quaternion currentRotation = Quaternion.identity;

        private bool clicking = false;
        private bool inHand = false;

        private bool colliderFloorHit = false;

        // Ensure that activation click doesn't pass through to hovered object
        private bool haltAction = true;

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

                if (clicked && !haltAction)
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

        public void WakeUpPointer()
        {
            this.haltAction = true;
            this.gameObject.SetActive(true);
            this.Cancel();
            StartCoroutine(WakeUp());
        }

        private IEnumerator WakeUp()
        {
            yield return new WaitForSeconds(2f);
            this.haltAction = false;
        }

        public void SleepPointer()
        {
            this.haltAction = true;
            this.Cancel();
            this.gameObject.SetActive(false);
        }

        private void PickUp()
        {
            Debug.Log("PICKING UP");
            if (selectedObject != null && selectedObject.transform.parent != null)
            {
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

                origionalObject = selectedFamily;
                originalObjectController = selectedFamilyController;
                currentRotation = origionalObject.transform.rotation;
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

                var controller = origionalObject.GetComponent<FamilyController>();
                StartCoroutine(UpdateAndDestroyGhost(controller, selectedObject));

                selectedObject = null;
                originalObjectController = null;
                origionalObject = null;
            }

            inHand = false;
        }

        private IEnumerator UpdateAndDestroyGhost(FamilyController controller, GameObject ghost)
        {
            yield return controller.UpdatePosition();
            GameObject.Destroy(ghost);
        }

        private void Cancel()
        {
            Debug.Log("CANCELLING " + (selectedObject == null).ToString());

            if (selectedObject != null && inHand)
            {
                GameObject.Destroy(selectedObject);
            }

            selectedObject = null;
            originalObjectController = null;
            origionalObject = null;

            currentRotation = Quaternion.identity;
            inHand = false;
        }

        private Vector3 CalculatedEnd()
        {
            RaycastHit hit = CreateForwardRaycast();
            Vector3 endPosition = DefaultEnd(defaultLength);

            if (hit.collider && !haltAction)
            {
                Debug.Log("COLLIDING");
                selectedObject = hit.transform.gameObject;
                endPosition = hit.point;
            }
            else
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

            int layerMask = 1 << Helpers.Constants.LAYER_FAMILY;
            Physics.Raycast(ray, out hit, defaultLength, layerMask);

            return hit;
        }

        private RaycastHit CreateForwardRaycastFloor()
        {
            RaycastHit hit;

            Ray ray = new Ray(transform.position, transform.forward);

            int layerMask = originalObjectController.GetHostLayerMask();
            Physics.Raycast(ray, out hit, defaultLengthPlace, layerMask);

            return hit;
        }


        private Vector3 DefaultEnd(float length)
        {
            return transform.position + (transform.forward * length);
        }
    }
}