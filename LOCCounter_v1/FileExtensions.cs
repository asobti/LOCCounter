using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace LOCCounter_v1
{
    
    [XmlRoot("FileExtensions")]
    public class FileExtensions
    {
        [XmlElement("Script")]
        public Extension Script;

        [XmlElement("Program")]
        public Extension Program;

        [XmlElement("Other")]
        public Extension Other;

        [XmlElement("Custom")]
        public Extension Custom;
    }

    public class Extension
    {
        [XmlElement("Extension")]
        public List<string> extensions;

        public Extension()
        {
            extensions = new List<string>();
        }
    }   
}
