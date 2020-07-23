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

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    [RequireComponent(typeof(WallPlacer))]
    [RequireComponent(typeof(FloorPlacer))]
    [RequireComponent(typeof(CeilingPlacer))]
    [RequireComponent(typeof(FamilyPlacer))]
    public class StreamVRController : MonoBehaviour
    {
        public bool LoadMaterials = true;
        public bool LoadFamilies = true;
        public bool LoadWalls = true;
        public bool LoadFloors = true;
        public bool LoadCeilings = true;
        public bool LoadFamilyInstances = true;

        public GameObject Character;

        private bool waiting = true;

        private void Start()
        {
            StreamVR.Instance.Connect(new StreamVROptions
            {
                LoadMaterials = LoadMaterials,
                LoadFamilies = LoadFamilies,
                LoadWalls = LoadWalls,
                LoadFloors = LoadFloors,
                LoadCeilings = LoadCeilings,
                LoadFamilyInstances = LoadFamilyInstances
            });

            if (Character != null)
            {
                View3D so = StreamVR.Instance.GetStartingOrientation();
                Character.transform.position = new Vector3(
                    (float)so.Position.X * Helpers.Constants.M_PER_FT,
                    (float)so.Position.Z * Helpers.Constants.M_PER_FT,
                    (float)so.Position.Y * Helpers.Constants.M_PER_FT
                );
                Character.transform.rotation = Quaternion.LookRotation(
                    new Vector3(
                        (float)so.ForwardDirection.X,
                        (float)so.ForwardDirection.Z,
                        (float)so.ForwardDirection.Y
                    ),
                    new Vector3(
                        (float)so.UpDirection.X,
                        (float)so.UpDirection.Z,
                        (float)so.UpDirection.Y
                    )
                );
            }

            Task.Run(StreamVR.Instance.LoadAllAsync);
        }
        
        private void Update()
        {
            if (waiting)
            {
                if (StreamVR.Instance.IsLoaded)
                {
                    StartCoroutine(LoadAll());
                    waiting = false;
                }
            }
        }

        private IEnumerator LoadAll()
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();

            if (StreamVR.Instance.Materials != null)
            {
                MaterialLibrary.LoadMaterials(StreamVR.Instance.Materials);
            }

            StartCoroutine(this.LoadSurfaces());
            StartCoroutine(this.LoadFamiles());

            yield return 0;

            Debug.Log($"StreamVR Initial load in: {s.ElapsedMilliseconds}ms");
            s.Stop();
        }

        private IEnumerator LoadSurfaces()
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();

            if (StreamVR.Instance.Materials != null)
            {
                yield return MaterialLibrary.DownloadAllTextures(this);
            }
            if (StreamVR.Instance.Walls != null)
            {
                this.GetComponent<WallPlacer>().Place(StreamVR.Instance.Walls);
            }
            if (StreamVR.Instance.Floors != null)
            {
                this.GetComponent<FloorPlacer>().Place(StreamVR.Instance.Floors);
            }
            if (StreamVR.Instance.Ceilings != null)
            {
                this.GetComponent<CeilingPlacer>().Place(StreamVR.Instance.Ceilings);
            }

            Debug.Log($"StreamVR Surface load in: {s.ElapsedMilliseconds}ms");
            s.Stop();
        }

        private IEnumerator LoadFamiles()
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();

            if (StreamVR.Instance.Families != null)
            {
                FamilyLibrary.LoadFamilies(StreamVR.Instance.Families);
            }

            if (StreamVR.Instance.FamilyInstances != null)
            {
                this.GetComponent<FamilyPlacer>().Place(StreamVR.Instance.FamilyInstances);
                FamilyLibrary.LoadFamilies(StreamVR.Instance.Families);
            }

            yield return 0;

            Debug.Log($"StreamVR Family load in: {s.ElapsedMilliseconds}ms");
            s.Stop();
        }

#if UNITY_EDITOR
        [MenuItem("StreamVR/Shutdown Server")]
        public static void ShutdownInterface()
        {
            var comms = new Communicator("192.168.0.119:7002", "lisamarie.mueller", "123456", Debug.Log);
            comms.Connect();
            comms.Publish(comms.TO_SERVER_CHANNEL, new Message { Type = "EXIT" });
        }

        [MenuItem("StreamVR/Export all")]
        public static void ExportAll()
        {
            var comms = new Communicator("192.168.0.119:7002", "lisamarie.mueller", "123456", Debug.Log);
            comms.Connect();
            Message response = comms.RequestSync(comms.TO_SERVER_CHANNEL, new Message { Type = "EXPORT_ALL" }, 30000);
            Debug.Log(JsonConvert.SerializeObject(response));
        }

        [MenuItem("StreamVR/Export all materials")]
        public static void ExportAllMaterials()
        {
            var comms = new Communicator("192.168.0.119:7002", "lisamarie.mueller", "123456", Debug.Log);
            comms.Connect();
            Message response = comms.RequestSync(comms.TO_SERVER_CHANNEL, new Message { Type = "EXPORT_ALL_MATERIALS" }, 30000);
            Debug.Log(JsonConvert.SerializeObject(response));
        }
#endif
    }
}
