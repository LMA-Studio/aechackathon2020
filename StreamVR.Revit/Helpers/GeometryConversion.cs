﻿/*
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
using System.Linq;
using Autodesk.Revit.DB;

namespace LMAStudio.StreamVR.Revit.Helpers
{
    public class GeometryConversion
    {
        public static List<LMAStudio.StreamVR.Common.Models.Face> ConvertToDTO(Autodesk.Revit.DB.Element parent, Autodesk.Revit.DB.Solid geometry)
        {
            List<LMAStudio.StreamVR.Common.Models.Face> wallFaces = new List<LMAStudio.StreamVR.Common.Models.Face>();

            IEnumerable<Face> faces = geometry.Faces.Cast<Face>();

            Face topFace = faces.Where(
                (f, i) => i == 1
            ).FirstOrDefault() ?? faces.FirstOrDefault();

            wallFaces = faces.Select((f, index) =>
            {
                Mesh m = f.Triangulate();

                List<int> indices = new List<int>();

                for (int i = 0; i < m.NumTriangles; i++)
                {
                    MeshTriangle mt = m.get_Triangle(i);
                    for (int j = 0; j < 3; j++)
                    {
                        indices.Add((int)mt.get_Index(j));
                    }
                }

                return new LMAStudio.StreamVR.Common.Models.Face
                {
                    ElementId = parent.Id.ToString(),
                    FaceIndex = index,
                    MaterialId = f.MaterialElementId?.ToString(),
                    Indices = indices,
                    Vertices = m.Vertices.Select(v => new LMAStudio.StreamVR.Common.Models.XYZ
                    {
                        X = v.X,
                        Y = v.Y,
                        Z = v.Z,
                    }).ToList()
                };
            }).ToList();

            return wallFaces;
        }
    }
}
