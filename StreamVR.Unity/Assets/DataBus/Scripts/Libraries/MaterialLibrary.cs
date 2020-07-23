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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMAStudio.StreamVR.Common.Models;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace LMAStudio.StreamVR.Unity.Logic
{
    public static class MaterialLibrary
    {
        private static Dictionary<string, Material> lib = new Dictionary<string, Material>();
        private static Dictionary<string, UnityEngine.Material> matLib = new Dictionary<string, UnityEngine.Material>();

        public static void LoadMaterials(List<Material> materials)
        {
            lib = materials.ToDictionary(
                kv => kv.Id,
                kv => kv
            );

            matLib = materials.ToDictionary(
                kv => kv.Id,
                kv =>
                {
                    var mat = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit"));
                    mat.name = kv.Name;
                    mat.color = new UnityEngine.Color(
                        kv.Color.Red / 255f,
                        kv.Color.Green / 255f,
                        kv.Color.Blue / 255f,
                        (100 - kv.Transparency) / 100f
                    );
                    if (kv.Transparency > 0)
                    {
                        mat.SetFloat("_Surface", 1);
                        mat.SetFloat("_Blend", 0);
                        mat.SetOverrideTag("RenderType", "Transparent");
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        mat.SetShaderPassEnabled("ShadowCaster", false);
                    }
                    mat.SetFloat("_Metallic", (float)kv.Shininess / byte.MaxValue);
                    mat.SetFloat("_Smoothness", (float)kv.Smoothness / byte.MaxValue);
                    return mat;
                }
            );
        }
        
        public static IEnumerable<Material> GetAllMaterials()
        {
            return lib.Values;
        }

        public static Material GetMaterial(string id)
        {
            if (id == null)
            {
                return null;
            }
            if (!lib.ContainsKey(id))
            {
                return null;
            }
            return lib[id];
        }

        public static Material ReverseGetMaterial(string name)
        {
            if (name == null)
            {
                return null;
            }
            foreach(var m in lib)
            {
                if (m.Value.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return m.Value;
                }
            }
            return null;
        }

        public static UnityEngine.Material LookupMaterial(string id)
        {
            if (id == null)
            {
                return null;
            }
            if (!matLib.ContainsKey(id))
            {
                return null;
            }
            return matLib[id];
        }

        public static UnityEngine.Material ReverseLookupMaterial(string name)
        {
            var mat = MaterialLibrary.ReverseGetMaterial(name);
            if (mat == null)
            {
                return null;
            }
            return (UnityEngine.Material)UnityEngine.Resources.Load($"Materials/{mat.Name}/{mat.Name}");
        }


        private static int _totalCalls = 0;

        public static IEnumerator DownloadAllTextures(UnityEngine.MonoBehaviour parent)
        {
            foreach(var mat in lib.Keys)
            {
                _totalCalls++;
                parent.StartCoroutine(ResolveMaterialTexture(mat));
            }

            while (_totalCalls > 0)
                yield return null;
        }

        private static IEnumerator ResolveMaterialTexture(string id)
        {
            Common.Models.Material mat = GetMaterial(id);
            if (mat == null)
            {
                UnityEngine.Debug.Log($"MATERIAL {id} DOES NOT EXIST");
                _totalCalls--;
                yield return null;
                yield break;
            }

            string url = $"http://192.168.0.119:5000/api/material/{mat.Id}";
            UnityEngine.Texture materialTexture = null;

            UnityEngine.Debug.Log(url);
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    UnityEngine.Debug.Log("Error: " + webRequest.error);
                    yield return null;
                    _totalCalls--;
                    yield break;
                }
                else if (webRequest.isHttpError)
                {
                    // UnityEngine.Debug.Log("Error: " + webRequest.error);
                    yield return null;
                    _totalCalls--;
                    yield break;
                }
                else
                {
                    materialTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                }
            }

            if (materialTexture != null)
            {
                UnityEngine.Material baseMaterial = LookupMaterial(mat.Id);

                UnityEngine.Material newMaterial = new UnityEngine.Material(baseMaterial.shader);

                newMaterial.CopyPropertiesFromMaterial(baseMaterial);

                newMaterial.name = mat.Name + " (With Texture)";
                newMaterial.mainTexture = materialTexture;
                newMaterial.color = new UnityEngine.Color(
                    220f / 255f,
                    220f / 255f,
                    220f / 255f
                );
                newMaterial.SetFloat("_Metallic", (float)mat.Shininess / byte.MaxValue);
                newMaterial.SetFloat("_Smoothness", (float)mat.Smoothness / byte.MaxValue);

                matLib.Add("_" + id, newMaterial);

                UnityEngine.Debug.Log(JsonConvert.SerializeObject(mat));

                // UnityEngine.GameObject.Find("Test").GetComponent<UnityEngine.MeshRenderer>().material = newMaterial;
            }

            _totalCalls--;
        }
    }
}
