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

using System;
using System.Collections.Generic;
using System.Text;

namespace LMAStudio.StreamVR.Common.Models
{
    public class Family: Element
    {
        public string FamilyName { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public string FamilyId { get; set; }

        public XYZ BoundingBoxMin { get; set; }
        public XYZ BoundingBoxMax { get; set; }
    }
}
