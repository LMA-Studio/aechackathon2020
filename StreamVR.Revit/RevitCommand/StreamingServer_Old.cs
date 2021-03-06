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

    public class StreamingServer_old : IExternalCommand
    {
        private string serverUrl;
        private string userName;
        private string roomCode;
        private View3D startingView;
        private IEnumerable<View3D> startingViews;

        private static EventWaitHandle _closeUIWaitHandle;

        private IGenericConverter Converter;
        private IBaseCommand Command_GetAll;
        private IBaseCommand Command_Get;
        private IBaseCommand Command_Set;
        private IBaseCommand Command_Paint;
        private IBaseCommand Command_Create;
        private IBaseCommand Command_Export;

        private static Queue<Message> msgQueue = new Queue<Message>();

        private Application application;

        private void Debug(string msg)
        {
            this.application.WriteJournalComment(msg, true);
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            this.application = commandData.Application.Application;

            this.Converter = new GenericConverter(Debug);
            this.Command_GetAll = new GetAll(Debug, this.Converter);
            this.Command_Get = new Get(Debug, this.Converter);
            this.Command_Set = new Set(Debug, uiDoc, this.Converter);
            this.Command_Paint = new Paint(Debug, this.Converter);
            this.Command_Create = new Create(Debug, this.Converter);
            this.Command_Export = new Export(Debug, this.Converter);

            this.serverUrl = "192.168.0.119:7002";
            this.userName = this.application.Username;
            this.roomCode = "123456";
            this.startingViews = GetView3Ds(doc);
            this.startingView = this.startingViews.FirstOrDefault(e => e.Name == "3D View 3");

            Debug("SERVER CONN");
            Debug(serverUrl);
            Debug(userName);
            Debug(roomCode);

            StreamVRUI startUI = CreateUI(false);
            bool? start = startUI.ShowDialog();
            if (start != true)
            {
                return Result.Cancelled;
            }

            this.serverUrl = startUI.ServerURL;
            this.roomCode = startUI.RoomCode;
            this.startingView = this.startingViews.FirstOrDefault(v => v.Name == startUI.StartingView);

            this.CreateShutdownForm();

            this.ListenForMessages(uiDoc);

            return Result.Succeeded;
        }

        private void CreateShutdownForm()
        {
            using (_closeUIWaitHandle = new AutoResetEvent(false))
            {
                Thread closeUIThread = new Thread(() =>
                {
                    StreamVRUI closeUI = CreateUI(true);
                    closeUI.Show();
                    
                    // On close, enqueue exit message
                    closeUI.Closed += (object sender, EventArgs args) =>
                    {
                        msgQueue.Enqueue(new Message()
                        {
                            Type = "EXIT"
                        });
                    };

                    _closeUIWaitHandle.Set();
                    System.Windows.Threading.Dispatcher.Run();
                });

                closeUIThread.SetApartmentState(ApartmentState.STA);
                closeUIThread.IsBackground = true;
                closeUIThread.Start();

                _closeUIWaitHandle.WaitOne();
            }
        }

        private StreamVRUI CreateUI(bool isStreaming)
        {
            StreamVRUI ui = new StreamVRUI()
            {
                ServerURL = this.serverUrl,
                UserName = this.userName,
                RoomCode = this.roomCode,
                StartingView = this.startingView.Name,
                StartingViewOptions = this.startingViews.Select(v => v.Name)
            };

            ui.AssignValues(isStreaming);

            return ui;
        }

        private void ListenForMessages(UIDocument uiDoc)
        {
            using (var cc = new Communicator(this.serverUrl, this.userName, this.roomCode, this.Debug))
            {
                cc.Connect();
                cc.Subscribe(cc.TO_SERVER_CHANNEL, (Message msg) =>
                {
                    msgQueue.Enqueue(msg);
                });

                bool _shutdown = false;
                while (!_shutdown)
                {
                    if (msgQueue.Count > 0)
                    {
                        Message msg = msgQueue.Dequeue();

                        Debug(JsonConvert.SerializeObject(msg));

                        if (msg.Reply != null)
                        {
                            Message response = HandleClientRequest(uiDoc.Document, msg);
                            response.Reply = msg.Reply;
                            cc.Publish(msg.Reply, response);
                        }
                        else if (msg.Type == "EXIT")
                        {
                            Debug("Exit command received");
                            _shutdown = true;
                        }
                    }

                    uiDoc.RefreshActiveView();
                    Thread.Sleep(200);
                }
            }
        }

        private IEnumerable<View3D> GetView3Ds(Document doc)
        {
            return new FilteredElementCollector(doc).
                OfClass(typeof(View3D)).
                Select(e => e as View3D);
        }

        private Message ExportAll(Document doc, Message msg)
        {
            IEnumerable<string> families = new FilteredElementCollector(doc).
                            OfClass(typeof(FamilySymbol)).
                            Select(e => e.Id.ToString());

            int success = 0;
            int failure = 0;

            Debug("EXPORTING " + families.Count() + " FAMILIES");

            foreach (string id in families)
            {
                try
                {
                    Message response = this.Command_Export.Execute(doc, new Message
                    {
                        Type = "EXPORT",
                        Reply = null,
                        Data = JsonConvert.SerializeObject(new
                        {
                            Id = id // "56182"
                        })
                    });

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
                Failures = failure
            }));

            return new Message
            {
                Type = "EXPORT_RESULTS",
                Data = JsonConvert.SerializeObject(new
                {
                    Successes = success,
                    Failures = failure
                })
            };
        }

        private Message HandleClientRequest(Document doc, Message msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case "GET_ALL":
                        return this.Command_GetAll.Execute(doc, msg);
                    case "GET":
                        return this.Command_Get.Execute(doc, msg);
                    case "SET":
                        return this.Command_Set.Execute(doc, msg);
                    case "PAINT":
                        return this.Command_Paint.Execute(doc, msg);
                    case "CREATE":
                        return this.Command_Create.Execute(doc, msg);
                    case "EXPORT":
                        return this.Command_Export.Execute(doc, msg);
                    case "EXPORT_ALL":
                        return ExportAll(doc, msg);
                    case "GET_ORIENTATION":
                        return new Message
                        {
                            Type = "ORIENTATION",
                            Data = JsonConvert.SerializeObject(this.Converter.ConvertToDTO(this.startingView))
                        };
                }
            }
            catch(Exception e)
            {
                return new Message
                {
                    Type = "ERROR",
                    Data = JsonConvert.SerializeObject(new
                    {
                        Msg = e.Message,
                        Stack = e.StackTrace
                    })
                };
            }

            return new Message
            {
                Type = "NULL",
                Data = null
            };
        }
    }
}