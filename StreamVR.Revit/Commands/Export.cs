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
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LMAStudio.StreamVR.Revit.Commands
{
    public class Export: IBaseCommand
    {
        private readonly Action<string> _log;
        private readonly IGenericConverter _converter;
        private readonly string _modelServerUrl;

        public Export(Action<string> log, IGenericConverter converter, string modelServerUrl)
        {
            _log = log;
            _converter = converter;
            _modelServerUrl = modelServerUrl;
        }

        public Message Execute(Document doc, Message msg)
        {
            JObject msgData = JObject.Parse(msg.Data);
            string elementId = msgData["Id"].ToString();

            FamilySymbol family = doc.GetElement(new ElementId(Int32.Parse(elementId))) as FamilySymbol;

            GeometryElement geometry = family.get_Geometry(new Options());

            if (geometry == null)
            {
                return new Message
                {
                    Type = "EMPTY",
                    Data = $"No geometry found for family: {family.Name} ({family.Id})"
                };
            }

            byte[] file_bytes = GeometryToOBJ(doc, geometry);

            try
            {
                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                s.Start();

                List<Task> uploadTasks = new List<Task>();

                LMAStudio.StreamVR.Common.Models.Family dto = _converter.ConvertToDTO(family).ToObject<LMAStudio.StreamVR.Common.Models.Family>();

                uploadTasks.Add(Task.Run(() => UploadOBJ(dto, file_bytes)));

                IEnumerable<FamilyInstance> instances = new FilteredElementCollector(doc).
                    OfClass(typeof(FamilyInstance)).
                    Select(f => f as FamilyInstance).
                    Where(f => f.Symbol.Id == family.Id);

                // Get all different variants
                Dictionary<int, FamilyInstance> variants = new Dictionary<int, FamilyInstance>();
                foreach (var f in instances)
                {
                    var info = f.GetOrderedParameters().Where(p => p.Definition.ParameterGroup == BuiltInParameterGroup.PG_GEOMETRY);
                    Dictionary<string, string> infoDict = new Dictionary<string, string>();
                    foreach(var i in info)
                    {
                        infoDict[i.Definition.Name] = i.AsValueString();
                    }
                    int hash = JsonConvert.SerializeObject(infoDict).GetHashCode();
                    variants[hash] = f;
                }

                _log($"{family.Name} has {instances.Count()} varaints");

                // Upload each distinct variant
                foreach (var kv in variants)
                {
                    GeometryElement variantGeometry = kv.Value.get_Geometry(new Options());
                    GeometryInstance variantInstance = variantGeometry.Where(
                        g => (g as Solid) == null || (g as Solid).Faces.Size == 0
                    ).FirstOrDefault() as GeometryInstance;
                    if (variantInstance == null)
                    {
                        _log($"INSTANCE GEOMETRY NULL FOR: {kv.Value.Name}");
                        continue;
                    }
                    GeometryElement variantInstanceGeometry = variantInstance.GetSymbolGeometry();
                    if (variantInstanceGeometry != null)
                    {
                        byte[] variantFileBytes = GeometryToOBJ(doc, variantInstanceGeometry);
                        uploadTasks.Add(Task.Run(() => UploadOBJVariant(dto.FamilyId, kv.Key.ToString(), variantFileBytes)));
                    }
                }

                Task.WaitAll(uploadTasks.ToArray());

                _log($"Upload time for {instances.Count() + 1} models: {s.ElapsedMilliseconds}ms");
                s.Stop();

                return new Message
                {
                    Type = "OBJ",
                    Data = dto.FamilyId
                };
            }
            catch (Exception e)
            {

                return new Message
                {
                    Type = "ERROR",
                    Data = $"Error: {family.Name} ({family.Id}) {e.ToString()}"
                };
            }
        }

        private byte[] GeometryToOBJ(Document doc, GeometryElement geometry)
        {
            int indexOffset = 0;
            int nextIndexOffset = 0;
            int part = 0;

            StringBuilder fullObjectSB = new StringBuilder();

            fullObjectSB.Append($"o StreamVRExportedObject\n");

            foreach (GeometryObject obj in geometry)
            {
                Solid solid = obj as Solid;
                if (solid == null)
                {
                    continue;
                }

                if (solid.GraphicsStyleId != null)
                {
                    GraphicsStyle gs = doc.GetElement(solid.GraphicsStyleId) as GraphicsStyle;
                    bool isLightSource = gs?.GraphicsStyleCategory?.Name == "Light Source";
                    if (isLightSource)
                    {
                        XYZ centroid = solid.GetBoundingBox()?.Transform?.Origin;
                        if (centroid != null)
                        {
                            fullObjectSB.Append($"ls {centroid.X} {centroid.Z} {centroid.Y}\n");
                        }
                        continue;
                    }
                }

                fullObjectSB.Append($"g GeometryPart{part}\n");

                bool addedMaterial = false;
                IEnumerable<Face> faces = solid.Faces.Cast<Face>();
                foreach (Face f in faces)
                {
                    if (!addedMaterial && f.MaterialElementId != null)
                    {
                        fullObjectSB.Append($"svrm {f.MaterialElementId.ToString()}\n");
                        addedMaterial = true;
                    }

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

                    indexOffset = nextIndexOffset + 1;
                }

                part++;
            }

            byte[] file_bytes = Encoding.ASCII.GetBytes(fullObjectSB.ToString());

            return file_bytes;
        }

        private void UploadOBJ(LMAStudio.StreamVR.Common.Models.Family dto, byte[] file_bytes)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StringContent(dto.Name ?? ""), "symbolName");
                form.Add(new StringContent(dto.FamilyName ?? ""), "familyName");
                form.Add(new StringContent(dto.ModelName ?? ""), "modelName");
                form.Add(new StringContent(dto.Manufacturer ?? ""), "publisher");
                form.Add(new StringContent(dto.Description ?? ""), "description");
                form.Add(new StringContent(dto.Tag ?? ""), "tag");
                form.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), "file", $"{dto.FamilyId}.obj");

                string url = $"{_modelServerUrl}/api/model/{dto.FamilyId}";
                _log("Uploading model to: " + url);
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;

                response.EnsureSuccessStatusCode();

                string sd = response.Content.ReadAsStringAsync().Result;
            }
        }

        private void UploadOBJVariant(string familyId, string varaintId, byte[] file_bytes)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), "file", $"{varaintId}.obj");

                string url = $"{_modelServerUrl}/api/model/{familyId}?v={varaintId ?? ""}";
                _log("Uploading model variant to: " + url);
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;

                response.EnsureSuccessStatusCode();

                string sd = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
