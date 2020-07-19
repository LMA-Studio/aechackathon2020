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

using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Common;
using System.Threading.Tasks;

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

            Task.Run(StreamVR.Instance.LoadAllAsync);
        }
        
        private void Update()
        {
            if (waiting)
            {
                if (StreamVR.Instance.IsLoaded)
                {
                    try
                    {
                        System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                        s.Start();

                        MaterialLibrary.LoadMaterials(StreamVR.Instance.Materials);
                        FamilyLibrary.LoadFamilies(StreamVR.Instance.Families);
                        this.GetComponent<WallPlacer>().Place(StreamVR.Instance.Walls);
                        this.GetComponent<FloorPlacer>().Place(StreamVR.Instance.Floors);
                        this.GetComponent<CeilingPlacer>().Place(StreamVR.Instance.Ceilings);
                        this.GetComponent<FamilyPlacer>().Place(StreamVR.Instance.FamilyInstances);

                        Debug.Log($"StreamVR Initial load in: {s.ElapsedMilliseconds}ms");
                        s.Stop();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    waiting = false;
                }
            }
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
            Debug.Log(response);
        }
#endif
    }
}
