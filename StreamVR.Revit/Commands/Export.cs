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

using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using LMAStudio.StreamVR.Revit.Conversions;
using System;
using System.Linq;
using LMAStudio.StreamVR.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LMAStudio.StreamVR.Revit.Commands
{
    public class Export: IBaseCommand
    {
        private readonly Action<string> _log;
        private readonly IGenericConverter _converter;

        public Export(Action<string> log, IGenericConverter converter)
        {
            _log = log;
            _converter = converter;
        }

        public Message Execute(Document doc, Message msg)
        {
            JObject msgData = JObject.Parse(msg.Data);
            string elementId = msgData["Id"].ToString();

            FamilySymbol family = doc.GetElement(new ElementId(Int32.Parse(elementId))) as FamilySymbol;

            int indexOffset = 0;
            int nextIndexOffset = 0;

            StringBuilder fullObjectSB = new StringBuilder();
            int part = 0;

            fullObjectSB.Append($"o GeometryPart{part}\n");

            GeometryElement geometry = family.get_Geometry(new Options());
            foreach (GeometryObject obj in geometry)
            {
                Solid solid = obj as Solid;
                if (solid == null)
                {
                    continue;
                }

                int faceNum = 0;

                IEnumerable<Face> faces = solid.Faces.Cast<Face>();
                foreach (Face f in faces)
                {
                    fullObjectSB.Append($"g GeometryFace{faceNum}\n");

                    Mesh m = f.Triangulate();

                    foreach (XYZ v in m.Vertices)
                    {
                        fullObjectSB.Append($"v {v.X} {v.Z} {v.Y}\n");
                    }
                    for (int i = 0; i < m.NumTriangles; i++)
                    {
                        MeshTriangle mt = m.get_Triangle(i);
                        int f1 = (int)mt.get_Index(0) + indexOffset;
                        int f2 = (int)mt.get_Index(1) + indexOffset;
                        int f3 = (int)mt.get_Index(2) + indexOffset;
                        fullObjectSB.Append($"f {f1 + 1} {f2 + 1} {f3 + 1}\n");

                        if (f1 > nextIndexOffset)
                        {
                            nextIndexOffset = f1;
                        }
                        if (f2 > nextIndexOffset)
                        {
                            nextIndexOffset = f2;
                        }
                        if (f3 > nextIndexOffset)
                        {
                            nextIndexOffset = f3;
                        }
                    }
                    faceNum++;

                    indexOffset = nextIndexOffset + 1;
                }

                part++;
            }

            string path = Path.GetTempPath() + "\\StreamVR";
            Directory.CreateDirectory(path);
            string filename = $"{path}\\StreamVRTest.obj";
            File.WriteAllText(filename, fullObjectSB.ToString());

            return new Message
            {
                Type = "OBJ",
                Data = filename
            };
        }
    }
}
