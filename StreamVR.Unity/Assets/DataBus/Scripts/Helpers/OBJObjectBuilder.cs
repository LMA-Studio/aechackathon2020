using System.Collections.Generic;
using UnityEngine;

namespace LMAStudio.StreamVR.Unity.Helpers
{
    public class OBJObjectBuilder
    {
        public int PushedFaceCount { get; private set; } = 0;

        //stuff passed in by ctor
        private OBJLoader _loader;
        private string _name;

        private Dictionary<ObjLoopHash, int> _globalIndexRemap = new Dictionary<ObjLoopHash, int>();
        private Dictionary<string, List<int>> _materialIndices = new Dictionary<string, List<int>>();
        private List<int> _currentIndexList;
        private string _lastMaterial = null;

        //our local vert/normal/uv
        private List<Vector3> _vertices = new List<Vector3>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<Vector2> _uvs = new List<Vector2>();

        //this will be set if the model has no normals or missing normal info
        private bool recalculateNormals = false;

        /// <summary>
        /// Loop hasher helper class
        /// </summary>
        private class ObjLoopHash
        {
            public int vertexIndex;
            public int normalIndex;
            public int uvIndex;

            public override bool Equals(object obj)
            {
                if (!(obj is ObjLoopHash))
                    return false;

                var hash = obj as ObjLoopHash;
                return (hash.vertexIndex == vertexIndex) && (hash.uvIndex == uvIndex) && (hash.normalIndex == normalIndex);
            }

            public override int GetHashCode()
            {
                int hc = 3;
                hc = unchecked(hc * 314159 + vertexIndex);
                hc = unchecked(hc * 314159 + normalIndex);
                hc = unchecked(hc * 314159 + uvIndex);
                return hc;
            }
        }

        public GameObject Build(bool hasLight)
        {
            var go = new GameObject(_name);

            Scripts.FamilyGeometryController geoController = go.AddComponent<Scripts.FamilyGeometryController>();

            //add meshrenderer
            var mr = go.AddComponent<MeshRenderer>();
            int submesh = 0;
            
            //locate the material for each submesh
            Material[] materialArray = new Material[_materialIndices.Count];
            foreach (var kvp in _materialIndices)
            {
                Material material = Logic.MaterialLibrary.LookupMaterial(kvp.Key);

                if (hasLight && material != null)
                {
                    if (material.name.Contains("Glass") || material.name.Contains("Bulb"))
                    {
                        geoController.NighttimeMaterial = (Material)Resources.Load("Glow", typeof(Material));
                    }
                    else if (material.name.Contains("Lampscreen"))
                    {
                        geoController.NighttimeMaterial = (Material)Resources.Load("GlowShade", typeof(Material));
                    }
                }

                if (material == null)
                {
                    material = CreateNullMaterial();
                    material.name = kvp.Key;
                }

                if (geoController.NighttimeMaterial != null)
                {
                    geoController.DaytimeMaterial = material;
                }

                materialArray[submesh] = material;
                submesh++;
            }
            mr.sharedMaterials = materialArray;

            //add meshfilter
            var mf = go.AddComponent<MeshFilter>();
            submesh = 0;

            var msh = new Mesh()
            {
                name = _name,
                indexFormat = (_vertices.Count > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16,
                subMeshCount = _materialIndices.Count
            };

            //set vertex data
            msh.SetVertices(_vertices);
            msh.SetNormals(_normals);
            msh.SetUVs(0, _uvs);

            //set faces
            foreach (var kvp in _materialIndices)
            {
                msh.SetTriangles(kvp.Value, submesh);
                submesh++;
            }

            //recalculations
            if (recalculateNormals)
                msh.RecalculateNormals();
            msh.RecalculateTangents();
            msh.RecalculateBounds();

            mf.sharedMesh = msh;

            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = msh;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            if (geoController.NighttimeMaterial != null)
            {
                Logic.FamilyInstanceLibrary.AddLight(go);
            }

            //
            return go;
        }

        public void SetMaterial(string name)
        {
            if (!_materialIndices.TryGetValue(name, out _currentIndexList))
            {
                _currentIndexList = new List<int>();
                _materialIndices[name] = _currentIndexList;
            }
        }

        public void PushFace(List<int> vertexIndices, List<int> normalIndices, List<int> uvIndices, string materialId)
        {
            //invalid face size?
            if (vertexIndices.Count < 3)
            {
                return;
            }

            //set material
            if (_lastMaterial == null)
            {
                SetMaterial(materialId);
                _lastMaterial = materialId;
            }

            //remap
            int[] indexRemap = new int[vertexIndices.Count];
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                int vertexIndex = vertexIndices[i];
                int normalIndex = normalIndices[i];
                int uvIndex = uvIndices[i];

                var hashObj = new ObjLoopHash()
                {
                    vertexIndex = vertexIndex,
                    normalIndex = normalIndex,
                    uvIndex = uvIndex
                };
                int remap = -1;

                if (!_globalIndexRemap.TryGetValue(hashObj, out remap))
                {
                    //add to dict
                    _globalIndexRemap.Add(hashObj, _vertices.Count);
                    remap = _vertices.Count;

                    //add new verts and what not
                    _vertices.Add((vertexIndex >= 0 && vertexIndex < _loader.Vertices.Count) ? _loader.Vertices[vertexIndex] : Vector3.zero);
                    _normals.Add((normalIndex >= 0 && normalIndex < _loader.Normals.Count) ? _loader.Normals[normalIndex] : Vector3.zero);
                    _uvs.Add((uvIndex >= 0 && uvIndex < _loader.UVs.Count) ? _loader.UVs[uvIndex] : Vector2.zero);

                    //mark recalc flag
                    if (normalIndex < 0)
                    {
                        recalculateNormals = true;
                    }
                }

                indexRemap[i] = remap;
            }


            //add face to our mesh list
            if (indexRemap.Length == 3)
            {
                _currentIndexList.AddRange(new int[] { indexRemap[0], indexRemap[1], indexRemap[2] });
            }
            else if (indexRemap.Length == 4)
            {
                _currentIndexList.AddRange(new int[] { indexRemap[0], indexRemap[1], indexRemap[2] });
                _currentIndexList.AddRange(new int[] { indexRemap[2], indexRemap[3], indexRemap[0] });
            }
            else if (indexRemap.Length > 4)
            {
                for (int i = indexRemap.Length - 1; i >= 2; i--)
                {
                    _currentIndexList.AddRange(new int[] { indexRemap[0], indexRemap[i - 1], indexRemap[i] });
                }
            }

            PushedFaceCount++;
        }

        public OBJObjectBuilder(string name, OBJLoader loader)
        {
            _name = name;
            _loader = loader;
        }

        private Material CreateNullMaterial()
        {
            return new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
    }
}
