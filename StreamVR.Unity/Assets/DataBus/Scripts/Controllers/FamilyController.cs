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

using UnityEngine;

using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Unity.Extensions;
using System.Collections;
using LMAStudio.StreamVR.Unity.Helpers;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class FamilyController : MonoBehaviour
    {
        public string InstanceData = "";

        private FamilyInstance instanceData = null;
        private Family fam = null;
        private GameObject host = null;
        private List<string> hostableInteractions = new List<string>();
        private bool ignoreNextDifference = false;

        private string creatingFamilyId = null;

        void Update()
        {
            if (currentPostion == null || currentRotation == null)
            {
                currentPostion = this.transform.position;
                currentRotation = this.transform.rotation;
            }

            if (creatingFamilyId != null)
            {
                StartCoroutine(PlaceFamily(creatingFamilyId));
                creatingFamilyId = null;
            }
        }

        public void InitPlaceFamily(string familyId)
        {
            this.creatingFamilyId = familyId;
        }

        private IEnumerator PlaceFamily(string familyId)
        {
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

                Debug.Log($"Requesting new family");

                newFam = StreamVR.Instance.PlaceFamilyInstance(newFam);

                Debug.Log($"Loading family instance");

                yield return this.LoadInstance(newFam);

                FamilyInstanceLibrary.AddFamily(newFam, this.gameObject);
            }
            else
            {
                Debug.LogError($"Can't create family from missing ID {familyId}");
            }
        }

        public FamilyInstance GetInstanceData()
        {
            return instanceData;
        }

        public int GetHostLayerMask()
        {
            if (host == null)
            {
                return 1 << Helpers.Constants.LAYER_FLOOR;
            }
            return 1 << host.layer;
        }

        public void UpdateInstanceData(FamilyInstance f)
        {
            instanceData.Transform = f.Transform;
            instanceData.HostId = f.HostId;

            this.transform.SetPosition(instanceData.Transform);

            if (instanceData.HostId != null)
            {
                host = GeometryLibrary.GetObject(instanceData.HostId);
            }

#if UNITY_EDITOR
            this.ignoreNextDifference = this.transform.position != currentPostion || this.transform.rotation != currentRotation;
#endif
        }

        public void LoadInstanceAsync(FamilyInstance f)
        {
            StartCoroutine(this.LoadInstance(f));
        }

        private IEnumerator LoadInstance(FamilyInstance f)
        {
            this.instanceData = f;
            this.InstanceData = Newtonsoft.Json.JsonConvert.SerializeObject(f);
            this.fam = FamilyLibrary.GetFamily(f.FamilyId);
            if (f.HostId != null)
            {
                this.host = GeometryLibrary.GetObject(f.HostId);
            }
            
#if UNITY_EDITOR
            this.name = $"Family ({f.Id} - {this.fam.Tag} - {this.fam.ModelName ?? this.fam.FamilyName})";
#else
            this.name = f.Id;
#endif

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
                modelInstance.transform.localScale = new Vector3(
                    Helpers.Constants.M_PER_FT,
                    Helpers.Constants.M_PER_FT,
                    Helpers.Constants.M_PER_FT
                );

                foreach (var l in modelInstance.GetComponentsInChildren<Light>())
                {
                    l.gameObject.transform.rotation = Quaternion.LookRotation(Vector3.down);
                }

                foreach (UnityEngine.Transform child in transform)
                {
                    foreach (UnityEngine.Transform geo in child)
                    {
                        var c = geo.GetComponent<FamilyGeometryController>();
                        if (c != null)
                        {
                            c.CollisionEnter += this.OnCollsionEnterEvent;
                            c.CollisionExit += this.OnCollisionExitEvent;
                        }

                    }
                }
                // Debug.Log("Children " + count);
            }

#if UNITY_EDITOR
            StartCoroutine(CheckForUpdate());
#endif
        }

        private Vector3? currentPostion = null;
        private Quaternion? currentRotation = null;

        private IEnumerator CheckForUpdate()
        {
            for(; ;)
            {
                if (this.transform.position != currentPostion || this.transform.rotation != currentRotation)
                {
                    yield return UpdatePosition();
                }

                yield return new WaitForSeconds(2);
            }
        }

        public IEnumerator UpdatePosition()
        {
            if (this.instanceData != null)
            {
                currentPostion = this.transform.position;
                currentRotation = this.transform.rotation;

                if (!ignoreNextDifference)
                {
                    yield return SaveSelf();
                    Debug.Log("SAVED " + this.name);
                }
                else
                {
                    ignoreNextDifference = false;
                    Debug.Log("IGNORING " + this.name);
                }
            }
        }

        private IEnumerator SaveSelf()
        {
            FamilyInstance oldData = JObject.Parse(JsonConvert.SerializeObject(this.instanceData)).ToObject<FamilyInstance>();

            this.instanceData.Transform.Origin = new XYZ
            {
                X = this.transform.position.x * Helpers.Constants.FT_PER_M,
                Y = this.transform.position.z * Helpers.Constants.FT_PER_M,
                Z = this.transform.position.y * Helpers.Constants.FT_PER_M,
            };
            this.instanceData.Transform.SetRotation(this.transform);

            // No longer colliding with host
            if (this.instanceData.HostId != null
                && hostableInteractions.Count > 0
                && !hostableInteractions.Contains(this.instanceData.HostId))
            {
                this.instanceData.HostId = hostableInteractions.First();
            }

            yield return StreamVR.Instance.SaveFamilyInstance(this, oldData);

            // hasUpdate = false;

            yield break;
        }

        private void OnCollsionEnterEvent(object sender, Collision collisionInfo)
        {
            Debug.Log("COLLIDING");
            if (host != null)
            {
                GameObject collisionObject = collisionInfo.gameObject;
                if (collisionObject.layer == host.layer)
                {
                    HostController controller = collisionObject.GetComponent<HostController>();
                    if (controller != null && !hostableInteractions.Contains(controller.InstanceData.Id))
                    {
                        hostableInteractions.Add(controller.InstanceData.Id);
                    }
                }
            }
        }

        private void OnCollisionExitEvent(object sender, Collision collisionInfo)
        {
            Debug.Log("COLLIDING EXIT");
            if (host != null)
            {
                GameObject collisionObject = collisionInfo.gameObject;
                if (collisionObject.layer == host.layer)
                {
                    HostController controller = collisionObject.GetComponent<HostController>();
                    if (controller != null && hostableInteractions.Contains(controller.InstanceData.Id))
                    {
                        hostableInteractions.Remove(controller.InstanceData.Id);
                    }
                }
            }
        }
    }
}
