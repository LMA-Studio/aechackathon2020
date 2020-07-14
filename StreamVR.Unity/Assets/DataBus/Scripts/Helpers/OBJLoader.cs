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
using System.IO;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LMAStudio.StreamVR.Unity.Helpers
{
    public class OBJLoader
    {
        //global lists, accessed by objobjectbuilder
        internal List<Vector3> Vertices = new List<Vector3>();
        internal List<Vector3> Normals = new List<Vector3>();
        internal List<Vector2> UVs = new List<Vector2>();

        //materials, accessed by objobjectbuilder
        internal Dictionary<string, Material> Materials;

        //file info for files loaded from file path, used for GameObject naming and MTL finding
        private FileInfo _objInfo;

#if UNITY_EDITOR
        [MenuItem("GameObject/Import From OBJ")]
        static void ObjLoadMenu()
        {
            string pth = EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
            if (!string.IsNullOrEmpty(pth))
            {
                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                s.Start();

                var loader = new OBJLoader();
                loader.Load(pth);

                Debug.Log($"OBJ import time: {s.ElapsedMilliseconds}ms");
                s.Stop();
            }
        }
#endif

        /// <summary>
        /// Load an OBJ file from a stream. No materials will be loaded, and will instead be supplemented by a blank white material.
        /// </summary>
        /// <param name="input">Input OBJ stream</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(Stream input)
        {
            var reader = new StreamReader(input);
            //var reader = new StringReader(inputReader.ReadToEnd());

            Dictionary<string, OBJObjectBuilder> builderDict = new Dictionary<string, OBJObjectBuilder>();
            OBJObjectBuilder currentBuilder = null;

            //lists for face data
            //prevents excess GC
            List<int> vertexIndices = new List<int>();
            List<int> normalIndices = new List<int>();
            List<int> uvIndices = new List<int>();

            //helper func
            Action<string> setCurrentObjectFunc = (string n) =>
            {
                if (!builderDict.TryGetValue(n, out currentBuilder))
                {
                    currentBuilder = new OBJObjectBuilder(n, this);
                    builderDict[n] = currentBuilder;
                }
            };

            //create default object
            setCurrentObjectFunc.Invoke("default");

            //var buffer = new DoubleBuffer(reader, 256 * 1024);
            var buffer = new CharWordReader(reader, 4 * 1024);
            string objectName = null;
            string materialId = null;

            //do the reading
            while (true)
            {
                buffer.SkipWhitespaces();

                if (buffer.endReached == true)
                {
                    break;
                }

                buffer.ReadUntilWhiteSpace();

                //comment or blank
                if (buffer.Is("#"))
                {
                    buffer.SkipUntilNewLine();
                    continue;
                }

                if (buffer.Is("v"))
                {
                    Vertices.Add(buffer.ReadVector());
                    continue;
                }

                //normal
                if (buffer.Is("vn"))
                {
                    Normals.Add(buffer.ReadVector());
                    continue;
                }

                //uv
                if (buffer.Is("vt"))
                {
                    UVs.Add(buffer.ReadVector());
                    continue;
                }

                // stream-vr material id
                if (buffer.Is("svrm"))
                {
                    buffer.ReadUntilNewLine();
                    materialId = buffer.GetString(1);
                    continue;
                }

                //new object
                if ((buffer.Is("o") || buffer.Is("g")))
                {
                    buffer.ReadUntilNewLine();
                    string groupingName = buffer.GetString(1);
                    setCurrentObjectFunc.Invoke(groupingName);

                    if (buffer.Is("o"))
                    {
                        objectName = groupingName;
                    }

                    continue;
                }

                //face data (the fun part)
                if (buffer.Is("f"))
                {
                    //loop through indices
                    while (true)
                    {
                        bool newLinePassed;
                        buffer.SkipWhitespaces(out newLinePassed);
                        if (newLinePassed == true)
                        {
                            break;
                        }

                        int vertexIndex = int.MinValue;
                        int normalIndex = int.MinValue;
                        int uvIndex = int.MinValue;

                        vertexIndex = buffer.ReadInt();
                        if (buffer.currentChar == '/')
                        {
                            buffer.MoveNext();
                            if (buffer.currentChar != '/')
                            {
                                uvIndex = buffer.ReadInt();
                            }
                            if (buffer.currentChar == '/')
                            {
                                buffer.MoveNext();
                                normalIndex = buffer.ReadInt();
                            }
                        }

                        //"postprocess" indices
                        if (vertexIndex > int.MinValue)
                        {
                            if (vertexIndex < 0)
                                vertexIndex = Vertices.Count - vertexIndex;
                            vertexIndex--;
                        }
                        if (normalIndex > int.MinValue)
                        {
                            if (normalIndex < 0)
                                normalIndex = Normals.Count - normalIndex;
                            normalIndex--;
                        }
                        if (uvIndex > int.MinValue)
                        {
                            if (uvIndex < 0)
                                uvIndex = UVs.Count - uvIndex;
                            uvIndex--;
                        }

                        //set array values
                        vertexIndices.Add(vertexIndex);
                        normalIndices.Add(normalIndex);
                        uvIndices.Add(uvIndex);
                    }

                    vertexIndices.Reverse();
                    normalIndices.Reverse();
                    uvIndices.Reverse();

                    //push to builder
                    currentBuilder.PushFace(vertexIndices, normalIndices, uvIndices, materialId);

                    //clear lists
                    vertexIndices.Clear();
                    normalIndices.Clear();
                    uvIndices.Clear();

                    continue;
                }

                buffer.SkipUntilNewLine();
            }

            //finally, put it all together
            GameObject obj = new GameObject(_objInfo != null ? Path.GetFileNameWithoutExtension(_objInfo.Name) : objectName ?? "CustomObject");
            obj.transform.localScale = new Vector3(-1f, 1f, 1f);

            foreach (var builder in builderDict)
            {
                //empty object
                if (builder.Value.PushedFaceCount == 0)
                    continue;

                var builtObj = builder.Value.Build();
                builtObj.transform.SetParent(obj.transform, false);
            }

            return obj;
        }

        /// <summary>
        /// Load an OBJ and MTL file from a file path.
        /// </summary>
        /// <param name="path">Input OBJ path</param>
        /// /// <param name="mtlPath">Input MTL path</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(string path, string mtlPath)
        {
            _objInfo = new FileInfo(path);
            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return Load(fs);
                }
            }
            else
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return Load(fs);
                }
            }
        }

        /// <summary>
        /// Load an OBJ file from a file path. This function will also attempt to load the MTL defined in the OBJ file.
        /// </summary>
        /// <param name="path">Input OBJ path</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(string path)
        {
            return Load(path, null);
        }
    }
}
