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

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Newtonsoft.Json;

using LMAStudio.StreamVR.Common;
using LMAStudio.StreamVR.Revit.Commands;
using LMAStudio.StreamVR.Revit.Conversions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMAStudio.StreamVR.Revit.WPF;
using System.Threading;

namespace LMAStudio.StreamVR.Revit
{
    [Transaction(TransactionMode.Manual)]

    public class ExportMaterials : IExternalCommand
    {
        private IGenericConverter Converter;
        private Application application;

        private void Debug(string msg)
        {
            this.application.WriteJournalComment(msg, true);

        }
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            this.application = commandData.Application.Application;

            Debug("[STREAMVR] EXECUTING STREAMING SERVER EVENT!1!!");

            this.Converter = new GenericConverter(Debug);

            IBaseCommand command = new ExportMaterial(Debug, this.Converter);

            IEnumerable<string> materials = new FilteredElementCollector(doc).
                            OfClass(typeof(Material)).
                            Select(e => e.Id.ToString());

            int success = 0;
            int noMaterial = 0;
            int failure = 0;

            Debug("EXPORTING " + materials.Count() + " MATERIALS");

            foreach (string id in materials)
            {
                try
                {
                    Message response = command.Execute(doc, new Message
                    {
                        Type = "EXPORT_MATERIAL",
                        Reply = null,
                        Data = JsonConvert.SerializeObject(new
                        {
                            Id = id
                        })
                    });

                    if (response.Type == "EMPTY")
                    {
                        noMaterial++;
                        continue;
                    }

                    if (response.Type == "ERROR")
                    {
                        throw new Exception(response.Data);
                    }

                    Debug(response.Data);
                    success++;
                }
                catch (Exception e)
                {
                    Debug($"Error 2 {id} {e.ToString()}");
                    failure++;
                }
            }

            Debug("EXPORT RESULTS " + JsonConvert.SerializeObject(new
            {
                Successes = success,
                NoExportable = noMaterial,
                Failures = failure
            }));

            TaskDialog mainDialog = new TaskDialog("Export Report");
            mainDialog.MainInstruction = "Export Report";
            mainDialog.MainContent = $"Successes: {success}\nNot Exportable: {noMaterial}\nFailures: {failure}";
            TaskDialogResult tResult = mainDialog.Show();

            return Result.Succeeded;
        }
    }
}