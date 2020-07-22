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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Common;
using System.Collections;
using LMAStudio.StreamVR.Unity.Helpers;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class StreamVROptions
    {
        public bool LoadMaterials = true;
        public bool LoadFamilies = true;
        public bool LoadWalls = true;
        public bool LoadFloors = true;
        public bool LoadCeilings = true;
        public bool LoadFamilyInstances = true;
    }

    public class StreamVR
    {
        private static StreamVR _instance { get; set; }

        public static StreamVR Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StreamVR();
                }
                return _instance;
            }
        }

        public bool IsLoaded { get; private set; } = false;

        public List<Common.Models.Material> Materials { get; private set; }
        public List<Family> Families { get; private set; }
        public List<Wall> Walls { get; private set; }
        public List<Floor> Floors { get; private set; }
        public List<Ceiling> Ceilings { get; private set; }
        public List<FamilyInstance> FamilyInstances { get; private set; }

        private StreamVROptions options;
        private ICommunicator comms;

        public void Connect(StreamVROptions options)
        {
            this.options = options;
            this.comms = BusConnector.Connect();
            //var view = this.GetStartingOrientation();
            //Debug.Log(JsonConvert.SerializeObject(view));
        }

        #region Loaders

        public async Task LoadAllAsync()
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();

            if (options.LoadMaterials)
            {
                await this.LoadMaterials();
                Debug.Log("Got materials");
            }

            if (options.LoadFamilies)
            {
                await this.LoadFamilies();
                Debug.Log("Got Families");
            }

            if (options.LoadWalls)
            {
                await this.LoadWalls();
                Debug.Log("Got Walls");
            }

            if (options.LoadFloors)
            {
                await this.LoadFloors();
                Debug.Log("Got Floors");
            }

            if (options.LoadCeilings)
            {
                await this.LoadCeilings();
                Debug.Log("Got Ceilings");
            }

            if (options.LoadFamilyInstances)
            {
                await this.LoadFamilyInstances();
                Debug.Log("Got Family Instances");
            }

            Debug.Log($"StreamVR Download time: {s.ElapsedMilliseconds}ms");
            s.Stop();

            IsLoaded = true;
        }

        public async Task LoadMaterials()
        {
            List<JObject> dataSet = await LoadTypeAsync("Autodesk.Revit.DB.Material", "Material");
            Materials = dataSet.Select(x => x.ToObject<Common.Models.Material>()).ToList();
        }

        public async Task LoadFamilies()
        {
            List<JObject> dataSet = await LoadTypeAsync("Autodesk.Revit.DB.FamilySymbol", "Family");
            Families = dataSet.Select(x => x.ToObject<Family>()).ToList();
        }

        public async Task LoadWalls()
        {
            List<JObject> dataSet = await LoadTypeAsync("Autodesk.Revit.DB.Wall", "Wall");
            Walls = dataSet.Select(x => x.ToObject<Wall>()).ToList();
        }

        public async Task LoadFloors()
        {
            List<JObject> dataSet = await LoadTypeAsync("Autodesk.Revit.DB.Floor", "Floor");
            Floors = dataSet.Select(x => x.ToObject<Floor>()).ToList();
        }

        public async Task LoadCeilings()
        {
            List<JObject> dataSet = await LoadTypeAsync("Autodesk.Revit.DB.Ceiling", "Ceiling");
            Ceilings = dataSet.Select(x => x.ToObject<Ceiling>()).ToList();
        }

        public async Task LoadFamilyInstances()
        {
            List<JObject> dataSet = await LoadTypeAsync("Autodesk.Revit.DB.FamilyInstance", "FamilyInstance");
            FamilyInstances = dataSet.Select(x => x.ToObject<FamilyInstance>()).ToList();
        }

        public View3D GetStartingOrientation()
        {
            Message response = this.ServerRequest(new Message
            {
                Type = "GET_ORIENTATION",
            });
            JObject responseData = JObject.Parse(response.Data);
            return responseData.ToObject<View3D>();
        }

        #endregion

        #region Mutators

        public IEnumerator SaveFamilyInstance(FamilyController behaviour, FamilyInstance oldData)
        {
            if (behaviour.GetInstanceData().HostId == oldData.HostId)
            {
                yield return UpdateFamilyInstance(behaviour, oldData);
            }
            else
            {
                yield return RecreateFamilyInstance(behaviour, oldData);
            }
        }

        public IEnumerator UpdateFamilyInstance(FamilyController behaviour, FamilyInstance oldData)
        {
            FamilyInstance fam = behaviour.GetInstanceData();

            Debug.Log($"Saving {fam.Id} {fam.Name}");
            Debug.Log($"Saving {fam.Id} {fam.Name}");

            CoroutineWithData<Message> cd = new CoroutineWithData<Message>(
                behaviour,
                this.ServerRequestCoroutine(new Message
                {
                    Type = "SET",
                    Data = JsonConvert.SerializeObject(fam)
                })
            );
            yield return cd.coroutine;
            Message response = cd.result;

            if (response == null)
            {
                Debug.LogError("FAILED TO UPDATE FAMILY INSTANCE");
                behaviour.UpdateInstanceData(oldData);
                yield break;
            }

            FamilyInstance newFamily = JObject.Parse(response.Data).ToObject<FamilyInstance>();

            Debug.Log(JsonConvert.SerializeObject(response));

            behaviour.UpdateInstanceData(newFamily);

            if (fam.HostId != null)
            {
                yield return UpdateHostGeometry(behaviour, fam.HostId);
            }

            foreach (string subComponent in fam.SubComponents)
            {
                behaviour.StartCoroutine(UpdateSubFamilies(behaviour, subComponent));
            }
        }

        public IEnumerator RecreateFamilyInstance(FamilyController behaviour, FamilyInstance oldData)
        {
            FamilyInstance fam = behaviour.GetInstanceData();

            Debug.LogError($"UPDATING HOST ID TO {fam.HostId}");
            behaviour.UpdateInstanceData(oldData);

            yield break;
        }

        private IEnumerator UpdateHostGeometry(FamilyController behaviour, string hostId)
        {
            CoroutineWithData<Message> cd = new CoroutineWithData<Message>(
                behaviour,
                this.ServerRequestCoroutine(new Message
                {
                    Type = "GET",
                    Data = JsonConvert.SerializeObject(new
                    {
                        Id = hostId
                    })
                })
            );
            yield return cd.coroutine;

            Message getResponse = cd.result;

            GeometryElement geo = JObject.Parse(getResponse.Data).ToObject<GeometryElement>();
            GameObject obj = GeometryLibrary.GetObject(geo.Id);
            Helpers.MeshGenerator.ResetFaceMeshes(geo, obj);
        }

        private IEnumerator UpdateSubFamilies(FamilyController behaviour, string relatedFamilyId)
        {
            GameObject famObj = FamilyInstanceLibrary.GetFamily(relatedFamilyId);

            famObj.SetActive(false);

            CoroutineWithData<Message> cd = new CoroutineWithData<Message>(
                behaviour,
                this.ServerRequestCoroutine(new Message
                {
                    Type = "GET",
                    Data = JsonConvert.SerializeObject(new
                    {
                        Id = relatedFamilyId
                    })
                })
            );
            yield return cd.coroutine;

            Message getResponse = cd.result;

            FamilyInstance fam = JObject.Parse(getResponse.Data).ToObject<FamilyInstance>();
            famObj.GetComponent<FamilyController>().UpdateInstanceData(fam);

            famObj.SetActive(true);
        }

        public FamilyInstance PlaceFamilyInstance(FamilyInstance fam)
        {
            Debug.Log($"Placing {fam.FamilyId}");

            Message response = this.ServerRequest(new Message
            {
                Type = "CREATE",
                Data = JsonConvert.SerializeObject(fam)
            });

            Debug.Log(JsonConvert.SerializeObject(response));

            return JObject.Parse(response.Data).ToObject<FamilyInstance>();
        }

        public IEnumerator DeleteFamilyInstance(FamilyInstance fam)
        {
            Debug.Log($"Deleting {fam.FamilyId}");

            yield return this.ServerRequestCoroutine(new Message
            {
                Type = "DELETE",
                Data = JsonConvert.SerializeObject(fam)
            });
        }

        public void PaintFace(Face newFace)
        {
            Debug.Log($"Updating material {newFace.ElementId} {newFace.FaceIndex} {newFace.MaterialId}");

            Message response = this.ServerRequest(new Message
            {
                Type = "PAINT",
                Data = JsonConvert.SerializeObject(newFace)
            });

            Debug.Log(JsonConvert.SerializeObject(response));
        }

        #endregion

        #region Helpers

        private async Task<List<JObject>> LoadTypeAsync(string type, string name)
        {
            Debug.Log($"Getting {name}s...");

            try
            {
                Message response = await this.ServerRequestAsync(new Message
                {
                    Type = "GET_ALL",
                    Data = JsonConvert.SerializeObject(new
                    {
                        Type = type
                    })
                });

                Debug.Log($"Got {name}s!");
                Debug.Log(JsonConvert.SerializeObject(response));

                if (response.Type == "ERROR")
                {
                    Debug.LogError(response.Data);
                    return new List<JObject>();
                }

                List<JObject> objects = JArray.Parse(response.Data).Select(x => (JObject)x).ToList();

                List<JObject> errors = objects.Where(o => o["ERROR"] != null).ToList();
                foreach (var e in errors)
                {
                    Debug.LogWarning(e);
                }

                return objects.Where(o => o["ERROR"] == null).ToList();
            }
            catch (NATS.Client.NATSTimeoutException e)
            {
                Debug.Log("Timeout Exception");
                Debug.LogWarning(e);
            }
            catch (Exception e)
            {
                Debug.Log($"Error {e.Message}");
                Debug.LogError(e);
            }

            return new List<JObject>();
        }

        public Message ServerRequest(Message msg)
        {
            return this.comms.RequestSync(this.comms.TO_SERVER_CHANNEL, msg, 5000);
        }

        public async Task<Message> ServerRequestAsync(Message msg)
        {
            return await this.comms.Request(this.comms.TO_SERVER_CHANNEL, msg, 5000);
        }

        public IEnumerator ServerRequestCoroutine(Message msg)
        {
            Task<Message> requestTask = this.comms.Request(this.comms.TO_SERVER_CHANNEL, msg, 5000);

            int totalIteration = 0;
            while(true)
            {
                if (requestTask.IsCompleted || requestTask.IsFaulted || requestTask.IsCanceled)
                {
                    break;
                }

                yield return new WaitForSeconds(0.1f);

                totalIteration++;

                Debug.Log($"Waiting for response {totalIteration}");

                if (totalIteration > 100)
                {
                    break;
                }
            }

            Debug.Log($"Result {requestTask.IsCompleted} {requestTask.IsFaulted} {requestTask.IsCanceled}");

            if (totalIteration <= 100)
            {
                yield return requestTask.Result;
            }
            else
            {
                Debug.LogError("TIMED OUT WAITING FOR REVIT RESPONSE");
                yield return null;
            }
        }

        #endregion
    }
}
