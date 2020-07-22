using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LMAStudio.StreamVR.Common;
using LMAStudio.StreamVR.Revit.Commands;
using LMAStudio.StreamVR.Revit.Conversions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LMAStudio.StreamVR.Revit.EventHandlers
{
    public class MessageEventHandler : IExternalEventHandler
    {
        private IGenericConverter Converter;
        private IBaseCommand Command_GetAll;
        private IBaseCommand Command_Get;
        private IBaseCommand Command_Set;
        private IBaseCommand Command_Paint;
        private IBaseCommand Command_Create;
        private IBaseCommand Command_Export;
        private IBaseCommand Command_Delete;

        private Application application;

        private void Debug(string msg)
        {
            this.application.WriteJournalComment(msg, true);
        }

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;
            this.application = app.Application;

            Debug("[STREAMVR] EXECUTING STREAMING SERVER EVENT!1!!");

            this.Converter = new GenericConverter(Debug);
            this.Command_GetAll = new GetAll(Debug, this.Converter);
            this.Command_Get = new Get(Debug, this.Converter);
            this.Command_Set = new Set(Debug, uiDoc, this.Converter);
            this.Command_Paint = new Paint(Debug, this.Converter);
            this.Command_Create = new Create(Debug, this.Converter);
            this.Command_Export = new Export(Debug, this.Converter);
            this.Command_Delete = new Delete(Debug, this.Converter);

            Message request = StreamVRApp.Instance.CurrentRequest;

            Debug("[STREAMVR] REQUEST: " + JsonConvert.SerializeObject(request));

            Message response = HandleClientRequest(doc, request);

            StreamVRApp.Instance.CurrentResponse = response;
        }

        public string GetName()
        {
            return "StreamVR Message Handler";
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

        private Message GetOrientation(Document doc)
        {
            string name = StreamVRApp.Instance.StartingView;
            View3D startingView = new FilteredElementCollector(doc).
                OfClass(typeof(View3D)).
                Select(e => e as View3D).
                FirstOrDefault(v => v.Name == name);

            return new Message
            {
                Type = "ORIENTATION",
                Data = JsonConvert.SerializeObject(this.Converter.ConvertToDTO(startingView))
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
                    case "DELETE":
                        return this.Command_Delete.Execute(doc, msg);
                    case "EXPORT":
                        return this.Command_Export.Execute(doc, msg);
                    case "EXPORT_ALL":
                        return ExportAll(doc, msg);
                    case "GET_ORIENTATION":
                        return GetOrientation(doc);
                }
            }
            catch (Exception e)
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
