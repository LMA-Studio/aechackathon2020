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
using System.IO;
using System.Net.Http;
using Autodesk.Revit.DB.Visual;
using System.Collections.Generic;

namespace LMAStudio.StreamVR.Revit.Commands
{
    public class ExportMaterial: IBaseCommand
    {
        private readonly Action<string> _log;
        private readonly IGenericConverter _converter;
        private readonly string _modelServerUrl;

        private readonly List<string> textureProperties = new List<string>
        {
            "hardwood_color",
            "opaque_albedo"
        };

        public ExportMaterial(Action<string> log, IGenericConverter converter, string modelServerUrl)
        {
            _log = log;
            _converter = converter;
            _modelServerUrl = modelServerUrl;
        }

        public Message Execute(Document doc, Message msg)
        {
            JObject msgData = JObject.Parse(msg.Data);
            string elementId = msgData["Id"].ToString();

            Material mat = doc.GetElement(new ElementId(Int32.Parse(elementId))) as Material;

            try
            {
                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                s.Start();

                LMAStudio.StreamVR.Common.Models.Material dto = _converter.ConvertToDTO(mat).ToObject<LMAStudio.StreamVR.Common.Models.Material>();

                AppearanceAssetElement assetElement = doc.GetElement(mat.AppearanceAssetId) as AppearanceAssetElement;
                Asset asset = assetElement?.GetRenderingAsset();

                if (asset == null)
                {
                    return new Message
                    {
                        Type = "EMPTY",
                        Data = $"No material asset found for: {mat.Name} ({mat.Id})"
                    };
                }

                string bmpPathFull = "";

                for (int i = 0; i < asset.Size; i++)
                {
                    AssetProperty prop = asset.Get(i);
                    if (textureProperties.Contains(prop.Name) && prop.NumberOfConnectedProperties > 0)
                    {
                        Asset connectedAsset = prop.GetSingleConnectedAsset();
                        for (int j = 0; j < connectedAsset.Size; j++)
                        {
                            AssetProperty prop2 = connectedAsset.Get(j);
                            if (prop2.Name == "unifiedbitmap_Bitmap")
                            {
                                bmpPathFull = (prop2 as AssetPropertyString).Value;
                            }
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(bmpPathFull))
                {
                    return new Message
                    {
                        Type = "EMPTY",
                        Data = $"No exportable material found for: {mat.Name} ({mat.Id})"
                    };
                }

                string texturesDirectory = "C:\\Program Files (x86)\\Common Files\\Autodesk Shared\\Materials\\Textures";
                string bmpPath = bmpPathFull.Split('|').LastOrDefault();

                string fullPath = texturesDirectory + "\\" + bmpPath;
                string materialFileName = new FileInfo(fullPath).Name;

                byte[] materialAlbedo = File.ReadAllBytes(fullPath);

                UploadOBJ(dto, materialAlbedo, materialFileName);

                _log($"Export/Upload time for {1} materials: {s.ElapsedMilliseconds}ms");
                s.Stop();

                return new Message
                {
                    Type = "MAT",
                    Data = mat.Id.ToString()
                };
            }
            catch (Exception e)
            {
                return new Message
                {
                    Type = "ERROR",
                    Data = $"Error: {mat.Name} ({mat.Id}) {e.ToString()}"
                };
            }
        }

        private void UploadOBJ(LMAStudio.StreamVR.Common.Models.Material dto, byte[] file_bytes, string filename)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StringContent(dto.Name ?? ""), "materialName");
                form.Add(new StringContent(filename ?? ""), "fileName");
                form.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), "file", filename);

                string url = $"{_modelServerUrl}/api/material/{dto.Id}";
                _log("Uploading material to: " + url);
                HttpResponseMessage response = httpClient.PostAsync(url, form).Result;

                response.EnsureSuccessStatusCode();

                string sd = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
