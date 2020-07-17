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

using System.Collections.Generic;

using UnityEngine;

using LMAStudio.StreamVR.Common.Models;
using LMAStudio.StreamVR.Unity.Helpers;
using LMAStudio.StreamVR.Unity.Extensions;
using LMAStudio.StreamVR.Unity.Scripts;

namespace LMAStudio.StreamVR.Unity.Logic
{
    public class FamilyPlacer : MonoBehaviour
    {
        public void Place(List<FamilyInstance> families)
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();

            foreach (var f in families)
            {
                this.Place(f);
            }

            Debug.Log($"OBJ import time: {s.ElapsedMilliseconds}ms");
            s.Stop();
        }

        public void Place(FamilyInstance f)
        {
            XYZ originXYZ = f.Transform.Origin;
            Vector3 origin = new Vector3(
                (float)originXYZ.X * Helpers.Constants.M_PER_FT,
                (float)originXYZ.Z * Helpers.Constants.M_PER_FT,
                (float)originXYZ.Y * Helpers.Constants.M_PER_FT
            );

            GameObject newFamily = new GameObject();
            newFamily.transform.position = origin;
            newFamily.transform.rotation = GenerateRotation(f);

            newFamily.transform.parent = this.transform;
            newFamily.AddComponent<FamilyController>().LoadInstanceAsync(f);
        }

        private Quaternion GenerateRotation(FamilyInstance f)
        {
            Vector3 newForward = new Vector3(
                (float)f.Transform.BasisY.X,
                (float)f.Transform.BasisY.Z,
                (float)f.Transform.BasisY.Y
            );

            Vector3 newRight = new Vector3(
                (float)f.Transform.BasisX.X,
                (float)f.Transform.BasisX.Z,
                (float)f.Transform.BasisX.Y
            );
            //if (f.IsHandFlipped)
            //{
            //    newRight = -newRight;
            //}

            //if (f.IsFlipped)
            //{
            //    newForward = -newForward;
            //    newRight = -newRight;
            //}

            Vector3 newUp = Vector3.Cross(newForward, newRight);

            return Quaternion.LookRotation(
                newForward,
                newUp
            );
        }
    }
}
