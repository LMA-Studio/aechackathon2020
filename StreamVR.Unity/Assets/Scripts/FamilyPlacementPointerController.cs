using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Logic;
using UnityEngine;
using UnityEngine.XR;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class FamilyPlacementPointerController : MonoBehaviour
    {
        public GameObject worldRoot;
        public XRNode inputSource;
        public float defaultLength = 5.0f;
        public float defaultLengthPlace = 10.0f;
        public float rotationSpeed = 1.0f;

        public GameObject placemenu;

        private LineRenderer lineRenderer = null;

        private GameObject placingObject = null;
        private Family familyDef = null;

        private Quaternion currentRotation = Quaternion.identity;

        private bool clicking = false;

        private bool colliderFloorHit = false;

        public void BeginPlacing(GameObject baseObject, Family family)
        {
            Vector3 collisionPoint = CalculatedEndFloor();

            this.placingObject = Instantiate(baseObject, collisionPoint, Quaternion.identity);
            this.placingObject.transform.parent = this.transform;
            this.placingObject.transform.localScale = new Vector3(
                Helpers.Constants.M_PER_FT,
                Helpers.Constants.M_PER_FT,
                Helpers.Constants.M_PER_FT
            );
            this.familyDef = family;

            this.gameObject.SetActive(true);
        }


        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            if (this.placingObject != null)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);

                device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stickMove);

                currentRotation *= Quaternion.Euler(Vector3.up * stickMove.y * rotationSpeed);

                Vector3 collisionPoint = CalculatedEndFloor();

                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, collisionPoint);

                placingObject.transform.position = collisionPoint;
                placingObject.transform.rotation = currentRotation;

                placingObject.SetActive(colliderFloorHit);

                device.TryGetFeatureValue(CommonUsages.triggerButton, out bool clicked);

                if (clicked)
                {
                    if (!clicking)
                    {
                        clicking = true;

                        if (colliderFloorHit)
                        {
                            PutDown();
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

                device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripped);
                if (gripped)
                {
                    Cancel();
                }
            }
        }

        private void PutDown()
        {
            Debug.Log("CREATING OBJECT!");

            GameObject placedObject = new GameObject();
            placedObject.transform.position = placingObject.transform.position;
            placedObject.transform.rotation = placingObject.transform.rotation;
            placedObject.transform.parent = worldRoot.transform;

            placedObject.layer = Helpers.Constants.LAYER_FAMILY;
            placedObject.AddComponent<FamilyController>().InitPlaceFamily(familyDef.Id);

            Destroy(placingObject);
            placingObject = null;
            familyDef = null;
            currentRotation = Quaternion.identity;

            this.gameObject.SetActive(false);
            placemenu.gameObject.SetActive(true);
        }

        private void Cancel()
        {
            Debug.Log("CANCELLING " + (placingObject == null).ToString());

            if (placingObject != null)
            {
                GameObject.Destroy(placingObject);

                placingObject = null;
                familyDef = null;
                currentRotation = Quaternion.identity;
            }

            this.gameObject.SetActive(false);
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

        private RaycastHit CreateForwardRaycastFloor()
        {
            RaycastHit hit;

            Ray ray = new Ray(transform.position, transform.forward);

            int layerMask = 1 << Helpers.Constants.LAYER_FLOOR; // originalObjectController.GetHostLayerMask();
            Physics.Raycast(ray, out hit, defaultLengthPlace, layerMask);

            return hit;
        }


        private Vector3 DefaultEnd(float length)
        {
            return transform.position + (transform.forward * length);
        }
    }
}
