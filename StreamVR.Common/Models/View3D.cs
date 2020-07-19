using System;
using System.Collections.Generic;
using System.Text;

namespace LMAStudio.StreamVR.Common.Models
{
    public class View3D: Element
    {
        public XYZ Position { get; set; }
        public XYZ ForwardDirection { get; set; }
        public XYZ UpDirection { get; set; }
    }
}
