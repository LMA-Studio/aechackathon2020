/*
    This file is part of LMAStudio.StreamVR
    Copyright(C) 2020  Andreas Brake, Lisa-Marie Mueller

    LMAStudio.StreamVR is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;

using UnityEngine;

using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Unity.Extensions;
using System.Collections;
using LMAStudio.StreamVR.Unity.Helpers;
using System.IO;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class FamilyController : MonoBehaviour
    {
        public string CreatedFromFamilyId;
        public string InstanceData = "";

        private FamilyInstance instanceData = null;
        private Family fam = null;

        private void Start()
        {
            if (this.CreatedFromFamilyId != null)
            {
                PlaceFamily(null, CreatedFromFamilyId);
            }
        }

        public void PlaceFamily(GameObject parent, string familyId)
        {
            if (parent != null)
            {
                this.transform.parent = parent.transform;
            }

            if (FamilyLibrary.GetFamily(familyId) != null)
            {
                FamilyInstance newFam = new FamilyInstance
                {
                    FamilyId = familyId,
                    Transform = new Common.Models.Transform
                    {
                        Origin = new XYZ
                        {
                            X = this.transform.position.x * Helpers.Constants.FT_PER_M,
                            Y = this.transform.position.z * Helpers.Constants.FT_PER_M,
                            Z = this.transform.position.y * Helpers.Constants.FT_PER_M
                        }
                    }
                };

                newFam = StreamVR.Instance.PlaceFamilyInstance(newFam);

                this.LoadInstanceAsync(newFam);
            }
            else
            {
                Debug.LogError($"Can't create family from missing ID {familyId}");
            }
        }

        public void LoadInstanceAsync(FamilyInstance f)
        {
            StartCoroutine(this.LoadInstance(f));
        }

        private IEnumerator LoadInstance(FamilyInstance f)
        {
            CreatedFromFamilyId = null;

            Matrix4x4 rotM = f.Transform.GetRotation();
            //Matrix4x4 rotMI = rotM.inverse;

            //Vector3 bbMin = new Vector3((float)f.BoundingBoxMin.X, (float)f.BoundingBoxMin.Y, (float)f.BoundingBoxMin.Z);
            //Vector3 bbMax = new Vector3((float)f.BoundingBoxMax.X, (float)f.BoundingBoxMax.Y, (float)f.BoundingBoxMax.Z);

            //Vector3 bbMinRot = rotMI.MultiplyPoint(bbMin);
            //Vector3 bbMaxRot = rotMI.MultiplyPoint(bbMax);

            //this.transform.localScale = new Vector3(
            //    bbMax.x - bbMin.x,
            //    bbMax.y - bbMin.y,
            //    bbMax.z - bbMin.z
            //);

            // TODO: Create collider based on BB

            this.name = $"Family ({f.Id})";
            this.instanceData = f;
            this.InstanceData = Newtonsoft.Json.JsonConvert.SerializeObject(f);
            this.fam = FamilyLibrary.GetFamily(f.FamilyId);

            // GameObject model = (GameObject)Resources.Load($"Families/{this.fam.Name}/model");
            CoroutineWithData<object> cd = new CoroutineWithData<object>(this, FamilyLibrary.ResolveFamilyOBJ(f.FamilyId, f.VariantId));
            yield return cd.coroutine;
            object result = cd.result;

            if (result != null)
            {
                byte[] objData = cd.result as byte[];

                // Debug.Log("PLACING FAMILY");

                GameObject modelInstance;
                using (var textStream = new MemoryStream(objData))
                {
                    modelInstance = new OBJLoader().Load(textStream);
                }

                var initialRotation = modelInstance.transform.localRotation;

                modelInstance.transform.parent = this.transform;
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = initialRotation;
                modelInstance.transform.localScale = new Vector3(0.3048f, 0.3048f, 0.3048f);

                foreach(var l in modelInstance.GetComponentsInChildren<Light>())
                {
                    l.gameObject.transform.rotation = Quaternion.LookRotation(Vector3.down);
                }

                //Debug.Log("Parent " + this.gameObject.name);
                //Debug.Log("Child " + modelInstance.gameObject.name);

                var childXR = modelInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                if (childXR != null)
                {
                    // Debug.Log("HAS XR");
                    GameObject.Destroy(childXR);
                }


                BoxCollider childBox = modelInstance.GetComponent<BoxCollider>();
                if (childBox != null)
                {
                    // Debug.Log("HAS BOX");
                    BoxCollider parentBox = this.gameObject.AddComponent<BoxCollider>();
                    parentBox.size = childBox.size;
                    parentBox.center = childBox.center;

                    modelInstance.layer = 0;
                    this.gameObject.layer = Helpers.Constants.LAYER_FAMILY;

                    GameObject.Destroy(childBox);
                }

                Rigidbody childRB = modelInstance.GetComponent<Rigidbody>();
                if (childRB != null)
                {
                    // Debug.Log("HAS RB");
                    Rigidbody parentRB = this.gameObject.AddComponent<Rigidbody>();
                    parentRB.useGravity = false;
                    parentRB.constraints = RigidbodyConstraints.FreezeAll;

                    GameObject.Destroy(childRB);
                }

                int count = 0;
                foreach (UnityEngine.Transform child in transform)
                {
                    count++;
                    if (child.gameObject.GetComponent<Rigidbody>() != null)
                    {
                       //  Debug.Log("HAS RB " + count);
                    }

                }
                // Debug.Log("Children " + count);

                this.name = $"_ Family ({f.Id} - {this.fam.Tag} - {this.fam.ModelName ?? this.fam.FamilyName})";
            }
        }

        private Vector3? currentPostion = null;
        private Quaternion? currentRotation = null;

        //private bool hasUpdate = false;
        //private DateTime firstUpdate = DateTime.UtcNow;
        //private DateTime lastUpdate = DateTime.UtcNow;
        //private double DebounceTime = 500;

        void Update()
        {
            if (currentPostion == null || currentRotation == null)
            {
                currentPostion = this.transform.position;
                currentRotation = this.transform.rotation;
            }
        }

        public void UpdatePosition()
        {
            Debug.Log("Updating position");
            if (this.instanceData != null)
            {
                Debug.Log("Registering Update");

                currentPostion = this.transform.position;
                currentRotation = this.transform.rotation;

                SaveSelf();
                Debug.Log("SAVED");
            }
        }

        private void SaveSelf()
        {
            this.instanceData.Transform.Origin = new XYZ
            {
                X = this.transform.position.x * Helpers.Constants.FT_PER_M,
                Y = this.transform.position.z * Helpers.Constants.FT_PER_M,
                Z = this.transform.position.y * Helpers.Constants.FT_PER_M,
            };

            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(this.transform.rotation);
            this.instanceData.Transform.SetRotation(rotationMatrix, this.instanceData.IsFlipped);

            StreamVR.Instance.SaveFamilyInstance(this.instanceData);
            // hasUpdate = false;
        }
    }
}
