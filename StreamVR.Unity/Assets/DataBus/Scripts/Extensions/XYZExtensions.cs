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

namespace LMAStudio.StreamVR.Unity.Extensions
{
    public static class XYZExtensions
    {
        public static void SetPosition(this UnityEngine.Transform got, Common.Models.Transform t)
        {
            XYZ originXYZ = t.Origin;
            Vector3 origin = new Vector3(
                (float)originXYZ.X * Helpers.Constants.M_PER_FT,
                (float)originXYZ.Z * Helpers.Constants.M_PER_FT,
                (float)originXYZ.Y * Helpers.Constants.M_PER_FT
            );

            Vector3 newForward = new Vector3(
                (float)t.BasisY.X,
                (float)t.BasisY.Z,
                (float)t.BasisY.Y
            );

            Vector3 newRight = new Vector3(
                (float)t.BasisX.X,
                (float)t.BasisX.Z,
                (float)t.BasisX.Y
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

            got.position = origin;
            got.rotation = Quaternion.LookRotation(
                newForward,
                newUp
            );
        }

        public static void SetRotation(this Common.Models.Transform t, UnityEngine.Transform got)
        {
            t.BasisX = new XYZ
            {
                X = got.right.x,
                Y = got.right.z,
                Z = got.right.y,
            };
            t.BasisY = new XYZ
            {
                X = got.forward.x,
                Y = got.forward.z,
                Z = got.forward.y,
            };
            t.BasisZ = new XYZ
            {
                X = got.up.x,
                Y = got.up.z,
                Z = got.up.y,
            };
        }
    }
}
