using LMAStudio.StreamVR.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class BusConnector
    {
        private static string natsEndpoint = "192.168.0.119:7002";
        private static string username = "lisamarie.mueller";
        private static string roomCode = "123456";

        public static void ConfigureEndpoint(string endpoint)
        {
            BusConnector.natsEndpoint = endpoint;
        }

        public static void ConfigureRoom(string username, string roomCode)
        {
            BusConnector.username = username;
            BusConnector.roomCode = roomCode;
        }

        public static ICommunicator Connect()
        {
            ICommunicator comms = new Communicator(BusConnector.natsEndpoint, BusConnector.username, BusConnector.roomCode, Debug.Log);

            Debug.Log("Connecting...");

            comms.Connect();

            Debug.Log("Connected");
            
            Debug.Log($"> {natsEndpoint}");
            Debug.Log($"> {username}");
            Debug.Log($"> {roomCode}");

            return comms;
        }
    }
}
