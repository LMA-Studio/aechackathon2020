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

namespace LMAStudio.StreamVR.Revit.Conversions
{
    public class FamilyConverter: IConverter<Autodesk.Revit.DB.FamilySymbol>
    {
        public JObject ConvertToDTO(Autodesk.Revit.DB.FamilySymbol source)
        {
            LMAStudio.StreamVR.Common.Models.Family dest = new LMAStudio.StreamVR.Common.Models.Family
            {
                Id = source.Id.ToString(),
                Name = source.Name,
                FamilyName = source.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME)?.AsString(),
                ModelName = source.GetParameters("Product name").FirstOrDefault()?.AsString(),
                Manufacturer = source.GetParameters("Manufacturer name").FirstOrDefault()?.AsString(),
                Description = source.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)?.AsValueString(),
                Tag = source.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM)?.AsValueString(),
                URL = $"http://192.168.0.119:5000/api/model/{source.UniqueId}"
            };

            return JObject.FromObject(dest);
        }

        public void MapFromDTO(JObject sourceJSON, Autodesk.Revit.DB.FamilySymbol dest)
        {
        }

        public Autodesk.Revit.DB.FamilySymbol CreateFromDTO(Document doc, JObject source)
        {
            throw new NotImplementedException();
        }
    }
}
