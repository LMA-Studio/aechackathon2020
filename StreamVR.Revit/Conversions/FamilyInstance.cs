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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using LMAStudio.StreamVR.Common.Models;
using Autodesk.Revit.DB.Structure;
using Newtonsoft.Json;

namespace LMAStudio.StreamVR.Revit.Conversions
{
    public class FamilyInstanceConverter: IConverter<Autodesk.Revit.DB.FamilyInstance>
    {
        public JObject ConvertToDTO(Autodesk.Revit.DB.FamilyInstance source)
        {
            Autodesk.Revit.DB.FamilySymbol fam = source.Symbol;

            BoundingBoxXYZ bb = source.get_BoundingBox(null);
            Autodesk.Revit.DB.Transform trans = source.GetTransform();

            var info = source.GetOrderedParameters().Where(p => p.Definition.ParameterGroup == BuiltInParameterGroup.PG_GEOMETRY);
            Dictionary<string, string> infoDict = new Dictionary<string, string>();
            foreach (var i in info)
            {
                infoDict[i.Definition.Name] = i.AsValueString();
            }
            int geometryHash = JsonConvert.SerializeObject(infoDict).GetHashCode();

            LMAStudio.StreamVR.Common.Models.FamilyInstance dest = new LMAStudio.StreamVR.Common.Models.FamilyInstance
            {
                Id = source.Id.ToString(),
                Name = source.Name,
                HostId = source.Host?.Id.ToString(),
                FamilyId = fam.Id.ToString(),
                VariantId = geometryHash.ToString(),
                SubComponents = source.GetSubComponentIds().Select(e => e.ToString()),
                SuperComponent = source.SuperComponent?.Id.ToString(),
                //BoundingBoxMin = new LMAStudio.StreamVR.Common.Models.XYZ
                //{
                //    X = bb?.Min.X ?? 0,
                //    Y = bb?.Min.Y ?? 0,
                //    Z = bb?.Min.Z ?? 0,
                //},
                //BoundingBoxMax = new LMAStudio.StreamVR.Common.Models.XYZ
                //{
                //    X = bb?.Max.X ?? 0,
                //    Y = bb?.Max.Y ?? 0,
                //    Z = bb?.Max.Z ?? 0,
                //},
                IsFlipped = source.FacingFlipped,
                FacingOrientation = new LMAStudio.StreamVR.Common.Models.XYZ
                {
                    X = source.FacingOrientation.X,
                    Y = source.FacingOrientation.Y,
                    Z = source.FacingOrientation.Z,
                },
                IsHandFlipped = source.HandFlipped,
                HandOrientation = new LMAStudio.StreamVR.Common.Models.XYZ
                {
                    X = source.HandOrientation.X,
                    Y = source.HandOrientation.Y,
                    Z = source.HandOrientation.Z,
                },
                Transform = new LMAStudio.StreamVR.Common.Models.Transform
                {
                    Scale = trans.Scale,
                    Origin = new LMAStudio.StreamVR.Common.Models.XYZ
                    {
                        X = trans.Origin.X,
                        Y = trans.Origin.Y,
                        Z = trans.Origin.Z,
                    },
                    BasisX = new LMAStudio.StreamVR.Common.Models.XYZ
                    {
                        X = trans.BasisX.X,
                        Y = trans.BasisX.Y,
                        Z = trans.BasisX.Z,
                    },
                    BasisY = new LMAStudio.StreamVR.Common.Models.XYZ
                    {
                        X = trans.BasisY.X,
                        Y = trans.BasisY.Y,
                        Z = trans.BasisY.Z,
                    },
                    BasisZ = new LMAStudio.StreamVR.Common.Models.XYZ
                    {
                        X = trans.BasisZ.X,
                        Y = trans.BasisZ.Y,
                        Z = trans.BasisZ.Z,
                    },
                }
            };

            return JObject.FromObject(dest);
        }

        public void MapFromDTO(JObject sourceJSON, Autodesk.Revit.DB.FamilyInstance dest)
        {
            Autodesk.Revit.DB.Transform trans = dest.GetTransform();

            Common.Models.FamilyInstance source = sourceJSON.ToObject<Common.Models.FamilyInstance>();

            Autodesk.Revit.DB.XYZ newOrigin = new Autodesk.Revit.DB.XYZ(source.Transform.Origin.X, source.Transform.Origin.Y, source.Transform.Origin.Z);

            double oldAngle = Math.Atan2(trans.BasisX.Y, trans.BasisX.X);
            double newAngle = Math.Atan2(source.Transform.BasisX.Y, source.Transform.BasisX.X);

            Autodesk.Revit.DB.XYZ translation = newOrigin - trans.Origin;
            double rotation = newAngle - oldAngle;

            Autodesk.Revit.DB.XYZ a1 = trans.Origin;
            Autodesk.Revit.DB.XYZ a2 = new Autodesk.Revit.DB.XYZ(a1.X, a1.Y, a1.Z + 10);
            Line axis = Line.CreateBound(a1, a2);

            dest.Location.Rotate(axis, rotation);
            dest.Location.Move(translation);
        }

        public Autodesk.Revit.DB.FamilyInstance CreateFromDTO(Document doc, JObject source)
        {
            Common.Models.FamilyInstance famI = source.ToObject<Common.Models.FamilyInstance>();

            Autodesk.Revit.DB.XYZ newOrigin = new Autodesk.Revit.DB.XYZ(famI.Transform.Origin.X, famI.Transform.Origin.Y, famI.Transform.Origin.Z);

            Autodesk.Revit.DB.FamilySymbol fam = doc.GetElement(new ElementId(Int32.Parse(famI.FamilyId))) as Autodesk.Revit.DB.FamilySymbol;
            if (fam == null)
            {
                throw new Exception($"Failed to find family with id {famI.FamilyId}");
            }

            Autodesk.Revit.DB.FamilyInstance newFamily;
            if (famI.HostId != null)
            {
                ElementId hostId = new ElementId(Int32.Parse(famI.HostId));
                newFamily = doc.Create.NewFamilyInstance(
                    newOrigin,
                    fam,
                    doc.GetElement(hostId),
                    StructuralType.NonStructural
                );
            }
            else
            {
                newFamily = doc.Create.NewFamilyInstance(
                    newOrigin,
                    fam,
                    StructuralType.NonStructural
                );
            }

            Autodesk.Revit.DB.Transform trans = newFamily.GetTransform();

            double rotation = Math.Atan2(famI.Transform.BasisX.Y, famI.Transform.BasisX.X);

            Autodesk.Revit.DB.XYZ a1 = newOrigin;
            Autodesk.Revit.DB.XYZ a2 = new Autodesk.Revit.DB.XYZ(a1.X, a1.Y, a1.Z + 10);
            Line axis = Line.CreateBound(a1, a2);

            newFamily.Location.Rotate(axis, rotation);

            return newFamily;
        }

    }
}
