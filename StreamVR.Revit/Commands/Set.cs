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

using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LMAStudio.StreamVR.Revit.Conversions;
using System;
using LMAStudio.StreamVR.Common;
using Autodesk.Revit.UI;

namespace LMAStudio.StreamVR.Revit.Commands
{
    public class Set: IBaseCommand
    {
        private readonly Action<string> _log;
        private readonly IGenericConverter _converter;
        private readonly UIDocument _uiDocument;

        public Set(Action<string> log, UIDocument uiDocument, IGenericConverter converter)
        {
            _log = log;
            _uiDocument = uiDocument;
            _converter = converter;
        }

        public Message Execute(Document doc, Message msg)
        {
            _log("EXECUTE SET");

            JObject dto = JObject.Parse(msg.Data);

            _log("GOT DTO");
            _log(JsonConvert.SerializeObject(dto));

            _log("GETTING ELEMENT");

            Element dbValue = doc.GetElement(new ElementId(Int32.Parse(dto["Id"].ToString())));

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Set Element");

                // Ensure warnings are suppressed and failures roll-back
                var failureOptions = tx.GetFailureHandlingOptions();
                failureOptions.SetFailuresPreprocessor(new WarningSupressor());
                tx.SetFailureHandlingOptions(failureOptions);
                _log($"GOT FAMILY INSTANCE {dbValue?.Id.ToString()}");

                // Map dto values to DB
                _converter.MapFromDTO(dto, dbValue);

                tx.Commit();
            }

            _log($"MAPPED FAMILY INSTANCE");

            dto = _converter.ConvertToDTO(dbValue);

            _log("NEW VALUE");
            _log(JsonConvert.SerializeObject(dto));

            _uiDocument.RefreshActiveView();

            return new Message
            {
                Type = "VALUE",
                Data = JsonConvert.SerializeObject(dto)
            };
        }
    }
}
