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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using LMAStudio.StreamVR.Common.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace LMAStudio.StreamVR.Unity.Logic
{
    public static class FamilyLibrary
    {
        private static string _ModelServerURL = "";

        private static Dictionary<string, Family> lib = new Dictionary<string, Family>();
        private static IEnumerable<string> tags = new List<string>();

        public static void ConfigureModelServerURL (string url)
        {
            _ModelServerURL = url;
        }

        public static void LoadFamilies(List<Family> families)
        {
            lib = families.ToDictionary(
                kv => kv.Id,
                kv => kv
            );
            tags = families.Select(f => f.Tag).Distinct();
        }

        public static IEnumerable<string> GetTags()
        {
            return tags;
        }

        public static IEnumerable<Family> GetFamiliesForTag(string tag)
        {
            return lib.Values.Where(v => v.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Family> GetAllFamilies()
        {
            return lib.Values;
        }

        public static Family GetFamily(string id)
        {
            if (!lib.ContainsKey(id))
            {
                return null;
            }
            return lib[id];
        }

        public static Family ReverseGetFamily(string name)
        {
            foreach(var fam in lib)
            {
                if (fam.Value.Name == name)
                {
                    return fam.Value;
                }
            }
            return null;
        }

        public static IEnumerator ResolveFamilyOBJ(string id, string variantId)
        {
            Family fam = GetFamily(id);
            if (fam == null)
            {
                Debug.Log($"FAMILY {id} DOES NOT EXIST");
                yield return null;
                yield break;
            }

            string url = $"{_ModelServerURL }/api/model/{fam.FamilyId}?v={variantId ?? ""}";

            Debug.Log(url);
            using (var webRequest = new UnityWebRequest(url))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                    yield return null;
                }
                else
                {
                    yield return webRequest.downloadHandler.data;
                }
            }
        }
    }
}
