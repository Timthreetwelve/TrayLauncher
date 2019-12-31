//
// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
//

using System.Collections.Generic;
using System.Xml.Serialization;

namespace TrayLauncher
{
    public class SpecialMenuItems
    {
        [XmlElement("Shortcut")]
        public List<Shortcut> shortcuts = new List<Shortcut>();
    }

    public class Shortcut
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Path")]
        public string Path { get; set; }

        [XmlElement("Args")]
        public string Args { get; set; }

        [XmlElement("Type")]
        public string ItemType { get; set; }
    }
}
