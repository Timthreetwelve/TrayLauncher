// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Xml.Serialization;
using System.Collections.Generic;

namespace TrayLauncher
{
    // Class to define MenuList
    public class MenuList
    {
        [XmlElement("TLMenuItem")]
        public List<TLMenuItem> menuList = new List<TLMenuItem>();
    }


    public class TLMenuItem
    {
        [XmlElement("Position")]
        public int Pos { get; set; }

        [XmlElement("MenuHeader")]
        public string Header { get; set; }

        [XmlElement("AppPath")]
        public string AppPath { get; set; }

        [XmlElement("Arguments")]
        public string Arguments { get; set; }

        [XmlElement("ToolTip")]
        public string ToolTip { get; set; }

        [XmlElement("Type")]
        public string ItemType { get; set; }
    }
}
