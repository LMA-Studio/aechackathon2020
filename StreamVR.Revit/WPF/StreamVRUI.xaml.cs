using Autodesk.Revit.UI;
using LMAStudio.StreamVR.Common;
using LMAStudio.StreamVR.Revit.EventHandlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LMAStudio.StreamVR.Revit.WPF
{
    public partial class StreamVRUI : Window
    {
        public string ServerURL { get; set; }
        public string UserName { get; set; }
        public string RoomCode { get; set; }
        public string StartingView { get; set; }
        public IEnumerable<string> StartingViewOptions { get; set; }

        private static Queue<Message> msgQueue = new Queue<Message>();
        private Action<string> _log;

        private ExternalEvent _exEvent;
        private MessageEventHandler _handler;

        public StreamVRUI(ExternalEvent exEvent, MessageEventHandler handler, Action<string> debug)
        {
            _log = debug;

            InitializeComponent();
            _exEvent = exEvent;
            _handler = handler;

            var assembly = Assembly.GetCallingAssembly();
            var assemblyDir = new FileInfo(assembly.Location).Directory.FullName;
            var assemblyName = $"{assemblyDir}\\StreamVRLogo.png";
            var uri = new Uri(assemblyName);
            var bitmap = new BitmapImage(uri);
            StreamVRIcon.Source = bitmap;
        }

        public void InitialLoad()
        {
            this.AssignValues();
            this.SetVisibility(false);
        }

        private void ListenForMessages()
        {
            Task.Run(() =>
            {
                using (var cc = new Communicator(StreamVRApp.Instance.NatsServerURL, this.UserName, this.RoomCode, _log))
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

                            _log("[STREAMVR] Next msg");
                            _log(JsonConvert.SerializeObject(msg));

                            if (msg.Reply != null)
                            {
                                StreamVRApp.Instance.CurrentRequest = msg;

                                _exEvent.Raise();

                                while(StreamVRApp.Instance.CurrentResponse == null)
                                {
                                    Thread.Sleep(50);
                                }

                                Message response = StreamVRApp.Instance.CurrentResponse;

                                if (response.Type == "ERROR")
                                {
                                    _log($"[STREAMVR] Error response");
                                    _log(JsonConvert.SerializeObject(response));
                                }

                                _log($"[STREAMVR] Replying to request");

                                response.Reply = msg.Reply;
                                cc.Publish(msg.Reply, response);

                                StreamVRApp.Instance.CurrentResponse = null;
                            }
                            else if (msg.Type == "EXIT")
                            {
                                _log("Exit command received");
                                _shutdown = true;
                            }
                        }
                        Thread.Sleep(100);
                    }
                }
            });
        }

        public void AssignValues()
        {
            if (!string.IsNullOrEmpty(ServerURL))
            {
                txtbx_serverurl.Text = ServerURL;
            }
            if (!string.IsNullOrEmpty(UserName))
            {
                txtbx_username.Text = UserName;
            }
            if (!string.IsNullOrEmpty(RoomCode))
            {
                txtbx_roomcode.Text = RoomCode;
            }

            if (!string.IsNullOrEmpty(StartingView))
            {
                cbx_startingview.SelectedItem = StartingView;
            }

            if (StartingViewOptions != null)
            {
                foreach (var o in StartingViewOptions)
                {
                    cbx_startingview.Items.Add(o);
                }
            }
        }

        private void SetVisibility(bool isStreaming)
        {
            if (isStreaming)
            {
                txtbx_serverurl.IsEnabled = false;
                txtbx_username.IsEnabled = false;
                txtbx_roomcode.IsEnabled = false;
                cbx_startingview.IsEnabled = false;

                StopStreamText1.Visibility = Visibility.Visible;
                StopStreamText2.Visibility = Visibility.Visible;
                StreamButton.Visibility = Visibility.Hidden;
                CancelButton.Visibility = Visibility.Hidden;
            }
            else
            {
                txtbx_serverurl.IsEnabled = true;
                txtbx_username.IsEnabled = false;
                txtbx_roomcode.IsEnabled = true;
                cbx_startingview.IsEnabled = true;

                StopStreamText1.Visibility = Visibility.Hidden;
                StopStreamText2.Visibility = Visibility.Hidden;
                StreamButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        public void StartStream(object sender, RoutedEventArgs e)
        {
            ServerURL = txtbx_serverurl.Text;
            RoomCode = txtbx_roomcode.Text;
            StartingView = cbx_startingview.SelectedValue as string;

            StreamVRApp.Instance.BaseServerURL = ServerURL;
            StreamVRApp.Instance.StartingView = StartingView;

            this.SetVisibility(true);

            this.ListenForMessages();
        }

        public void CancelStream(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void ShutdownStream(object sender, RoutedEventArgs e)
        {
            msgQueue.Enqueue(new Message()
            {
                Type = "EXIT"
            });
        }
    }
}
