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
using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;

namespace LMAStudio.StreamVR.Revit.Conversions
{
    public class View3DConverter: IConverter<Autodesk.Revit.DB.View3D>
    {
        public JObject ConvertToDTO(Autodesk.Revit.DB.View3D source)
        {
            var orientation = source.GetOrientation();

            LMAStudio.StreamVR.Common.Models.View3D dest = new LMAStudio.StreamVR.Common.Models.View3D
            {
                Id = source.Id.ToString(),
                Name = source.Name,
                Position = new LMAStudio.StreamVR.Common.Models.XYZ
                {
                    X = orientation.EyePosition.X,
                    Y = orientation.EyePosition.Y,
                    Z = orientation.EyePosition.Z,
                },
                ForwardDirection = new LMAStudio.StreamVR.Common.Models.XYZ
                {
                    X = orientation.ForwardDirection.X,
                    Y = orientation.ForwardDirection.Y,
                    Z = orientation.ForwardDirection.Z,
                },
                UpDirection = new LMAStudio.StreamVR.Common.Models.XYZ
                {
                    X = orientation.UpDirection.X,
                    Y = orientation.UpDirection.Y,
                    Z = orientation.UpDirection.Z,
                },
            };

            return JObject.FromObject(dest);
        }

        public void MapFromDTO(JObject sourceJSON, Autodesk.Revit.DB.View3D dest)
        {
            throw new NotImplementedException();
        }

        public Autodesk.Revit.DB.View3D CreateFromDTO(Document doc, JObject source)
        {
            throw new NotImplementedException();
        }
    }
}
