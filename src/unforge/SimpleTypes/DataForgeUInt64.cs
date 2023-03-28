﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace unforge
{
    public class DataForgeUInt64 : DataForgeSerializable
    {
        public UInt64 Value { get; set; }

        public DataForgeUInt64(DataForge documentRoot)
            : base(documentRoot)
        {
            this.Value = this._br.ReadUInt64();
        }

        public override String ToString()
        {
            return String.Format("{0}", this.Value);
        }

        public XmlElement Read()
        {
            var element = this.DocumentRoot.CreateElement("UInt64");
            var attribute = this.DocumentRoot.CreateAttribute("value");
            attribute.Value = this.Value.ToString();
            element.Attributes.Append(attribute);
            return element;
        }
    }
}
