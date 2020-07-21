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
            GameObject newFamily = new GameObject();

            newFamily.layer = Helpers.Constants.LAYER_FAMILY;
            newFamily.transform.SetPosition(f.Transform);
            newFamily.transform.parent = this.transform;
            newFamily.AddComponent<FamilyController>().LoadInstanceAsync(f);

            FamilyInstanceLibrary.AddFamily(f, newFamily);
        }
    }
}
