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

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LMAStudio.StreamVR.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class Ribbon : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            String tabName = "StreamVR";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            { }

            List<RibbonPanel> allRibbonPanels = application.GetRibbonPanels(tabName);

            RibbonPanel intElevPanel = null;
            foreach (RibbonPanel rp in allRibbonPanels)
            {
                if (rp.Name == "Stream Actions")
                {
                    intElevPanel = rp;
                    break;
                }
            }

            if (intElevPanel == null)
            {
                intElevPanel = application.CreateRibbonPanel(tabName, "Stream Actions");
            }

            AddStreamButton(intElevPanel, application);
            AddExportButton(intElevPanel, application);
            AddExportMaterialsButton(intElevPanel, application);

            return Result.Succeeded;

        }

        private void AddStreamButton(RibbonPanel intElevPanel, UIControlledApplication app)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyDir = new FileInfo(assembly.Location).Directory.FullName;
            var assemblyName = $"{assemblyDir}\\LMAStudio.StreamVR.Revit.dll";

            PushButtonData intElevButtonData = new PushButtonData("Begin Streaming", "Begin Streaming", assemblyName, "LMAStudio.StreamVR.Revit.StreamingServer");
            PushButton placeIntElevButton = intElevPanel.AddItem(intElevButtonData) as PushButton;

            placeIntElevButton.ToolTip = "Automatically places interior elevations into all bound rooms";
        }

        private void AddExportButton(RibbonPanel intElevPanel, UIControlledApplication app)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyDir = new FileInfo(assembly.Location).Directory.FullName;
            var assemblyName = $"{assemblyDir}\\LMAStudio.StreamVR.Revit.dll";

            PushButtonData intElevButtonData = new PushButtonData("Export Families", "Export Families", assemblyName, "LMAStudio.StreamVR.Revit.ExportFamilies");
            PushButton placeIntElevButton = intElevPanel.AddItem(intElevButtonData) as PushButton;

            placeIntElevButton.ToolTip = "Exports familes to the model server as OBJs";
        }

        private void AddExportMaterialsButton(RibbonPanel intElevPanel, UIControlledApplication app)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyDir = new FileInfo(assembly.Location).Directory.FullName;
            var assemblyName = $"{assemblyDir}\\LMAStudio.StreamVR.Revit.dll";

            PushButtonData intElevButtonData = new PushButtonData("Export Materials", "Export Materials", assemblyName, "LMAStudio.StreamVR.Revit.ExportMaterials");
            PushButton placeIntElevButton = intElevPanel.AddItem(intElevButtonData) as PushButton;

            placeIntElevButton.ToolTip = "Exports basic material data to the model server";
        }

    }
}