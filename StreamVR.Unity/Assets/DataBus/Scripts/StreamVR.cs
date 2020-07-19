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
            var view = this.GetStartingOrientation();
            Debug.Log(JsonConvert.SerializeObject(view));
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

        public void SaveFamilyInstance(FamilyInstance fam)
        {
            Debug.Log($"Saving {fam.Id} {fam.Name}");
            Debug.Log($"Saving {fam.Id} {fam.Name}");

            Message response = this.ServerRequest(new Message
            {
                Type = "SET",
                Data = JsonConvert.SerializeObject(fam)
            });

            Debug.Log(JsonConvert.SerializeObject(response));

            if (fam.HostId != null)
            {
                Message getResponse = this.ServerRequest(new Message
                {
                    Type = "GET",
                    Data = JsonConvert.SerializeObject(new
                    {
                        Id = fam.HostId
                    })
                });

                GeometryElement geo = JObject.Parse(getResponse.Data).ToObject<GeometryElement>();
                GameObject obj = GeometryLibrary.GetObject(geo.Id);
                Helpers.MeshGenerator.ResetFaceMeshes(geo, obj);
            }
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

        #endregion
    }
}