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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LMAStudio.StreamVR.Common;
using LMAStudio.StreamVR.Revit.EventHandlers;
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
    public class StreamVRApp : IExternalApplication
    {
        // class instance
        public static StreamVRApp Instance = null;

        // ModelessForm instance
        private WPF.StreamVRUI _uiForm;

        private readonly object requestLock = new object();
        private readonly object responseLock = new object();

        private Message _currentRequest;
        private Message _currentResponse;

        public Message CurrentRequest
        {
            get
            {
                return _currentRequest;
            }
            set
            {
                lock (requestLock)
                {
                    _currentRequest = value;
                }
            }
        }
        public Message CurrentResponse
        {
            get
            {
                return _currentResponse;
            }
            set
            {
                lock (responseLock)
                {
                    _currentResponse = value;
                }
            }
        }

        public string BaseServerURL = "streamvr.lm2.me";
        public string StartingView;

        public string NatsServerURL { get { return "nats://" + BaseServerURL + ":4222"; } }
        public string ModelServerURL { get { return "http://" + BaseServerURL + ":8080"; } }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.WriteJournalComment("[STREAMVR] APP SHUTDOWN", true);

            if (_uiForm != null && _uiForm.IsActive)
            {
                _uiForm.Close();
            }

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.WriteJournalComment("[STREAMVR] APP STARTUP", true);

            _uiForm = null;   // no dialog needed yet; the command will bring it
            Instance = this;  // static access to this application instance

            return Result.Succeeded;
        }


        // The external command invokes this on the end-user's request
        public void ShowForm(UIApplication uiapp)
        {
            uiapp.Application.WriteJournalComment("[STREAMVR] APP SHOW FORM", true);

            // If we do not have a dialog yet, create and show it
            if (_uiForm == null || !_uiForm.IsActive)
            {
                // A new handler to handle request posting by the dialog
                MessageEventHandler handler = new MessageEventHandler();

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.CreateJournalable(handler);

                uiapp.Application.WriteJournalComment("[STREAMVR] CREATING FORM", true);

                var startingViews = new FilteredElementCollector(uiapp.ActiveUIDocument.Document).
                    OfClass(typeof(View3D)).
                    Select(e => e as View3D);

                _uiForm = new WPF.StreamVRUI(exEvent, handler, (string msg) =>
                {
                    uiapp.Application.WriteJournalComment(msg, true);
                })
                {
                    ServerURL = StreamVRApp.Instance.BaseServerURL,
                    UserName = uiapp.Application.Username,
                    RoomCode = "123456",
                    StartingView = startingViews.FirstOrDefault()?.Name ?? "",
                    StartingViewOptions = startingViews.Select(v => v.Name)
                };
                _uiForm.InitialLoad();
                _uiForm.Show();
            }
        }
    }
}