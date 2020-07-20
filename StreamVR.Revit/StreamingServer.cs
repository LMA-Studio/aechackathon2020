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

    public class StreamingServer : IExternalCommand
    {
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                commandData.Application.Application.WriteJournalComment("[STREAMVR] EXECUTING STREAMING SERVER", true);
                StreamVRApp.Instance.ShowForm(commandData.Application);
                commandData.Application.Application.WriteJournalComment("[STREAMVR] EXECUTED STREAMING SERVER!", true);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}