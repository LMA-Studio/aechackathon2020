using LMAStudio.StreamVR.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class HostController: MonoBehaviour
    {
        public Element InstanceData { get; private set; }

        public void UpdateInstanceData(Element e)
        {
            InstanceData = e;
        }
    }
}
